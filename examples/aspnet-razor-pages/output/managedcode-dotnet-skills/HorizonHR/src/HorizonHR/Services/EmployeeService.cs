using HorizonHR.Data;
using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HorizonHR.Services;

public class EmployeeService(ApplicationDbContext db, ILogger<EmployeeService> logger) : IEmployeeService
{
    public async Task<(List<Employee> Items, int TotalCount)> GetPagedAsync(
        int pageNumber, int pageSize,
        string? searchTerm = null, int? departmentId = null,
        EmploymentType? employmentType = null, EmployeeStatus? status = null)
    {
        var query = db.Employees
            .Include(e => e.Department)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.ToLower();
            query = query.Where(e =>
                e.FirstName.ToLower().Contains(term) ||
                e.LastName.ToLower().Contains(term) ||
                e.Email.ToLower().Contains(term) ||
                e.EmployeeNumber.ToLower().Contains(term));
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
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Employee?> GetByIdAsync(int id)
    {
        return await db.Employees
            .Include(e => e.Department)
            .Include(e => e.Manager)
            .Include(e => e.DirectReports)
            .Include(e => e.LeaveBalances).ThenInclude(lb => lb.LeaveType)
            .Include(e => e.EmployeeSkills).ThenInclude(es => es.Skill)
            .Include(e => e.PerformanceReviews.OrderByDescending(r => r.ReviewPeriodEnd).Take(5))
            .AsSplitQuery()
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Employee> CreateAsync(Employee employee)
    {
        employee.EmployeeNumber = await GenerateEmployeeNumberAsync();

        db.Employees.Add(employee);
        await db.SaveChangesAsync();

        // Initialize leave balances for the current year
        var leaveTypes = await db.LeaveTypes.ToListAsync();
        foreach (var lt in leaveTypes)
        {
            db.LeaveBalances.Add(new LeaveBalance
            {
                EmployeeId = employee.Id,
                LeaveTypeId = lt.Id,
                Year = DateTime.UtcNow.Year,
                TotalDays = lt.DefaultDaysPerYear
            });
        }
        await db.SaveChangesAsync();

        logger.LogInformation("Employee {Name} ({Number}) created", employee.FullName, employee.EmployeeNumber);
        return employee;
    }

    public async Task UpdateAsync(Employee employee)
    {
        db.Employees.Update(employee);
        await db.SaveChangesAsync();
    }

    public async Task TerminateAsync(int employeeId, DateOnly terminationDate)
    {
        var employee = await db.Employees.FindAsync(employeeId)
            ?? throw new InvalidOperationException("Employee not found.");

        employee.Status = EmployeeStatus.Terminated;
        employee.TerminationDate = terminationDate;

        // Cancel all submitted leave requests
        var pendingLeaves = await db.LeaveRequests
            .Where(lr => lr.EmployeeId == employeeId && lr.Status == LeaveRequestStatus.Submitted)
            .ToListAsync();
        foreach (var leave in pendingLeaves)
            leave.Status = LeaveRequestStatus.Cancelled;

        // Remove as department manager
        var managedDepts = await db.Departments
            .Where(d => d.ManagerId == employeeId)
            .ToListAsync();
        foreach (var dept in managedDepts)
            dept.ManagerId = null;

        // Remove as manager from direct reports
        var reports = await db.Employees
            .Where(e => e.ManagerId == employeeId)
            .ToListAsync();
        foreach (var report in reports)
            report.ManagerId = null;

        await db.SaveChangesAsync();
        logger.LogInformation("Employee {Id} terminated on {Date}", employeeId, terminationDate);
    }

    public async Task<string> GenerateEmployeeNumberAsync()
    {
        var lastNumber = await db.Employees
            .OrderByDescending(e => e.EmployeeNumber)
            .Select(e => e.EmployeeNumber)
            .FirstOrDefaultAsync();

        var nextSeq = 1;
        if (lastNumber is not null && lastNumber.StartsWith("EMP-"))
        {
            if (int.TryParse(lastNumber[4..], out var parsed))
                nextSeq = parsed + 1;
        }

        return $"EMP-{nextSeq:D4}";
    }

    public async Task<List<Employee>> GetDirectReportsAsync(int employeeId)
    {
        return await db.Employees
            .Include(e => e.Department)
            .AsNoTracking()
            .Where(e => e.ManagerId == employeeId)
            .OrderBy(e => e.LastName)
            .ToListAsync();
    }

    public async Task<List<Employee>> GetByDepartmentAsync(int departmentId)
    {
        return await db.Employees
            .AsNoTracking()
            .Where(e => e.DepartmentId == departmentId)
            .OrderBy(e => e.LastName)
            .ToListAsync();
    }
}
