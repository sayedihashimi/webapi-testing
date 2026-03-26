using HorizonHR.Data;
using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HorizonHR.Services;

public sealed class LeaveService : ILeaveService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<LeaveService> _logger;

    public LeaveService(ApplicationDbContext context, ILogger<LeaveService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(List<LeaveRequest> Items, int TotalCount)> GetLeaveRequestsAsync(
        LeaveRequestStatus? status, int? employeeId, int? leaveTypeId,
        DateOnly? startDate, DateOnly? endDate, int page, int pageSize)
    {
        var query = _context.LeaveRequests
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

        if (startDate.HasValue)
        {
            query = query.Where(lr => lr.StartDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(lr => lr.EndDate <= endDate.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(lr => lr.SubmittedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<LeaveRequest?> GetLeaveRequestByIdAsync(int id)
    {
        return await _context.LeaveRequests
            .AsNoTracking()
            .Include(lr => lr.Employee)
            .Include(lr => lr.LeaveType)
            .Include(lr => lr.ReviewedBy)
            .FirstOrDefaultAsync(lr => lr.Id == id);
    }

    public async Task<LeaveRequest> SubmitLeaveRequestAsync(LeaveRequest request)
    {
        // Calculate business days
        request.TotalDays = await CalculateBusinessDaysAsync(request.StartDate, request.EndDate);

        // Check leave balance
        var year = request.StartDate.Year;
        var balance = await _context.LeaveBalances
            .FirstOrDefaultAsync(lb =>
                lb.EmployeeId == request.EmployeeId &&
                lb.LeaveTypeId == request.LeaveTypeId &&
                lb.Year == year);

        if (balance == null)
        {
            throw new InvalidOperationException("No leave balance found for the specified leave type and year.");
        }

        var remainingDays = balance.TotalDays + balance.CarriedOverDays - balance.UsedDays;
        if (request.TotalDays > remainingDays)
        {
            throw new InvalidOperationException(
                $"Insufficient leave balance. Requested: {request.TotalDays} days, Available: {remainingDays} days.");
        }

        // Check for overlapping requests
        var hasOverlap = await _context.LeaveRequests
            .AsNoTracking()
            .AnyAsync(lr =>
                lr.EmployeeId == request.EmployeeId &&
                (lr.Status == LeaveRequestStatus.Submitted || lr.Status == LeaveRequestStatus.Approved) &&
                lr.StartDate <= request.EndDate &&
                lr.EndDate >= request.StartDate);

        if (hasOverlap)
        {
            throw new InvalidOperationException(
                "An overlapping leave request already exists for the specified dates.");
        }

        request.Status = LeaveRequestStatus.Submitted;
        request.SubmittedDate = DateTime.UtcNow;
        request.CreatedAt = DateTime.UtcNow;
        request.UpdatedAt = DateTime.UtcNow;

        // Check if auto-approval applies
        var leaveType = await _context.LeaveTypes
            .AsNoTracking()
            .FirstOrDefaultAsync(lt => lt.Id == request.LeaveTypeId);

        if (leaveType != null && !leaveType.RequiresApproval)
        {
            request.Status = LeaveRequestStatus.Approved;
            request.ReviewDate = DateTime.UtcNow;

            // Deduct from balance
            balance.UsedDays += request.TotalDays;
        }

        _context.LeaveRequests.Add(request);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Leave request {RequestId} submitted for employee {EmployeeId}. " +
            "Status: {Status}, Days: {TotalDays}",
            request.Id, request.EmployeeId, request.Status, request.TotalDays);

        return request;
    }

    public async Task ApproveLeaveRequestAsync(int requestId, int reviewerId, string? notes)
    {
        var request = await _context.LeaveRequests
            .FirstOrDefaultAsync(lr => lr.Id == requestId);

        if (request == null)
        {
            throw new InvalidOperationException($"Leave request with ID {requestId} not found.");
        }

        if (request.Status != LeaveRequestStatus.Submitted)
        {
            throw new InvalidOperationException("Only submitted leave requests can be approved.");
        }

        request.Status = LeaveRequestStatus.Approved;
        request.ReviewedById = reviewerId;
        request.ReviewDate = DateTime.UtcNow;
        request.ReviewNotes = notes;
        request.UpdatedAt = DateTime.UtcNow;

        // Deduct from leave balance
        var balance = await _context.LeaveBalances
            .FirstOrDefaultAsync(lb =>
                lb.EmployeeId == request.EmployeeId &&
                lb.LeaveTypeId == request.LeaveTypeId &&
                lb.Year == request.StartDate.Year);

        if (balance != null)
        {
            balance.UsedDays += request.TotalDays;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Leave request {RequestId} approved by reviewer {ReviewerId}", requestId, reviewerId);
    }

    public async Task RejectLeaveRequestAsync(int requestId, int reviewerId, string? notes)
    {
        var request = await _context.LeaveRequests
            .FirstOrDefaultAsync(lr => lr.Id == requestId);

        if (request == null)
        {
            throw new InvalidOperationException($"Leave request with ID {requestId} not found.");
        }

        if (request.Status != LeaveRequestStatus.Submitted)
        {
            throw new InvalidOperationException("Only submitted leave requests can be rejected.");
        }

        request.Status = LeaveRequestStatus.Rejected;
        request.ReviewedById = reviewerId;
        request.ReviewDate = DateTime.UtcNow;
        request.ReviewNotes = notes;
        request.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Leave request {RequestId} rejected by reviewer {ReviewerId}", requestId, reviewerId);
    }

    public async Task CancelLeaveRequestAsync(int requestId)
    {
        var request = await _context.LeaveRequests
            .FirstOrDefaultAsync(lr => lr.Id == requestId);

        if (request == null)
        {
            throw new InvalidOperationException($"Leave request with ID {requestId} not found.");
        }

        var previousStatus = request.Status;

        // If the request was approved, restore the days to balance
        if (previousStatus == LeaveRequestStatus.Approved)
        {
            var balance = await _context.LeaveBalances
                .FirstOrDefaultAsync(lb =>
                    lb.EmployeeId == request.EmployeeId &&
                    lb.LeaveTypeId == request.LeaveTypeId &&
                    lb.Year == request.StartDate.Year);

            if (balance != null)
            {
                balance.UsedDays -= request.TotalDays;
            }
        }

        request.Status = LeaveRequestStatus.Cancelled;
        request.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Leave request {RequestId} cancelled. Previous status: {PreviousStatus}",
            requestId, previousStatus);
    }

    public async Task<List<LeaveBalance>> GetEmployeeLeaveBalancesAsync(int employeeId, int year)
    {
        return await _context.LeaveBalances
            .AsNoTracking()
            .Include(lb => lb.LeaveType)
            .Where(lb => lb.EmployeeId == employeeId && lb.Year == year)
            .OrderBy(lb => lb.LeaveType.Name)
            .ToListAsync();
    }

    public async Task<(List<LeaveBalance> Items, int TotalCount)> GetAllLeaveBalancesAsync(
        int? departmentId, int year, int page, int pageSize)
    {
        var query = _context.LeaveBalances
            .AsNoTracking()
            .Include(lb => lb.Employee)
                .ThenInclude(e => e.Department)
            .Include(lb => lb.LeaveType)
            .Where(lb => lb.Year == year);

        if (departmentId.HasValue)
        {
            query = query.Where(lb => lb.Employee.DepartmentId == departmentId.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(lb => lb.Employee.LastName)
            .ThenBy(lb => lb.Employee.FirstName)
            .ThenBy(lb => lb.LeaveType.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<LeaveType>> GetLeaveTypesAsync()
    {
        return await _context.LeaveTypes
            .AsNoTracking()
            .OrderBy(lt => lt.Name)
            .ToListAsync();
    }

    public Task<decimal> CalculateBusinessDaysAsync(DateOnly startDate, DateOnly endDate)
    {
        var businessDays = 0m;
        var current = startDate;

        while (current <= endDate)
        {
            if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
            {
                businessDays++;
            }

            current = current.AddDays(1);
        }

        return Task.FromResult(businessDays);
    }
}
