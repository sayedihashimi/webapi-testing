using HorizonHR.Models;
using HorizonHR.Models.Enums;

namespace HorizonHR.Services;

public interface ILeaveService
{
    Task<PaginatedList<LeaveRequest>> GetAllRequestsAsync(LeaveRequestStatus? status, int? employeeId, int? leaveTypeId, DateOnly? startDate, DateOnly? endDate, int pageNumber, int pageSize, CancellationToken ct = default);
    Task<LeaveRequest?> GetRequestByIdAsync(int id, CancellationToken ct = default);
    Task<LeaveRequest> SubmitRequestAsync(int employeeId, int leaveTypeId, DateOnly startDate, DateOnly endDate, string reason, CancellationToken ct = default);
    Task<LeaveRequest> ApproveAsync(int id, int reviewerId, string? notes, CancellationToken ct = default);
    Task<LeaveRequest> RejectAsync(int id, int reviewerId, string? notes, CancellationToken ct = default);
    Task<LeaveRequest> CancelAsync(int id, CancellationToken ct = default);
    Task<List<LeaveBalance>> GetBalancesAsync(int? departmentId, int? year, CancellationToken ct = default);
    Task<List<LeaveBalance>> GetEmployeeBalancesAsync(int employeeId, int? year, CancellationToken ct = default);
    Task<List<LeaveRequest>> GetEmployeeRequestsAsync(int employeeId, CancellationToken ct = default);
    Task<List<LeaveType>> GetLeaveTypesAsync(CancellationToken ct = default);
}
