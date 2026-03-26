using HorizonHR.Models;
using HorizonHR.Models.Enums;

namespace HorizonHR.Services.Interfaces;

public interface IDepartmentService
{
    Task<List<Department>> GetAllAsync();
    Task<Department?> GetByIdAsync(int id);
    Task<Department> CreateAsync(Department department);
    Task UpdateAsync(Department department);
    Task<bool> ValidateHierarchy(int departmentId, int? parentDepartmentId);
    Task<int> GetEmployeeCountAsync(int departmentId);
}

public interface IEmployeeService
{
    Task<(List<Employee> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, int pageSize,
        string? searchTerm = null, int? departmentId = null,
        EmploymentType? employmentType = null, EmployeeStatus? status = null);
    Task<Employee?> GetByIdAsync(int id);
    Task<Employee> CreateAsync(Employee employee);
    Task UpdateAsync(Employee employee);
    Task TerminateAsync(int employeeId, DateOnly terminationDate);
    Task<string> GenerateEmployeeNumberAsync();
    Task<List<Employee>> GetDirectReportsAsync(int employeeId);
    Task<List<Employee>> GetByDepartmentAsync(int departmentId);
}

public interface ILeaveService
{
    Task<(List<LeaveRequest> Items, int TotalCount)> GetPagedRequestsAsync(
        int pageNumber, int pageSize,
        LeaveRequestStatus? status = null, int? employeeId = null,
        int? leaveTypeId = null, DateOnly? startDate = null, DateOnly? endDate = null);
    Task<LeaveRequest?> GetRequestByIdAsync(int id);
    Task<LeaveRequest> SubmitRequestAsync(LeaveRequest request);
    Task ApproveAsync(int requestId, int reviewerId, string? notes);
    Task RejectAsync(int requestId, int reviewerId, string? notes);
    Task CancelAsync(int requestId);
    Task<List<LeaveBalance>> GetBalancesAsync(int? departmentId = null, int? year = null);
    Task<List<LeaveBalance>> GetEmployeeBalancesAsync(int employeeId, int? year = null);
    Task<List<LeaveRequest>> GetEmployeeRequestsAsync(int employeeId);
    Task<List<LeaveType>> GetLeaveTypesAsync();
    Task<decimal> CalculateBusinessDays(DateOnly start, DateOnly end);
}

public interface IReviewService
{
    Task<(List<PerformanceReview> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, int pageSize,
        ReviewStatus? status = null, OverallRating? rating = null,
        int? departmentId = null);
    Task<PerformanceReview?> GetByIdAsync(int id);
    Task<PerformanceReview> CreateAsync(PerformanceReview review);
    Task SubmitSelfAssessmentAsync(int reviewId, string selfAssessment);
    Task CompleteManagerReviewAsync(int reviewId, string managerAssessment,
        OverallRating rating, string? strengths, string? areasForImprovement, string? goals);
}

public interface ISkillService
{
    Task<List<Skill>> GetAllAsync();
    Task<Skill?> GetByIdAsync(int id);
    Task<Skill> CreateAsync(Skill skill);
    Task UpdateAsync(Skill skill);
    Task<List<EmployeeSkill>> GetEmployeeSkillsAsync(int employeeId);
    Task AddEmployeeSkillAsync(EmployeeSkill employeeSkill);
    Task UpdateEmployeeSkillAsync(EmployeeSkill employeeSkill);
    Task RemoveEmployeeSkillAsync(int employeeId, int skillId);
    Task<List<EmployeeSkill>> SearchBySkillAsync(int skillId, ProficiencyLevel? minLevel = null);
}

public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardDataAsync();
}

public class DashboardViewModel
{
    public int TotalEmployees { get; set; }
    public int DepartmentCount { get; set; }
    public int PendingLeaveRequests { get; set; }
    public int UpcomingReviews { get; set; }
    public List<Employee> RecentHires { get; set; } = [];
    public List<Employee> EmployeesOnLeaveToday { get; set; } = [];
    public List<DepartmentHeadcount> HeadcountByDepartment { get; set; } = [];
}

public class DepartmentHeadcount
{
    public string DepartmentName { get; set; } = string.Empty;
    public int Count { get; set; }
}
