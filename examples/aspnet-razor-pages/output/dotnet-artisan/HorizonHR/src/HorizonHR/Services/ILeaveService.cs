using HorizonHR.Models;

namespace HorizonHR.Services;

public interface ILeaveService
{
    Task<PaginatedList<LeaveRequest>> GetAllRequestsAsync(int pageNumber, int pageSize,
        LeaveRequestStatus? status = null, int? employeeId = null, int? leaveTypeId = null,
        CancellationToken ct = default);
    Task<LeaveRequest?> GetRequestByIdAsync(int id, CancellationToken ct = default);
    Task<string?> SubmitRequestAsync(LeaveRequest request, CancellationToken ct = default);
    Task<string?> ApproveAsync(int requestId, int reviewedById, string? notes, CancellationToken ct = default);
    Task<string?> RejectAsync(int requestId, int reviewedById, string? notes, CancellationToken ct = default);
    Task<string?> CancelAsync(int requestId, CancellationToken ct = default);
    Task<List<LeaveBalance>> GetBalancesAsync(int? departmentId = null, CancellationToken ct = default);
    Task<List<LeaveBalance>> GetEmployeeBalancesAsync(int employeeId, CancellationToken ct = default);
    Task<List<LeaveRequest>> GetEmployeeRequestsAsync(int employeeId, CancellationToken ct = default);
    Task<List<LeaveType>> GetLeaveTypesAsync(CancellationToken ct = default);
    Task<int> GetPendingCountAsync(CancellationToken ct = default);
    Task<decimal> CalculateBusinessDays(DateOnly start, DateOnly end);
}
