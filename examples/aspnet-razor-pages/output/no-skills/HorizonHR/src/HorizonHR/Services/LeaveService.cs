using HorizonHR.Data;
using HorizonHR.Models;
using HorizonHR.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HorizonHR.Services;

public class LeaveService : ILeaveService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<LeaveService> _logger;

    public LeaveService(ApplicationDbContext context, ILogger<LeaveService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(List<LeaveRequest> Items, int TotalCount)> GetPagedRequestsAsync(
        int page, int pageSize, LeaveRequestStatus? status = null, int? employeeId = null, int? leaveTypeId = null)
    {
        var query = _context.LeaveRequests
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

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(lr => lr.SubmittedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<LeaveRequest?> GetRequestByIdAsync(int id)
    {
        return await _context.LeaveRequests
            .Include(lr => lr.Employee).ThenInclude(e => e.Department)
            .Include(lr => lr.LeaveType)
            .Include(lr => lr.ReviewedBy)
            .FirstOrDefaultAsync(lr => lr.Id == id);
    }

    public async Task<LeaveRequest> SubmitRequestAsync(LeaveRequest request)
    {
        // Check for overlapping leave requests
        var hasOverlap = await _context.LeaveRequests
            .AnyAsync(lr => lr.EmployeeId == request.EmployeeId
                && (lr.Status == LeaveRequestStatus.Submitted || lr.Status == LeaveRequestStatus.Approved)
                && lr.StartDate <= request.EndDate
                && lr.EndDate >= request.StartDate);

        if (hasOverlap)
            throw new InvalidOperationException("This leave request overlaps with an existing submitted or approved leave request.");

        // Check leave balance
        var balance = await _context.LeaveBalances
            .FirstOrDefaultAsync(lb => lb.EmployeeId == request.EmployeeId
                && lb.LeaveTypeId == request.LeaveTypeId
                && lb.Year == request.StartDate.Year);

        if (balance == null)
            throw new InvalidOperationException("No leave balance found for the selected leave type and year.");

        if (balance.RemainingDays < request.TotalDays)
            throw new InvalidOperationException($"Insufficient leave balance. Remaining: {balance.RemainingDays} days, Requested: {request.TotalDays} days.");

        // Check if auto-approval
        var leaveType = await _context.LeaveTypes.FindAsync(request.LeaveTypeId);
        if (leaveType != null && !leaveType.RequiresApproval)
        {
            request.Status = LeaveRequestStatus.Approved;
            request.ReviewDate = DateTime.UtcNow;
            balance.UsedDays += request.TotalDays;
        }

        _context.LeaveRequests.Add(request);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Leave request submitted for employee {EmployeeId}, type {LeaveTypeId}, {TotalDays} days",
            request.EmployeeId, request.LeaveTypeId, request.TotalDays);

        return request;
    }

    public async Task ApproveAsync(int id, int reviewedById, string? notes)
    {
        var request = await _context.LeaveRequests.FindAsync(id);
        if (request == null) throw new InvalidOperationException("Leave request not found.");
        if (request.Status != LeaveRequestStatus.Submitted)
            throw new InvalidOperationException("Only submitted requests can be approved.");

        var balance = await _context.LeaveBalances
            .FirstOrDefaultAsync(lb => lb.EmployeeId == request.EmployeeId
                && lb.LeaveTypeId == request.LeaveTypeId
                && lb.Year == request.StartDate.Year);

        if (balance == null)
            throw new InvalidOperationException("Leave balance not found.");

        if (balance.RemainingDays < request.TotalDays)
            throw new InvalidOperationException("Insufficient leave balance to approve.");

        request.Status = LeaveRequestStatus.Approved;
        request.ReviewedById = reviewedById;
        request.ReviewDate = DateTime.UtcNow;
        request.ReviewNotes = notes;

        balance.UsedDays += request.TotalDays;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Leave request {Id} approved by {ReviewedById}", id, reviewedById);
    }

    public async Task RejectAsync(int id, int reviewedById, string? notes)
    {
        var request = await _context.LeaveRequests.FindAsync(id);
        if (request == null) throw new InvalidOperationException("Leave request not found.");
        if (request.Status != LeaveRequestStatus.Submitted)
            throw new InvalidOperationException("Only submitted requests can be rejected.");

        request.Status = LeaveRequestStatus.Rejected;
        request.ReviewedById = reviewedById;
        request.ReviewDate = DateTime.UtcNow;
        request.ReviewNotes = notes;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Leave request {Id} rejected by {ReviewedById}", id, reviewedById);
    }

    public async Task CancelAsync(int id)
    {
        var request = await _context.LeaveRequests.FindAsync(id);
        if (request == null) throw new InvalidOperationException("Leave request not found.");

        if (request.Status != LeaveRequestStatus.Submitted && request.Status != LeaveRequestStatus.Approved)
            throw new InvalidOperationException("Only submitted or approved requests can be cancelled.");

        // Restore balance if was approved
        if (request.Status == LeaveRequestStatus.Approved)
        {
            var balance = await _context.LeaveBalances
                .FirstOrDefaultAsync(lb => lb.EmployeeId == request.EmployeeId
                    && lb.LeaveTypeId == request.LeaveTypeId
                    && lb.Year == request.StartDate.Year);

            if (balance != null)
                balance.UsedDays -= request.TotalDays;
        }

        request.Status = LeaveRequestStatus.Cancelled;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Leave request {Id} cancelled", id);
    }

    public async Task<List<LeaveBalance>> GetBalancesAsync(int? departmentId = null, int? year = null)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var query = _context.LeaveBalances
            .Include(lb => lb.Employee).ThenInclude(e => e.Department)
            .Include(lb => lb.LeaveType)
            .Where(lb => lb.Year == targetYear && lb.Employee.Status != EmployeeStatus.Terminated);

        if (departmentId.HasValue)
            query = query.Where(lb => lb.Employee.DepartmentId == departmentId.Value);

        return await query
            .OrderBy(lb => lb.Employee.LastName)
            .ThenBy(lb => lb.LeaveType.Name)
            .ToListAsync();
    }

    public async Task<List<LeaveBalance>> GetEmployeeBalancesAsync(int employeeId, int? year = null)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        return await _context.LeaveBalances
            .Include(lb => lb.LeaveType)
            .Where(lb => lb.EmployeeId == employeeId && lb.Year == targetYear)
            .OrderBy(lb => lb.LeaveType.Name)
            .ToListAsync();
    }

    public async Task<List<LeaveRequest>> GetEmployeeRequestsAsync(int employeeId)
    {
        return await _context.LeaveRequests
            .Include(lr => lr.LeaveType)
            .Include(lr => lr.ReviewedBy)
            .Where(lr => lr.EmployeeId == employeeId)
            .OrderByDescending(lr => lr.SubmittedDate)
            .ToListAsync();
    }

    public async Task<List<LeaveType>> GetLeaveTypesAsync()
    {
        return await _context.LeaveTypes.OrderBy(lt => lt.Name).ToListAsync();
    }

    public async Task<int> GetPendingCountAsync()
    {
        return await _context.LeaveRequests.CountAsync(lr => lr.Status == LeaveRequestStatus.Submitted);
    }
}
