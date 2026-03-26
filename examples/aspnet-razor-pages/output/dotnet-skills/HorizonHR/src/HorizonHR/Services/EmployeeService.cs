using HorizonHR.Data;
using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HorizonHR.Services;

public sealed class EmployeeService : IEmployeeService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(ApplicationDbContext context, ILogger<EmployeeService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(List<Employee> Items, int TotalCount)> GetEmployeesAsync(
        string? searchTerm, int? departmentId, EmploymentType? employmentType,
        EmployeeStatus? status, int page, int pageSize)
    {
        var query = _context.Employees
            .AsNoTracking()
            .Include(e => e.Department)
            .Include(e => e.Manager)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = $"%{searchTerm.Trim()}%";
            query = query.Where(e =>
                EF.Functions.Like(e.FirstName, term) ||
                EF.Functions.Like(e.LastName, term) ||
                EF.Functions.Like(e.Email, term) ||
                EF.Functions.Like(e.EmployeeNumber, term));
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

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Employee?> GetEmployeeByIdAsync(int id)
    {
        return await _context.Employees
            .AsNoTracking()
            .Include(e => e.Department)
            .Include(e => e.Manager)
            .Include(e => e.DirectReports)
            .Include(e => e.LeaveBalances)
                .ThenInclude(lb => lb.LeaveType)
            .Include(e => e.PerformanceReviews)
            .Include(e => e.EmployeeSkills)
                .ThenInclude(es => es.Skill)
            .AsSplitQuery()
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Employee> CreateEmployeeAsync(Employee employee)
    {
        employee.EmployeeNumber = await GenerateEmployeeNumberAsync();
        employee.CreatedAt = DateTime.UtcNow;
        employee.UpdatedAt = DateTime.UtcNow;

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        // Create leave balances for all leave types for the current year
        var currentYear = DateTime.UtcNow.Year;
        var leaveTypes = await _context.LeaveTypes.AsNoTracking().ToListAsync();

        foreach (var leaveType in leaveTypes)
        {
            var leaveBalance = new LeaveBalance
            {
                EmployeeId = employee.Id,
                LeaveTypeId = leaveType.Id,
                Year = currentYear,
                TotalDays = leaveType.DefaultDaysPerYear,
                UsedDays = 0,
                CarriedOverDays = 0
            };
            _context.LeaveBalances.Add(leaveBalance);
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Created employee {EmployeeName} ({EmployeeNumber}) with ID {EmployeeId}",
            employee.FullName, employee.EmployeeNumber, employee.Id);

        return employee;
    }

    public async Task<Employee> UpdateEmployeeAsync(Employee employee)
    {
        var existingEmployee = await _context.Employees.FindAsync(employee.Id);
        if (existingEmployee == null)
        {
            throw new InvalidOperationException($"Employee with ID {employee.Id} not found.");
        }

        // If department changed, check if manager needs clearing
        if (existingEmployee.DepartmentId != employee.DepartmentId && employee.ManagerId.HasValue)
        {
            var manager = await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == employee.ManagerId.Value);

            if (manager != null && manager.DepartmentId != employee.DepartmentId)
            {
                employee.ManagerId = null;
                _logger.LogInformation("Cleared manager for employee {EmployeeId} due to department change", employee.Id);
            }
        }

        employee.UpdatedAt = DateTime.UtcNow;

        _context.Entry(existingEmployee).CurrentValues.SetValues(employee);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated employee {EmployeeName} with ID {EmployeeId}",
            employee.FullName, employee.Id);

        return employee;
    }

    public async Task TerminateEmployeeAsync(int employeeId, DateOnly terminationDate)
    {
        var employee = await _context.Employees.FindAsync(employeeId);
        if (employee == null)
        {
            throw new InvalidOperationException($"Employee with ID {employeeId} not found.");
        }

        employee.Status = EmployeeStatus.Terminated;
        employee.TerminationDate = terminationDate;
        employee.UpdatedAt = DateTime.UtcNow;

        // Cancel all submitted leave requests
        var submittedLeaveRequests = await _context.LeaveRequests
            .Where(lr => lr.EmployeeId == employeeId && lr.Status == LeaveRequestStatus.Submitted)
            .ToListAsync();

        foreach (var request in submittedLeaveRequests)
        {
            request.Status = LeaveRequestStatus.Cancelled;
            request.UpdatedAt = DateTime.UtcNow;
        }

        // Remove as manager from departments
        var managedDepartments = await _context.Departments
            .Where(d => d.ManagerId == employeeId)
            .ToListAsync();

        foreach (var department in managedDepartments)
        {
            department.ManagerId = null;
            department.UpdatedAt = DateTime.UtcNow;
        }

        // Remove as manager from direct reports
        var directReports = await _context.Employees
            .Where(e => e.ManagerId == employeeId)
            .ToListAsync();

        foreach (var report in directReports)
        {
            report.ManagerId = null;
            report.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Terminated employee {EmployeeId} effective {TerminationDate}. " +
            "Cancelled {LeaveCount} leave requests, cleared {DeptCount} department manager assignments, " +
            "cleared {ReportCount} direct report manager assignments",
            employeeId, terminationDate, submittedLeaveRequests.Count,
            managedDepartments.Count, directReports.Count);
    }

    public async Task<List<Employee>> GetDirectReportsAsync(int employeeId)
    {
        return await _context.Employees
            .AsNoTracking()
            .Include(e => e.Department)
            .Where(e => e.ManagerId == employeeId)
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync();
    }

    public async Task<List<Employee>> GetEmployeesByDepartmentAsync(int departmentId)
    {
        return await _context.Employees
            .AsNoTracking()
            .Where(e => e.DepartmentId == departmentId)
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ToListAsync();
    }

    public async Task<string> GenerateEmployeeNumberAsync()
    {
        var maxNumber = await _context.Employees
            .AsNoTracking()
            .Where(e => e.EmployeeNumber.StartsWith("EMP-"))
            .Select(e => e.EmployeeNumber)
            .ToListAsync();

        var nextNumber = 1;

        if (maxNumber.Count > 0)
        {
            nextNumber = maxNumber
                .Select(n =>
                {
                    var parts = n.Split('-');
                    return parts.Length == 2 && int.TryParse(parts[1], out var num) ? num : 0;
                })
                .Max() + 1;
        }

        return $"EMP-{nextNumber:D4}";
    }
}
