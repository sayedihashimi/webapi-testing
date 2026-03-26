using HorizonHR.Data;
using HorizonHR.Models;
using HorizonHR.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HorizonHR.Services;

public sealed class LeaveService(HorizonDbContext db, ILogger<LeaveService> logger) : ILeaveService
{
    public async Task<PaginatedList<LeaveRequest>> GetAllRequestsAsync(LeaveRequestStatus? status, int? employeeId, int? leaveTypeId, DateOnly? startDate, DateOnly? endDate, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = db.LeaveRequests
            .AsNoTracking()
            .Include(lr => lr.Employee)
            .Include(lr => lr.LeaveType)
            .Include(lr => lr.ReviewedBy)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(lr => lr.Status == status.Value);
        if (employeeId.HasValue)
            query = query.Where(lr => lr.EmployeeId == employeeId.Value);
        if (leaveTypeId.HasValue)
            query = query.Where(lr => lr.LeaveTypeId == leaveTypeId.Value);
        if (startDate.HasValue)
            query = query.Where(lr => lr.EndDate >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(lr => lr.StartDate <= endDate.Value);

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

    public async Task<LeaveRequest> SubmitRequestAsync(int employeeId, int leaveTypeId, DateOnly startDate, DateOnly endDate, string reason, CancellationToken ct = default)
    {
        if (endDate < startDate)
            throw new InvalidOperationException("End date must be on or after start date.");

        var employee = await db.Employees.FindAsync([employeeId], ct)
            ?? throw new KeyNotFoundException("Employee not found.");

        var leaveType = await db.LeaveTypes.FindAsync([leaveTypeId], ct)
            ?? throw new KeyNotFoundException("Leave type not found.");

        // Calculate business days
        var totalDays = CalculateBusinessDays(startDate, endDate);

        // Check for overlapping leave
        var hasOverlap = await db.LeaveRequests.AnyAsync(lr =>
            lr.EmployeeId == employeeId &&
            (lr.Status == LeaveRequestStatus.Submitted || lr.Status == LeaveRequestStatus.Approved) &&
            lr.StartDate <= endDate && lr.EndDate >= startDate, ct);

        if (hasOverlap)
            throw new InvalidOperationException("This leave request overlaps with an existing submitted or approved request.");

        // Check balance
        var currentYear = startDate.Year;
        var balance = await db.LeaveBalances.FirstOrDefaultAsync(lb =>
            lb.EmployeeId == employeeId && lb.LeaveTypeId == leaveTypeId && lb.Year == currentYear, ct);

        if (balance == null)
            throw new InvalidOperationException("No leave balance found for this leave type and year.");

        var remaining = balance.TotalDays + balance.CarriedOverDays - balance.UsedDays;
        if (totalDays > remaining)
            throw new InvalidOperationException($"Insufficient leave balance. Remaining: {remaining} days, Requested: {totalDays} days.");

        var request = new LeaveRequest
        {
            EmployeeId = employeeId,
            LeaveTypeId = leaveTypeId,
            StartDate = startDate,
            EndDate = endDate,
            TotalDays = totalDays,
            Reason = reason,
            Status = leaveType.RequiresApproval ? LeaveRequestStatus.Submitted : LeaveRequestStatus.Approved,
            SubmittedDate = DateTime.UtcNow
        };

        db.LeaveRequests.Add(request);

        // If auto-approved, deduct from balance
        if (!leaveType.RequiresApproval)
        {
            balance.UsedDays += totalDays;
            request.ReviewDate = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Leave request submitted: Employee {EmployeeId}, Type {LeaveType}, {TotalDays} days", employeeId, leaveType.Name, totalDays);
        return request;
    }

    public async Task<LeaveRequest> ApproveAsync(int id, int reviewerId, string? notes, CancellationToken ct = default)
    {
        var request = await db.LeaveRequests.Include(lr => lr.LeaveType).FirstOrDefaultAsync(lr => lr.Id == id, ct)
            ?? throw new KeyNotFoundException("Leave request not found.");

        if (request.Status != LeaveRequestStatus.Submitted)
            throw new InvalidOperationException("Only submitted requests can be approved.");

        request.Status = LeaveRequestStatus.Approved;
        request.ReviewedById = reviewerId;
        request.ReviewDate = DateTime.UtcNow;
        request.ReviewNotes = notes;

        // Deduct from balance
        var balance = await db.LeaveBalances.FirstOrDefaultAsync(lb =>
            lb.EmployeeId == request.EmployeeId && lb.LeaveTypeId == request.LeaveTypeId && lb.Year == request.StartDate.Year, ct);

        if (balance != null)
            balance.UsedDays += request.TotalDays;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Leave request approved: {RequestId} by {ReviewerId}", id, reviewerId);
        return request;
    }

    public async Task<LeaveRequest> RejectAsync(int id, int reviewerId, string? notes, CancellationToken ct = default)
    {
        var request = await db.LeaveRequests.FindAsync([id], ct)
            ?? throw new KeyNotFoundException("Leave request not found.");

        if (request.Status != LeaveRequestStatus.Submitted)
            throw new InvalidOperationException("Only submitted requests can be rejected.");

        request.Status = LeaveRequestStatus.Rejected;
        request.ReviewedById = reviewerId;
        request.ReviewDate = DateTime.UtcNow;
        request.ReviewNotes = notes;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Leave request rejected: {RequestId} by {ReviewerId}", id, reviewerId);
        return request;
    }

    public async Task<LeaveRequest> CancelAsync(int id, CancellationToken ct = default)
    {
        var request = await db.LeaveRequests.FindAsync([id], ct)
            ?? throw new KeyNotFoundException("Leave request not found.");

        if (request.Status != LeaveRequestStatus.Submitted && request.Status != LeaveRequestStatus.Approved)
            throw new InvalidOperationException("Only submitted or approved requests can be cancelled.");

        // If approved, restore balance
        if (request.Status == LeaveRequestStatus.Approved)
        {
            var balance = await db.LeaveBalances.FirstOrDefaultAsync(lb =>
                lb.EmployeeId == request.EmployeeId && lb.LeaveTypeId == request.LeaveTypeId && lb.Year == request.StartDate.Year, ct);

            if (balance != null)
                balance.UsedDays -= request.TotalDays;
        }

        request.Status = LeaveRequestStatus.Cancelled;
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Leave request cancelled: {RequestId}", id);
        return request;
    }

    public async Task<List<LeaveBalance>> GetBalancesAsync(int? departmentId, int? year, CancellationToken ct = default)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var query = db.LeaveBalances
            .AsNoTracking()
            .Include(lb => lb.Employee).ThenInclude(e => e.Department)
            .Include(lb => lb.LeaveType)
            .Where(lb => lb.Year == targetYear);

        if (departmentId.HasValue)
            query = query.Where(lb => lb.Employee.DepartmentId == departmentId.Value);

        return await query.OrderBy(lb => lb.Employee.LastName).ThenBy(lb => lb.LeaveType.Name).ToListAsync(ct);
    }

    public async Task<List<LeaveBalance>> GetEmployeeBalancesAsync(int employeeId, int? year, CancellationToken ct = default)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        return await db.LeaveBalances
            .AsNoTracking()
            .Include(lb => lb.LeaveType)
            .Where(lb => lb.EmployeeId == employeeId && lb.Year == targetYear)
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

    private static decimal CalculateBusinessDays(DateOnly start, DateOnly end)
    {
        var days = 0m;
        var current = start;
        while (current <= end)
        {
            if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
                days += 1;
            current = current.AddDays(1);
        }
        return days;
    }
}
