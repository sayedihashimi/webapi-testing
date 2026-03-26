using HorizonHR.Data;
using HorizonHR.Models;
using Microsoft.EntityFrameworkCore;

namespace HorizonHR.Services;

public sealed class EmployeeService(ApplicationDbContext db, ILogger<EmployeeService> logger) : IEmployeeService
{
    public async Task<PaginatedList<Employee>> GetAllAsync(int pageNumber, int pageSize, string? search = null,
        int? departmentId = null, EmploymentType? employmentType = null, EmployeeStatus? status = null,
        CancellationToken ct = default)
    {
        var query = db.Employees
            .AsNoTracking()
            .Include(e => e.Department)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(e =>
                e.FirstName.ToLower().Contains(term) ||
                e.LastName.ToLower().Contains(term) ||
                e.Email.ToLower().Contains(term) ||
                e.EmployeeNumber.ToLower().Contains(term));
        }

        if (departmentId.HasValue)
        {
            query = query.Where(e => e.DepartmentId == departmentId.Value);
        }

        if (employmentType.HasValue)
        {
            query = query.Where(e => e.EmploymentType == employmentType.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(e => e.Status == status.Value);
        }

        query = query.OrderBy(e => e.LastName).ThenBy(e => e.FirstName);

        return await PaginatedList<Employee>.CreateAsync(query, pageNumber, pageSize, ct);
    }

    public async Task<Employee?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await db.Employees
            .Include(e => e.Department)
            .Include(e => e.Manager)
            .Include(e => e.DirectReports)
            .Include(e => e.LeaveBalances).ThenInclude(lb => lb.LeaveType)
            .Include(e => e.PerformanceReviews.OrderByDescending(r => r.ReviewPeriodEnd).Take(5))
            .Include(e => e.EmployeeSkills).ThenInclude(es => es.Skill)
            .AsSplitQuery()
            .FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public async Task<List<Employee>> GetByDepartmentAsync(int departmentId, CancellationToken ct = default)
    {
        return await db.Employees
            .AsNoTracking()
            .Where(e => e.DepartmentId == departmentId)
            .OrderBy(e => e.LastName)
            .ToListAsync(ct);
    }

    public async Task<List<Employee>> GetAllActiveAsync(CancellationToken ct = default)
    {
        return await db.Employees
            .AsNoTracking()
            .Where(e => e.Status != EmployeeStatus.Terminated)
            .OrderBy(e => e.LastName).ThenBy(e => e.FirstName)
            .ToListAsync(ct);
    }

    public async Task<List<Employee>> GetDirectReportsAsync(int managerId, CancellationToken ct = default)
    {
        return await db.Employees
            .AsNoTracking()
            .Include(e => e.Department)
            .Where(e => e.ManagerId == managerId)
            .OrderBy(e => e.LastName)
            .ToListAsync(ct);
    }

    public async Task CreateAsync(Employee employee, CancellationToken ct = default)
    {
        employee.EmployeeNumber = await GenerateEmployeeNumberAsync(ct);

        db.Employees.Add(employee);
        await db.SaveChangesAsync(ct);

        // Initialize leave balances for current year
        var currentYear = DateTime.Today.Year;
        var leaveTypes = await db.LeaveTypes.ToListAsync(ct);
        foreach (var lt in leaveTypes)
        {
            db.LeaveBalances.Add(new LeaveBalance
            {
                EmployeeId = employee.Id,
                LeaveTypeId = lt.Id,
                Year = currentYear,
                TotalDays = lt.DefaultDaysPerYear,
                UsedDays = 0,
                CarriedOverDays = 0
            });
        }
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Employee created: {EmployeeNumber} {FullName}", employee.EmployeeNumber, employee.FullName);
    }

    public async Task UpdateAsync(Employee employee, CancellationToken ct = default)
    {
        db.Employees.Update(employee);
        await db.SaveChangesAsync(ct);
    }

    public async Task TerminateAsync(int employeeId, DateOnly terminationDate, CancellationToken ct = default)
    {
        var employee = await db.Employees.FindAsync([employeeId], ct)
            ?? throw new InvalidOperationException($"Employee {employeeId} not found.");

        employee.Status = EmployeeStatus.Terminated;
        employee.TerminationDate = terminationDate;

        // Cancel submitted leave requests
        var submittedLeaves = await db.LeaveRequests
            .Where(lr => lr.EmployeeId == employeeId && lr.Status == LeaveRequestStatus.Submitted)
            .ToListAsync(ct);
        foreach (var lr in submittedLeaves)
        {
            lr.Status = LeaveRequestStatus.Cancelled;
        }

        // Remove as department manager
        var managedDepts = await db.Departments
            .Where(d => d.ManagerId == employeeId)
            .ToListAsync(ct);
        foreach (var dept in managedDepts)
        {
            dept.ManagerId = null;
        }

        // Remove as manager from direct reports
        var directReports = await db.Employees
            .Where(e => e.ManagerId == employeeId)
            .ToListAsync(ct);
        foreach (var report in directReports)
        {
            report.ManagerId = null;
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Employee terminated: {EmployeeNumber} {FullName}", employee.EmployeeNumber, employee.FullName);
    }

    public async Task<string> GenerateEmployeeNumberAsync(CancellationToken ct = default)
    {
        var lastNumber = await db.Employees
            .OrderByDescending(e => e.EmployeeNumber)
            .Select(e => e.EmployeeNumber)
            .FirstOrDefaultAsync(ct);

        var nextNum = 1;
        if (lastNumber is not null && lastNumber.StartsWith("EMP-"))
        {
            if (int.TryParse(lastNumber[4..], out var parsed))
            {
                nextNum = parsed + 1;
            }
        }

        return $"EMP-{nextNum:D4}";
    }

    public async Task<int> GetTotalCountAsync(CancellationToken ct = default)
    {
        return await db.Employees.CountAsync(e => e.Status != EmployeeStatus.Terminated, ct);
    }

    public async Task<List<Employee>> GetRecentHiresAsync(int days, CancellationToken ct = default)
    {
        var cutoff = DateOnly.FromDateTime(DateTime.Today.AddDays(-days));
        return await db.Employees
            .AsNoTracking()
            .Include(e => e.Department)
            .Where(e => e.HireDate >= cutoff && e.Status != EmployeeStatus.Terminated)
            .OrderByDescending(e => e.HireDate)
            .ToListAsync(ct);
    }

    public async Task<List<Employee>> GetOnLeaveAsync(CancellationToken ct = default)
    {
        return await db.Employees
            .AsNoTracking()
            .Include(e => e.Department)
            .Where(e => e.Status == EmployeeStatus.OnLeave)
            .OrderBy(e => e.LastName)
            .ToListAsync(ct);
    }
}
