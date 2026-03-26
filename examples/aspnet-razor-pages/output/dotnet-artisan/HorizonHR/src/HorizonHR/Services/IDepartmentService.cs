using HorizonHR.Models;

namespace HorizonHR.Services;

public interface IDepartmentService
{
    Task<PaginatedList<Department>> GetAllAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    Task<List<Department>> GetHierarchyAsync(CancellationToken ct = default);
    Task<Department?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<Department>> GetAllActiveAsync(CancellationToken ct = default);
    Task CreateAsync(Department department, CancellationToken ct = default);
    Task UpdateAsync(Department department, CancellationToken ct = default);
    Task<bool> HasCircularReference(int departmentId, int? parentId, CancellationToken ct = default);
}
