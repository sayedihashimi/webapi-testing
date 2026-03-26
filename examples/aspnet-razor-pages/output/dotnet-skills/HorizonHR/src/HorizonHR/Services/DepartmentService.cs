using HorizonHR.Data;
using HorizonHR.Models;
using HorizonHR.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HorizonHR.Services;

public sealed class DepartmentService : IDepartmentService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DepartmentService> _logger;

    public DepartmentService(ApplicationDbContext context, ILogger<DepartmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(List<Department> Items, int TotalCount)> GetDepartmentsAsync(int page, int pageSize)
    {
        var query = _context.Departments
            .AsNoTracking()
            .Include(d => d.Manager)
            .Include(d => d.ParentDepartment)
            .OrderBy(d => d.Name);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<List<Department>> GetAllDepartmentsAsync()
    {
        return await _context.Departments
            .AsNoTracking()
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    public async Task<Department?> GetDepartmentByIdAsync(int id)
    {
        return await _context.Departments
            .AsNoTracking()
            .Include(d => d.Manager)
            .Include(d => d.ParentDepartment)
            .Include(d => d.Employees)
            .Include(d => d.ChildDepartments)
            .AsSplitQuery()
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<Department> CreateDepartmentAsync(Department department)
    {
        department.CreatedAt = DateTime.UtcNow;
        department.UpdatedAt = DateTime.UtcNow;

        _context.Departments.Add(department);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created department {DepartmentName} with ID {DepartmentId}", department.Name, department.Id);

        return department;
    }

    public async Task<Department> UpdateDepartmentAsync(Department department)
    {
        department.UpdatedAt = DateTime.UtcNow;

        _context.Departments.Update(department);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated department {DepartmentName} with ID {DepartmentId}", department.Name, department.Id);

        return department;
    }

    public async Task<bool> ValidateHierarchyAsync(int departmentId, int? parentDepartmentId)
    {
        if (parentDepartmentId == null)
        {
            return true;
        }

        // A department cannot be its own parent
        if (departmentId == parentDepartmentId.Value)
        {
            return false;
        }

        // Walk up the parent chain to detect circular references
        var currentId = parentDepartmentId.Value;
        var visited = new HashSet<int> { departmentId };

        while (true)
        {
            if (visited.Contains(currentId))
            {
                return false;
            }

            visited.Add(currentId);

            var parent = await _context.Departments
                .AsNoTracking()
                .Where(d => d.Id == currentId)
                .Select(d => d.ParentDepartmentId)
                .FirstOrDefaultAsync();

            if (parent == null)
            {
                break;
            }

            currentId = parent.Value;
        }

        return true;
    }

    public async Task<int> GetEmployeeCountAsync(int departmentId)
    {
        return await _context.Employees
            .AsNoTracking()
            .CountAsync(e => e.DepartmentId == departmentId);
    }
}
