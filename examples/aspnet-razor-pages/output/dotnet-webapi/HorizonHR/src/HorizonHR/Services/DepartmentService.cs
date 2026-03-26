using HorizonHR.Data;
using HorizonHR.Models;
using HorizonHR.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HorizonHR.Services;

public sealed class DepartmentService(HorizonDbContext db, ILogger<DepartmentService> logger) : IDepartmentService
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

    public async Task<Department> CreateAsync(string name, string code, string? description, int? parentDepartmentId, int? managerId, CancellationToken ct = default)
    {
        if (parentDepartmentId.HasValue)
        {
            var parent = await db.Departments.FindAsync([parentDepartmentId.Value], ct)
                ?? throw new InvalidOperationException("Parent department not found.");
        }

        if (managerId.HasValue)
        {
            var manager = await db.Employees.FindAsync([managerId.Value], ct)
                ?? throw new InvalidOperationException("Manager not found.");
        }

        var dept = new Department
        {
            Name = name,
            Code = code,
            Description = description,
            ParentDepartmentId = parentDepartmentId,
            ManagerId = managerId,
            IsActive = true
        };

        db.Departments.Add(dept);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Department created: {DepartmentName} ({Code})", name, code);
        return dept;
    }

    public async Task<Department> UpdateAsync(int id, string name, string code, string? description, int? parentDepartmentId, int? managerId, bool isActive, CancellationToken ct = default)
    {
        var dept = await db.Departments.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Department {id} not found.");

        if (parentDepartmentId == id)
            throw new InvalidOperationException("A department cannot be its own parent.");

        if (parentDepartmentId.HasValue)
        {
            // Check for circular reference
            var current = await db.Departments.FindAsync([parentDepartmentId.Value], ct);
            while (current != null)
            {
                if (current.Id == id)
                    throw new InvalidOperationException("Circular department hierarchy detected.");
                if (current.ParentDepartmentId.HasValue)
                    current = await db.Departments.FindAsync([current.ParentDepartmentId.Value], ct);
                else
                    current = null;
            }
        }

        if (managerId.HasValue)
        {
            var manager = await db.Employees.FindAsync([managerId.Value], ct)
                ?? throw new InvalidOperationException("Manager not found.");
            if (manager.DepartmentId != id)
                throw new InvalidOperationException("Department manager must belong to this department.");
        }

        dept.Name = name;
        dept.Code = code;
        dept.Description = description;
        dept.ParentDepartmentId = parentDepartmentId;
        dept.ManagerId = managerId;
        dept.IsActive = isActive;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Department updated: {DepartmentName}", name);
        return dept;
    }

    public async Task<List<Department>> GetAllSimpleAsync(CancellationToken ct = default)
    {
        return await db.Departments.AsNoTracking().OrderBy(d => d.Name).ToListAsync(ct);
    }
}
