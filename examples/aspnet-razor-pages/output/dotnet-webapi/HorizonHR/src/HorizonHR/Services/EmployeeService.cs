using HorizonHR.Data;
using HorizonHR.Models;
using HorizonHR.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HorizonHR.Services;

public sealed class EmployeeService(HorizonDbContext db, ILogger<EmployeeService> logger) : IEmployeeService
{
    public async Task<PaginatedList<Employee>> GetAllAsync(string? search, int? departmentId, EmploymentType? employmentType, EmployeeStatus? status, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = db.Employees
            .AsNoTracking()
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
            .Include(e => e.EmployeeSkills).ThenInclude(es => es.Skill)
            .Include(e => e.PerformanceReviews.OrderByDescending(r => r.ReviewPeriodEnd).Take(5))
            .FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public async Task<Employee> CreateAsync(string firstName, string lastName, string email, string? phone, DateOnly dateOfBirth, DateOnly hireDate, int departmentId, string jobTitle, EmploymentType employmentType, decimal salary, int? managerId, string? profileImageUrl, string? address, string? city, string? state, string? zipCode, CancellationToken ct = default)
    {
        var empNumber = await GenerateEmployeeNumberAsync(ct);

        var employee = new Employee
        {
            EmployeeNumber = empNumber,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Phone = phone,
            DateOfBirth = dateOfBirth,
            HireDate = hireDate,
            DepartmentId = departmentId,
            JobTitle = jobTitle,
            EmploymentType = employmentType,
            Salary = salary,
            ManagerId = managerId,
            Status = EmployeeStatus.Active,
            ProfileImageUrl = profileImageUrl,
            Address = address,
            City = city,
            State = state,
            ZipCode = zipCode
        };

        db.Employees.Add(employee);
        await db.SaveChangesAsync(ct);

        // Initialize leave balances for current year
        var currentYear = DateTime.UtcNow.Year;
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

        logger.LogInformation("Employee created: {EmployeeNumber} - {Name}", empNumber, $"{firstName} {lastName}");
        return employee;
    }

    public async Task<Employee> UpdateAsync(int id, string firstName, string lastName, string email, string? phone, DateOnly dateOfBirth, DateOnly hireDate, int departmentId, string jobTitle, EmploymentType employmentType, decimal salary, int? managerId, EmployeeStatus status, string? profileImageUrl, string? address, string? city, string? state, string? zipCode, CancellationToken ct = default)
    {
        var employee = await db.Employees.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Employee {id} not found.");

        var oldDepartmentId = employee.DepartmentId;

        employee.FirstName = firstName;
        employee.LastName = lastName;
        employee.Email = email;
        employee.Phone = phone;
        employee.DateOfBirth = dateOfBirth;
        employee.HireDate = hireDate;
        employee.DepartmentId = departmentId;
        employee.JobTitle = jobTitle;
        employee.EmploymentType = employmentType;
        employee.Salary = salary;
        employee.Status = status;
        employee.ProfileImageUrl = profileImageUrl;
        employee.Address = address;
        employee.City = city;
        employee.State = state;
        employee.ZipCode = zipCode;

        // If department changed, clear manager if old manager isn't in new department
        if (oldDepartmentId != departmentId && managerId.HasValue)
        {
            var manager = await db.Employees.FindAsync([managerId.Value], ct);
            if (manager == null || manager.DepartmentId != departmentId)
                employee.ManagerId = null;
            else
                employee.ManagerId = managerId;
        }
        else
        {
            employee.ManagerId = managerId;
        }

        // If department changed, remove as department manager from old department
        if (oldDepartmentId != departmentId)
        {
            var oldDept = await db.Departments.FirstOrDefaultAsync(d => d.ManagerId == id, ct);
            if (oldDept != null)
            {
                oldDept.ManagerId = null;
                logger.LogInformation("Department manager changed: {Department} manager cleared due to transfer", oldDept.Name);
            }
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Employee updated: {EmployeeNumber}", employee.EmployeeNumber);
        return employee;
    }

    public async Task TerminateAsync(int id, DateOnly terminationDate, CancellationToken ct = default)
    {
        var employee = await db.Employees.FindAsync([id], ct)
            ?? throw new KeyNotFoundException($"Employee {id} not found.");

        employee.Status = EmployeeStatus.Terminated;
        employee.TerminationDate = terminationDate;

        // Cancel all submitted leave requests
        var submittedLeaves = await db.LeaveRequests
            .Where(lr => lr.EmployeeId == id && lr.Status == LeaveRequestStatus.Submitted)
            .ToListAsync(ct);
        foreach (var lr in submittedLeaves)
            lr.Status = LeaveRequestStatus.Cancelled;

        // Remove as department manager
        var managedDepts = await db.Departments
            .Where(d => d.ManagerId == id)
            .ToListAsync(ct);
        foreach (var dept in managedDepts)
            dept.ManagerId = null;

        // Remove as manager of direct reports
        var directReports = await db.Employees
            .Where(e => e.ManagerId == id)
            .ToListAsync(ct);
        foreach (var dr in directReports)
            dr.ManagerId = null;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Employee terminated: {EmployeeNumber} on {Date}", employee.EmployeeNumber, terminationDate);
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

    public async Task<List<Employee>> GetByDepartmentAsync(int departmentId, CancellationToken ct = default)
    {
        return await db.Employees
            .AsNoTracking()
            .Where(e => e.DepartmentId == departmentId)
            .OrderBy(e => e.LastName)
            .ToListAsync(ct);
    }

    public async Task<string> GenerateEmployeeNumberAsync(CancellationToken ct = default)
    {
        var maxNumber = await db.Employees
            .Select(e => e.EmployeeNumber)
            .ToListAsync(ct);

        var max = 0;
        foreach (var num in maxNumber)
        {
            if (num.StartsWith("EMP-") && int.TryParse(num[4..], out var n) && n > max)
                max = n;
        }

        return $"EMP-{(max + 1):D4}";
    }
}
