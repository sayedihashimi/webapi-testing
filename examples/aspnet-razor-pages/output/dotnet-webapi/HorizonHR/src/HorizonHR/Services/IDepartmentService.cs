using HorizonHR.Models;
using HorizonHR.Models.Enums;

namespace HorizonHR.Services;

public interface IDepartmentService
{
    Task<PaginatedList<Department>> GetAllAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    Task<List<Department>> GetHierarchyAsync(CancellationToken ct = default);
    Task<Department?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Department> CreateAsync(string name, string code, string? description, int? parentDepartmentId, int? managerId, CancellationToken ct = default);
    Task<Department> UpdateAsync(int id, string name, string code, string? description, int? parentDepartmentId, int? managerId, bool isActive, CancellationToken ct = default);
    Task<List<Department>> GetAllSimpleAsync(CancellationToken ct = default);
}
