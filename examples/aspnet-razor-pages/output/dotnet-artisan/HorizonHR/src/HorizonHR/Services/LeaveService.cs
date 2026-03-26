using HorizonHR.Data;
using HorizonHR.Models;
using Microsoft.EntityFrameworkCore;

namespace HorizonHR.Services;

public sealed class LeaveService(ApplicationDbContext db, ILogger<LeaveService> logger) : ILeaveService
{
    public async Task<PaginatedList<LeaveRequest>> GetAllRequestsAsync(int pageNumber, int pageSize,
        LeaveRequestStatus? status = null, int? employeeId = null, int? leaveTypeId = null,
        CancellationToken ct = default)
    {
        var query = db.LeaveRequests
            .AsNoTracking()
            .Include(lr => lr.Employee)
            .Include(lr => lr.LeaveType)
            .Include(lr => lr.ReviewedBy)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(lr => lr.Status == status.Value);
        }
        if (employeeId.HasValue)
        {
            query = query.Where(lr => lr.EmployeeId == employeeId.Value);
        }
        if (leaveTypeId.HasValue)
        {
            query = query.Where(lr => lr.LeaveTypeId == leaveTypeId.Value);
        }

        query = query.OrderByDescending(lr => lr.SubmittedDate);
        return await PaginatedList<LeaveRequest>.CreateAsync(query, pageNumber, pageSize, ct);
    }

    public async Task<LeaveRequest?> GetRequestByIdAsync(int id, CancellationToken ct = default)
    {
        return await db.LeaveRequests
            .Include(lr => lr.Employee).ThenInclude(e => e.Department)
            .Include(lr => lr.LeaveType)
            .Include(lr => lr.ReviewedBy)
            .FirstOrDefaultAsync(lr => lr.Id == id, ct);
    }

    public async Task<string?> SubmitRequestAsync(LeaveRequest request, CancellationToken ct = default)
    {
        // Check for overlapping leave
        var hasOverlap = await db.LeaveRequests.AnyAsync(lr =>
            lr.EmployeeId == request.EmployeeId &&
            (lr.Status == LeaveRequestStatus.Submitted || lr.Status == LeaveRequestStatus.Approved) &&
            lr.StartDate <= request.EndDate &&
            lr.EndDate >= request.StartDate, ct);

        if (hasOverlap)
        {
            return "This leave request overlaps with an existing submitted or approved request.";
        }

        // Check leave balance
        var currentYear = request.StartDate.Year;
        var balance = await db.LeaveBalances
            .FirstOrDefaultAsync(lb =>
                lb.EmployeeId == request.EmployeeId &&
                lb.LeaveTypeId == request.LeaveTypeId &&
                lb.Year == currentYear, ct);

        if (balance is null)
        {
            return "No leave balance found for this leave type and year.";
        }

        if (balance.RemainingDays < request.TotalDays)
        {
            return $"Insufficient leave balance. Remaining: {balance.RemainingDays} days, Requested: {request.TotalDays} days.";
        }

        // Check if leave type requires approval
        var leaveType = await db.LeaveTypes.FindAsync([request.LeaveTypeId], ct);
        if (leaveType is not null && !leaveType.RequiresApproval)
        {
            request.Status = LeaveRequestStatus.Approved;
            request.ReviewDate = DateTime.UtcNow;
            balance.UsedDays += request.TotalDays;
        }
        else
        {
            request.Status = LeaveRequestStatus.Submitted;
        }

        db.LeaveRequests.Add(request);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Leave request submitted for employee {EmployeeId}, type {LeaveTypeId}, {TotalDays} days",
            request.EmployeeId, request.LeaveTypeId, request.TotalDays);
        return null;
    }

    public async Task<string?> ApproveAsync(int requestId, int reviewedById, string? notes, CancellationToken ct = default)
    {
        var request = await db.LeaveRequests.FindAsync([requestId], ct);
        if (request is null) { return "Leave request not found."; }
        if (request.Status != LeaveRequestStatus.Submitted) { return "Only submitted requests can be approved."; }

        var balance = await db.LeaveBalances
            .FirstOrDefaultAsync(lb =>
                lb.EmployeeId == request.EmployeeId &&
                lb.LeaveTypeId == request.LeaveTypeId &&
                lb.Year == request.StartDate.Year, ct);

        if (balance is null) { return "No leave balance found."; }
        if (balance.RemainingDays < request.TotalDays)
        {
            return $"Insufficient leave balance. Remaining: {balance.RemainingDays} days.";
        }

        request.Status = LeaveRequestStatus.Approved;
        request.ReviewedById = reviewedById;
        request.ReviewDate = DateTime.UtcNow;
        request.ReviewNotes = notes;

        balance.UsedDays += request.TotalDays;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Leave request {RequestId} approved by {ReviewerId}", requestId, reviewedById);
        return null;
    }

    public async Task<string?> RejectAsync(int requestId, int reviewedById, string? notes, CancellationToken ct = default)
    {
        var request = await db.LeaveRequests.FindAsync([requestId], ct);
        if (request is null) { return "Leave request not found."; }
        if (request.Status != LeaveRequestStatus.Submitted) { return "Only submitted requests can be rejected."; }

        request.Status = LeaveRequestStatus.Rejected;
        request.ReviewedById = reviewedById;
        request.ReviewDate = DateTime.UtcNow;
        request.ReviewNotes = notes;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Leave request {RequestId} rejected by {ReviewerId}", requestId, reviewedById);
        return null;
    }

    public async Task<string?> CancelAsync(int requestId, CancellationToken ct = default)
    {
        var request = await db.LeaveRequests.FindAsync([requestId], ct);
        if (request is null) { return "Leave request not found."; }
        if (request.Status is LeaveRequestStatus.Cancelled or LeaveRequestStatus.Rejected)
        {
            return "This request cannot be cancelled.";
        }

        // If approved, restore balance
        if (request.Status == LeaveRequestStatus.Approved)
        {
            var balance = await db.LeaveBalances
                .FirstOrDefaultAsync(lb =>
                    lb.EmployeeId == request.EmployeeId &&
                    lb.LeaveTypeId == request.LeaveTypeId &&
                    lb.Year == request.StartDate.Year, ct);

            if (balance is not null)
            {
                balance.UsedDays -= request.TotalDays;
            }
        }

        request.Status = LeaveRequestStatus.Cancelled;
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Leave request {RequestId} cancelled", requestId);
        return null;
    }

    public async Task<List<LeaveBalance>> GetBalancesAsync(int? departmentId = null, CancellationToken ct = default)
    {
        var currentYear = DateTime.Today.Year;
        var query = db.LeaveBalances
            .AsNoTracking()
            .Include(lb => lb.Employee).ThenInclude(e => e.Department)
            .Include(lb => lb.LeaveType)
            .Where(lb => lb.Year == currentYear && lb.Employee.Status != EmployeeStatus.Terminated);

        if (departmentId.HasValue)
        {
            query = query.Where(lb => lb.Employee.DepartmentId == departmentId.Value);
        }

        return await query
            .OrderBy(lb => lb.Employee.LastName)
            .ThenBy(lb => lb.LeaveType.Name)
            .ToListAsync(ct);
    }

    public async Task<List<LeaveBalance>> GetEmployeeBalancesAsync(int employeeId, CancellationToken ct = default)
    {
        var currentYear = DateTime.Today.Year;
        return await db.LeaveBalances
            .AsNoTracking()
            .Include(lb => lb.LeaveType)
            .Where(lb => lb.EmployeeId == employeeId && lb.Year == currentYear)
            .OrderBy(lb => lb.LeaveType.Name)
            .ToListAsync(ct);
    }

    public async Task<List<LeaveRequest>> GetEmployeeRequestsAsync(int employeeId, CancellationToken ct = default)
    {
        return await db.LeaveRequests
            .AsNoTracking()
            .Include(lr => lr.LeaveType)
            .Include(lr => lr.ReviewedBy)
            .Where(lr => lr.EmployeeId == employeeId)
            .OrderByDescending(lr => lr.SubmittedDate)
            .ToListAsync(ct);
    }

    public async Task<List<LeaveType>> GetLeaveTypesAsync(CancellationToken ct = default)
    {
        return await db.LeaveTypes.AsNoTracking().OrderBy(lt => lt.Name).ToListAsync(ct);
    }

    public async Task<int> GetPendingCountAsync(CancellationToken ct = default)
    {
        return await db.LeaveRequests.CountAsync(lr => lr.Status == LeaveRequestStatus.Submitted, ct);
    }

    public Task<decimal> CalculateBusinessDays(DateOnly start, DateOnly end)
    {
        if (end < start) { return Task.FromResult(0m); }

        var totalDays = 0m;
        var current = start;
        while (current <= end)
        {
            if (current.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday)
            {
                totalDays++;
            }
            current = current.AddDays(1);
        }

        return Task.FromResult(totalDays);
    }
}
