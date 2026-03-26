using HorizonHR.Data;
using HorizonHR.Models;
using Microsoft.EntityFrameworkCore;

namespace HorizonHR.Services;

public class DepartmentService : IDepartmentService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DepartmentService> _logger;

    public DepartmentService(ApplicationDbContext context, ILogger<DepartmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Department>> GetAllAsync()
    {
        return await _context.Departments
            .Include(d => d.Manager)
            .Include(d => d.ParentDepartment)
            .Include(d => d.Employees)
            .Include(d => d.ChildDepartments)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }

    public async Task<Department?> GetByIdAsync(int id)
    {
        return await _context.Departments
            .Include(d => d.Manager)
            .Include(d => d.ParentDepartment)
            .Include(d => d.Employees).ThenInclude(e => e.Manager)
            .Include(d => d.ChildDepartments)
            .FirstOrDefaultAsync(d => d.Id == id);
    }

    public async Task<Department> CreateAsync(Department department)
    {
        _context.Departments.Add(department);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Department created: {Name} ({Code})", department.Name, department.Code);
        return department;
    }

    public async Task UpdateAsync(Department department)
    {
        _context.Departments.Update(department);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Department updated: {Name} ({Code})", department.Name, department.Code);
    }

    public async Task<bool> IsCircularReference(int departmentId, int? parentDepartmentId)
    {
        if (parentDepartmentId == null) return false;
        if (departmentId == parentDepartmentId) return true;

        var visited = new HashSet<int> { departmentId };
        var currentId = parentDepartmentId;

        while (currentId != null)
        {
            if (visited.Contains(currentId.Value)) return true;
            visited.Add(currentId.Value);

            var dept = await _context.Departments.FindAsync(currentId);
            currentId = dept?.ParentDepartmentId;
        }

        return false;
    }

    public async Task<List<Department>> GetTopLevelDepartmentsAsync()
    {
        return await _context.Departments
            .Where(d => d.ParentDepartmentId == null)
            .Include(d => d.Manager)
            .Include(d => d.Employees)
            .Include(d => d.ChildDepartments).ThenInclude(c => c.Employees)
            .Include(d => d.ChildDepartments).ThenInclude(c => c.Manager)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }
}
