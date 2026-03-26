using HorizonHR.Data;
using HorizonHR.Models;
using HorizonHR.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HorizonHR.Services;

public class EmployeeService : IEmployeeService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(ApplicationDbContext context, ILogger<EmployeeService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(List<Employee> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search = null, int? departmentId = null,
        EmploymentType? employmentType = null, EmployeeStatus? status = null)
    {
        var query = _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Manager)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(e =>
                e.FirstName.ToLower().Contains(s) ||
                e.LastName.ToLower().Contains(s) ||
                e.Email.ToLower().Contains(s) ||
                e.EmployeeNumber.ToLower().Contains(s));
        }

        if (departmentId.HasValue)
            query = query.Where(e => e.DepartmentId == departmentId.Value);

        if (employmentType.HasValue)
            query = query.Where(e => e.EmploymentType == employmentType.Value);

        if (status.HasValue)
            query = query.Where(e => e.Status == status.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(e => e.LastName).ThenBy(e => e.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Employee?> GetByIdAsync(int id)
    {
        return await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.Manager)
            .Include(e => e.DirectReports)
            .Include(e => e.LeaveBalances).ThenInclude(lb => lb.LeaveType)
            .Include(e => e.EmployeeSkills).ThenInclude(es => es.Skill)
            .Include(e => e.PerformanceReviews).ThenInclude(pr => pr.Reviewer)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Employee> CreateAsync(Employee employee)
    {
        employee.EmployeeNumber = await GenerateEmployeeNumberAsync();

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        // Initialize leave balances
        var leaveTypes = await _context.LeaveTypes.ToListAsync();
        foreach (var lt in leaveTypes)
        {
            _context.LeaveBalances.Add(new LeaveBalance
            {
                EmployeeId = employee.Id,
                LeaveTypeId = lt.Id,
                Year = DateTime.UtcNow.Year,
                TotalDays = lt.DefaultDaysPerYear
            });
        }
        await _context.SaveChangesAsync();

        _logger.LogInformation("Employee created: {FullName} ({EmployeeNumber})", employee.FullName, employee.EmployeeNumber);
        return employee;
    }

    public async Task UpdateAsync(Employee employee)
    {
        _context.Employees.Update(employee);
        await _context.SaveChangesAsync();
    }

    public async Task TerminateAsync(int id, DateOnly terminationDate)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null) throw new InvalidOperationException("Employee not found");

        employee.Status = EmployeeStatus.Terminated;
        employee.TerminationDate = terminationDate;

        // Cancel submitted leave requests
        var pendingLeaves = await _context.LeaveRequests
            .Where(lr => lr.EmployeeId == id && lr.Status == LeaveRequestStatus.Submitted)
            .ToListAsync();
        foreach (var lr in pendingLeaves)
            lr.Status = LeaveRequestStatus.Cancelled;

        // Remove as department manager
        var managedDepts = await _context.Departments
            .Where(d => d.ManagerId == id)
            .ToListAsync();
        foreach (var dept in managedDepts)
            dept.ManagerId = null;

        // Remove as employee manager
        var directReports = await _context.Employees
            .Where(e => e.ManagerId == id)
            .ToListAsync();
        foreach (var report in directReports)
            report.ManagerId = null;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Employee terminated: {FullName} ({EmployeeNumber})", employee.FullName, employee.EmployeeNumber);
    }

    public async Task<List<Employee>> GetDirectReportsAsync(int managerId)
    {
        return await _context.Employees
            .Include(e => e.Department)
            .Where(e => e.ManagerId == managerId)
            .OrderBy(e => e.LastName)
            .ToListAsync();
    }

    public async Task<string> GenerateEmployeeNumberAsync()
    {
        var maxNumber = await _context.Employees
            .Select(e => e.EmployeeNumber)
            .ToListAsync();

        int max = 0;
        foreach (var num in maxNumber)
        {
            if (num.StartsWith("EMP-") && int.TryParse(num[4..], out var n))
            {
                if (n > max) max = n;
            }
        }

        return $"EMP-{(max + 1):D4}";
    }

    public async Task<List<Employee>> GetAllActiveAsync()
    {
        return await _context.Employees
            .Where(e => e.Status != EmployeeStatus.Terminated)
            .Include(e => e.Department)
            .OrderBy(e => e.LastName)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.Employees.CountAsync(e => e.Status != EmployeeStatus.Terminated);
    }

    public async Task<List<Employee>> GetRecentHiresAsync(int days)
    {
        var cutoff = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-days));
        return await _context.Employees
            .Where(e => e.HireDate >= cutoff && e.Status != EmployeeStatus.Terminated)
            .Include(e => e.Department)
            .OrderByDescending(e => e.HireDate)
            .ToListAsync();
    }

    public async Task<List<Employee>> GetOnLeaveAsync()
    {
        return await _context.Employees
            .Where(e => e.Status == EmployeeStatus.OnLeave)
            .Include(e => e.Department)
            .ToListAsync();
    }
}
