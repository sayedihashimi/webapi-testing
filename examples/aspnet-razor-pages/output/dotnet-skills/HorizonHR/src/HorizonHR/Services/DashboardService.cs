using HorizonHR.Data;
using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HorizonHR.Services;

public sealed class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(ApplicationDbContext context, ILogger<DashboardService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DashboardViewModel> GetDashboardDataAsync()
    {
        var thirtyDaysAgo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));

        var dashboard = new DashboardViewModel
        {
            TotalEmployees = await _context.Employees
                .AsNoTracking()
                .CountAsync(e => e.Status != EmployeeStatus.Terminated),

            TotalDepartments = await _context.Departments
                .AsNoTracking()
                .CountAsync(d => d.IsActive),

            PendingLeaveRequests = await _context.LeaveRequests
                .AsNoTracking()
                .CountAsync(lr => lr.Status == LeaveRequestStatus.Submitted),

            UpcomingReviews = await _context.PerformanceReviews
                .AsNoTracking()
                .CountAsync(r => r.Status != ReviewStatus.Completed),

            RecentHires = await _context.Employees
                .AsNoTracking()
                .Include(e => e.Department)
                .Where(e => e.HireDate >= thirtyDaysAgo)
                .OrderByDescending(e => e.HireDate)
                .ToListAsync(),

            EmployeesOnLeave = await _context.Employees
                .AsNoTracking()
                .Include(e => e.Department)
                .Where(e => e.Status == EmployeeStatus.OnLeave)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync(),

            HeadcountByDepartment = await _context.Departments
                .AsNoTracking()
                .Where(d => d.IsActive)
                .Select(d => new DepartmentHeadcount
                {
                    DepartmentName = d.Name,
                    Count = d.Employees.Count(e => e.Status != EmployeeStatus.Terminated)
                })
                .OrderByDescending(h => h.Count)
                .ToListAsync()
        };

        _logger.LogInformation("Dashboard data loaded: {TotalEmployees} employees, {TotalDepartments} departments",
            dashboard.TotalEmployees, dashboard.TotalDepartments);

        return dashboard;
    }
}
