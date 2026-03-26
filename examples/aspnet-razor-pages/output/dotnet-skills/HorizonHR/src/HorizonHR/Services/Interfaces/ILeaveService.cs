using HorizonHR.Models;
using HorizonHR.Models.Enums;

namespace HorizonHR.Services.Interfaces;

public interface ILeaveService
{
    Task<(List<LeaveRequest> Items, int TotalCount)> GetLeaveRequestsAsync(LeaveRequestStatus? status, int? employeeId, int? leaveTypeId, DateOnly? startDate, DateOnly? endDate, int page, int pageSize);
    Task<LeaveRequest?> GetLeaveRequestByIdAsync(int id);
    Task<LeaveRequest> SubmitLeaveRequestAsync(LeaveRequest request);
    Task ApproveLeaveRequestAsync(int requestId, int reviewerId, string? notes);
    Task RejectLeaveRequestAsync(int requestId, int reviewerId, string? notes);
    Task CancelLeaveRequestAsync(int requestId);
    Task<List<LeaveBalance>> GetEmployeeLeaveBalancesAsync(int employeeId, int year);
    Task<(List<LeaveBalance> Items, int TotalCount)> GetAllLeaveBalancesAsync(int? departmentId, int year, int page, int pageSize);
    Task<List<LeaveType>> GetLeaveTypesAsync();
    Task<decimal> CalculateBusinessDaysAsync(DateOnly startDate, DateOnly endDate);
}
