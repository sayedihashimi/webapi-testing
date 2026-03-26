using HorizonHR.Models;

namespace HorizonHR.Services.Interfaces;

public interface IDepartmentService
{
    Task<(List<Department> Items, int TotalCount)> GetDepartmentsAsync(int page, int pageSize);
    Task<List<Department>> GetAllDepartmentsAsync();
    Task<Department?> GetDepartmentByIdAsync(int id);
    Task<Department> CreateDepartmentAsync(Department department);
    Task<Department> UpdateDepartmentAsync(Department department);
    Task<bool> ValidateHierarchyAsync(int departmentId, int? parentDepartmentId);
    Task<int> GetEmployeeCountAsync(int departmentId);
}
