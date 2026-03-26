using HorizonHR.Models;

namespace HorizonHR.Services;

public interface IEmployeeService
{
    Task<PaginatedList<Employee>> GetAllAsync(int pageNumber, int pageSize, string? search = null,
        int? departmentId = null, EmploymentType? employmentType = null, EmployeeStatus? status = null,
        CancellationToken ct = default);
    Task<Employee?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<Employee>> GetByDepartmentAsync(int departmentId, CancellationToken ct = default);
    Task<List<Employee>> GetAllActiveAsync(CancellationToken ct = default);
    Task<List<Employee>> GetDirectReportsAsync(int managerId, CancellationToken ct = default);
    Task CreateAsync(Employee employee, CancellationToken ct = default);
    Task UpdateAsync(Employee employee, CancellationToken ct = default);
    Task TerminateAsync(int employeeId, DateOnly terminationDate, CancellationToken ct = default);
    Task<string> GenerateEmployeeNumberAsync(CancellationToken ct = default);
    Task<int> GetTotalCountAsync(CancellationToken ct = default);
    Task<List<Employee>> GetRecentHiresAsync(int days, CancellationToken ct = default);
    Task<List<Employee>> GetOnLeaveAsync(CancellationToken ct = default);
}
