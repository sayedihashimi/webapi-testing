using HorizonHR.Models;
using HorizonHR.Models.Enums;

namespace HorizonHR.Services;

public interface IEmployeeService
{
    Task<PaginatedList<Employee>> GetAllAsync(string? search, int? departmentId, EmploymentType? employmentType, EmployeeStatus? status, int pageNumber, int pageSize, CancellationToken ct = default);
    Task<Employee?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Employee> CreateAsync(string firstName, string lastName, string email, string? phone, DateOnly dateOfBirth, DateOnly hireDate, int departmentId, string jobTitle, EmploymentType employmentType, decimal salary, int? managerId, string? profileImageUrl, string? address, string? city, string? state, string? zipCode, CancellationToken ct = default);
    Task<Employee> UpdateAsync(int id, string firstName, string lastName, string email, string? phone, DateOnly dateOfBirth, DateOnly hireDate, int departmentId, string jobTitle, EmploymentType employmentType, decimal salary, int? managerId, EmployeeStatus status, string? profileImageUrl, string? address, string? city, string? state, string? zipCode, CancellationToken ct = default);
    Task TerminateAsync(int id, DateOnly terminationDate, CancellationToken ct = default);
    Task<List<Employee>> GetDirectReportsAsync(int managerId, CancellationToken ct = default);
    Task<List<Employee>> GetByDepartmentAsync(int departmentId, CancellationToken ct = default);
    Task<string> GenerateEmployeeNumberAsync(CancellationToken ct = default);
}
