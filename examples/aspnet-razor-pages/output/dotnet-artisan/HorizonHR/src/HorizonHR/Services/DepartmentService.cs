using HorizonHR.Data;
using HorizonHR.Models;
using Microsoft.EntityFrameworkCore;

namespace HorizonHR.Services;

public sealed class DepartmentService(ApplicationDbContext db, ILogger<DepartmentService> logger) : IDepartmentService
{
    public async Task<PaginatedList<Department>> GetAllAsync(int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = db.Departments
            .AsNoTracking()
            .Include(d => d.Manager)
            .Include(d => d.ParentDepartment)
            .Include(d => d.Employees)
            .OrderBy(d => d.Name);

        return await PaginatedList<Department>.CreateAsync(query, pageNumber, pageSize, ct);
    }

    public async Task<List<Department>> GetHierarchyAsync(CancellationToken ct = default)
    {
        return await db.Departments
            .AsNoTracking()
            .Include(d => d.Manager)
            .Include(d => d.Employees)
            .Include(d => d.ChildDepartments)
            .Where(d => d.ParentDepartmentId == null)
            .OrderBy(d => d.Name)
            .ToListAsync(ct);
    }

    public async Task<Department?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await db.Departments
            .Include(d => d.Manager)
            .Include(d => d.ParentDepartment)
            .Include(d => d.Employees)
            .Include(d => d.ChildDepartments)
            .FirstOrDefaultAsync(d => d.Id == id, ct);
    }

    public async Task<List<Department>> GetAllActiveAsync(CancellationToken ct = default)
    {
        return await db.Departments
            .AsNoTracking()
            .Where(d => d.IsActive)
            .OrderBy(d => d.Name)
            .ToListAsync(ct);
    }

    public async Task CreateAsync(Department department, CancellationToken ct = default)
    {
        db.Departments.Add(department);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Department created: {DepartmentName} ({DepartmentCode})", department.Name, department.Code);
    }

    public async Task UpdateAsync(Department department, CancellationToken ct = default)
    {
        db.Departments.Update(department);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Department updated: {DepartmentName}", department.Name);
    }

    public async Task<bool> HasCircularReference(int departmentId, int? parentId, CancellationToken ct = default)
    {
        if (parentId is null)
        {
            return false;
        }

        if (parentId == departmentId)
        {
            return true;
        }

        var visited = new HashSet<int> { departmentId };
        var currentId = parentId;

        while (currentId is not null)
        {
            if (!visited.Add(currentId.Value))
            {
                return true;
            }

            var dept = await db.Departments
                .AsNoTracking()
                .Where(d => d.Id == currentId.Value)
                .Select(d => d.ParentDepartmentId)
                .FirstOrDefaultAsync(ct);

            currentId = dept;
        }

        return false;
    }
}
