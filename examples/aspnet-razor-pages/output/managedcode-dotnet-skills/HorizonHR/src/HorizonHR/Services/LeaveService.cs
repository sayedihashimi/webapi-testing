using HorizonHR.Data;
using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HorizonHR.Services;

public class LeaveService(ApplicationDbContext db, ILogger<LeaveService> logger) : ILeaveService
{
    public async Task<(List<LeaveRequest> Items, int TotalCount)> GetPagedRequestsAsync(
        int pageNumber, int pageSize,
        LeaveRequestStatus? status = null, int? employeeId = null,
        int? leaveTypeId = null, DateOnly? startDate = null, DateOnly? endDate = null)
    {
        var query = db.LeaveRequests
            .Include(lr => lr.Employee)
            .Include(lr => lr.LeaveType)
            .Include(lr => lr.ReviewedBy)
            .AsNoTracking()
            .AsQueryable();

        if (status.HasValue) query = query.Where(lr => lr.Status == status.Value);
        if (employeeId.HasValue) query = query.Where(lr => lr.EmployeeId == employeeId.Value);
        if (leaveTypeId.HasValue) query = query.Where(lr => lr.LeaveTypeId == leaveTypeId.Value);
        if (startDate.HasValue) query = query.Where(lr => lr.EndDate >= startDate.Value);
        if (endDate.HasValue) query = query.Where(lr => lr.StartDate <= endDate.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(lr => lr.SubmittedDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<LeaveRequest?> GetRequestByIdAsync(int id)
    {
        return await db.LeaveRequests
            .Include(lr => lr.Employee).ThenInclude(e => e.Department)
            .Include(lr => lr.LeaveType)
            .Include(lr => lr.ReviewedBy)
            .FirstOrDefaultAsync(lr => lr.Id == id);
    }

    public async Task<LeaveRequest> SubmitRequestAsync(LeaveRequest request)
    {
        // Validate dates
        if (request.EndDate < request.StartDate)
            throw new InvalidOperationException("End date must be on or after start date.");

        // Calculate business days
        request.TotalDays = await CalculateBusinessDays(request.StartDate, request.EndDate);

        // Check for overlapping requests
        var hasOverlap = await db.LeaveRequests
            .AnyAsync(lr => lr.EmployeeId == request.EmployeeId
                && lr.Status != LeaveRequestStatus.Rejected
                && lr.Status != LeaveRequestStatus.Cancelled
                && lr.StartDate <= request.EndDate
                && lr.EndDate >= request.StartDate);

        if (hasOverlap)
            throw new InvalidOperationException("This leave request overlaps with an existing request.");

        // Check balance
        var currentYear = request.StartDate.Year;
        var balance = await db.LeaveBalances
            .FirstOrDefaultAsync(lb => lb.EmployeeId == request.EmployeeId
                && lb.LeaveTypeId == request.LeaveTypeId
                && lb.Year == currentYear);

        if (balance is null)
            throw new InvalidOperationException("No leave balance found for this leave type and year.");

        if (balance.RemainingDays < request.TotalDays)
            throw new InvalidOperationException(
                $"Insufficient leave balance. Remaining: {balance.RemainingDays:F1} days, Requested: {request.TotalDays:F1} days.");

        // Check if auto-approval
        var leaveType = await db.LeaveTypes.FindAsync(request.LeaveTypeId);
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
        await db.SaveChangesAsync();
        logger.LogInformation("Leave request submitted for employee {EmpId}, {Days} days of {Type}",
            request.EmployeeId, request.TotalDays, leaveType?.Name);
        return request;
    }

    public async Task ApproveAsync(int requestId, int reviewerId, string? notes)
    {
        var request = await db.LeaveRequests.FindAsync(requestId)
            ?? throw new InvalidOperationException("Leave request not found.");

        if (request.Status != LeaveRequestStatus.Submitted)
            throw new InvalidOperationException("Only submitted requests can be approved.");

        var balance = await db.LeaveBalances
            .FirstOrDefaultAsync(lb => lb.EmployeeId == request.EmployeeId
                && lb.LeaveTypeId == request.LeaveTypeId
                && lb.Year == request.StartDate.Year)
            ?? throw new InvalidOperationException("Leave balance not found.");

        if (balance.RemainingDays < request.TotalDays)
            throw new InvalidOperationException("Insufficient leave balance.");

        request.Status = LeaveRequestStatus.Approved;
        request.ReviewedById = reviewerId;
        request.ReviewDate = DateTime.UtcNow;
        request.ReviewNotes = notes;
        balance.UsedDays += request.TotalDays;

        await db.SaveChangesAsync();
        logger.LogInformation("Leave request {Id} approved by {Reviewer}", requestId, reviewerId);
    }

    public async Task RejectAsync(int requestId, int reviewerId, string? notes)
    {
        var request = await db.LeaveRequests.FindAsync(requestId)
            ?? throw new InvalidOperationException("Leave request not found.");

        if (request.Status != LeaveRequestStatus.Submitted)
            throw new InvalidOperationException("Only submitted requests can be rejected.");

        request.Status = LeaveRequestStatus.Rejected;
        request.ReviewedById = reviewerId;
        request.ReviewDate = DateTime.UtcNow;
        request.ReviewNotes = notes;

        await db.SaveChangesAsync();
        logger.LogInformation("Leave request {Id} rejected by {Reviewer}", requestId, reviewerId);
    }

    public async Task CancelAsync(int requestId)
    {
        var request = await db.LeaveRequests.FindAsync(requestId)
            ?? throw new InvalidOperationException("Leave request not found.");

        if (request.Status is LeaveRequestStatus.Rejected or LeaveRequestStatus.Cancelled)
            throw new InvalidOperationException("This request cannot be cancelled.");

        // Restore balance if it was approved
        if (request.Status == LeaveRequestStatus.Approved)
        {
            var balance = await db.LeaveBalances
                .FirstOrDefaultAsync(lb => lb.EmployeeId == request.EmployeeId
                    && lb.LeaveTypeId == request.LeaveTypeId
                    && lb.Year == request.StartDate.Year);

            if (balance is not null)
                balance.UsedDays -= request.TotalDays;
        }

        request.Status = LeaveRequestStatus.Cancelled;
        await db.SaveChangesAsync();
        logger.LogInformation("Leave request {Id} cancelled", requestId);
    }

    public async Task<List<LeaveBalance>> GetBalancesAsync(int? departmentId = null, int? year = null)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var query = db.LeaveBalances
            .Include(lb => lb.Employee).ThenInclude(e => e.Department)
            .Include(lb => lb.LeaveType)
            .AsNoTracking()
            .Where(lb => lb.Year == targetYear);

        if (departmentId.HasValue)
            query = query.Where(lb => lb.Employee.DepartmentId == departmentId.Value);

        return await query.OrderBy(lb => lb.Employee.LastName).ToListAsync();
    }

    public async Task<List<LeaveBalance>> GetEmployeeBalancesAsync(int employeeId, int? year = null)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        return await db.LeaveBalances
            .Include(lb => lb.LeaveType)
            .AsNoTracking()
            .Where(lb => lb.EmployeeId == employeeId && lb.Year == targetYear)
            .OrderBy(lb => lb.LeaveType.Name)
            .ToListAsync();
    }

    public async Task<List<LeaveRequest>> GetEmployeeRequestsAsync(int employeeId)
    {
        return await db.LeaveRequests
            .Include(lr => lr.LeaveType)
            .Include(lr => lr.ReviewedBy)
            .AsNoTracking()
            .Where(lr => lr.EmployeeId == employeeId)
            .OrderByDescending(lr => lr.SubmittedDate)
            .ToListAsync();
    }

    public async Task<List<LeaveType>> GetLeaveTypesAsync()
    {
        return await db.LeaveTypes.AsNoTracking().OrderBy(lt => lt.Name).ToListAsync();
    }

    public Task<decimal> CalculateBusinessDays(DateOnly start, DateOnly end)
    {
        var days = 0m;
        var current = start;
        while (current <= end)
        {
            if (current.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday))
                days++;
            current = current.AddDays(1);
        }
        return Task.FromResult(days);
    }
}
