using HorizonHR.Models;
using HorizonHR.Models.Enums;

namespace HorizonHR.Services.Interfaces;

public interface IEmployeeService
{
    Task<(List<Employee> Items, int TotalCount)> GetEmployeesAsync(string? searchTerm, int? departmentId, EmploymentType? employmentType, EmployeeStatus? status, int page, int pageSize);
    Task<Employee?> GetEmployeeByIdAsync(int id);
    Task<Employee> CreateEmployeeAsync(Employee employee);
    Task<Employee> UpdateEmployeeAsync(Employee employee);
    Task TerminateEmployeeAsync(int employeeId, DateOnly terminationDate);
    Task<List<Employee>> GetDirectReportsAsync(int employeeId);
    Task<List<Employee>> GetEmployeesByDepartmentAsync(int departmentId);
    Task<string> GenerateEmployeeNumberAsync();
}
