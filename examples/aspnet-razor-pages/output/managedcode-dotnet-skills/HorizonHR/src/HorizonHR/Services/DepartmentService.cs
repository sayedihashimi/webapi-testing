using HorizonHR.Data;
using HorizonHR.Models;
using HorizonHR.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HorizonHR.Services;

public class DepartmentService(ApplicationDbContext db, ILogger<DepartmentService> logger) : IDepartmentService
{
    public async Task<List<Department>> GetAllAsync()
    {
        return await db.Departments
            .Include(d => d.Manager)
            .Include(d => d.ParentDepartment)
            .Include(d => d.ChildDepartments)
            .AsNoTracking()
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    public async Task<Department?> GetByIdAsync(int id)
    {
        return await db.Departments
            .Include(d => d.Manager)
            .Include(d => d.ParentDepartment)
            .Include(d => d.ChildDepartments)
            .Include(d => d.Employees)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<Department> CreateAsync(Department department)
    {
        if (department.ParentDepartmentId.HasValue)
        {
            if (!await ValidateHierarchy(0, department.ParentDepartmentId))
                throw new InvalidOperationException("Invalid department hierarchy.");
        }

        db.Departments.Add(department);
        await db.SaveChangesAsync();
        logger.LogInformation("Department {Name} ({Code}) created", department.Name, department.Code);
        return department;
    }

    public async Task UpdateAsync(Department department)
    {
        db.Departments.Update(department);
        await db.SaveChangesAsync();
        logger.LogInformation("Department {Name} updated", department.Name);
    }

    public async Task<bool> ValidateHierarchy(int departmentId, int? parentDepartmentId)
    {
        if (!parentDepartmentId.HasValue) return true;
        if (departmentId == parentDepartmentId.Value) return false;

        // Walk up the chain to detect cycles
        var currentId = parentDepartmentId;
        var visited = new HashSet<int>();
        while (currentId.HasValue)
        {
            if (!visited.Add(currentId.Value)) return false;
            if (currentId.Value == departmentId) return false;
            var parent = await db.Departments.AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == currentId.Value);
            currentId = parent?.ParentDepartmentId;
        }
        return true;
    }

    public async Task<int> GetEmployeeCountAsync(int departmentId)
    {
        return await db.Employees.CountAsync(e => e.DepartmentId == departmentId);
    }
}
