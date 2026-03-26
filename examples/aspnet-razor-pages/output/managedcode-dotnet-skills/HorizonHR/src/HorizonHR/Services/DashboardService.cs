using HorizonHR.Data;
using HorizonHR.Models.Enums;
using HorizonHR.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HorizonHR.Services;

public class DashboardService(ApplicationDbContext db) : IDashboardService
{
    public async Task<DashboardViewModel> GetDashboardDataAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var thirtyDaysAgo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));

        var totalEmployees = await db.Employees.CountAsync(e => e.Status != EmployeeStatus.Terminated);
        var departmentCount = await db.Departments.CountAsync(d => d.IsActive);
        var pendingLeaveRequests = await db.LeaveRequests.CountAsync(lr => lr.Status == LeaveRequestStatus.Submitted);

        var upcomingReviews = await db.PerformanceReviews
            .CountAsync(r => r.Status != ReviewStatus.Completed);

        var recentHires = await db.Employees
            .Include(e => e.Department)
            .AsNoTracking()
            .Where(e => e.HireDate >= thirtyDaysAgo && e.Status != EmployeeStatus.Terminated)
            .OrderByDescending(e => e.HireDate)
            .Take(5)
            .ToListAsync();

        var employeesOnLeave = await db.Employees
            .Include(e => e.Department)
            .AsNoTracking()
            .Where(e => e.Status == EmployeeStatus.OnLeave)
            .ToListAsync();

        // Also check for approved leave requests covering today
        var onLeaveFromRequests = await db.LeaveRequests
            .Include(lr => lr.Employee).ThenInclude(e => e.Department)
            .AsNoTracking()
            .Where(lr => lr.Status == LeaveRequestStatus.Approved
                && lr.StartDate <= today
                && lr.EndDate >= today)
            .Select(lr => lr.Employee)
            .ToListAsync();

        var allOnLeave = employeesOnLeave
            .UnionBy(onLeaveFromRequests, e => e.Id)
            .ToList();

        var headcount = await db.Employees
            .Where(e => e.Status != EmployeeStatus.Terminated)
            .GroupBy(e => e.Department.Name)
            .Select(g => new DepartmentHeadcount { DepartmentName = g.Key, Count = g.Count() })
            .OrderByDescending(h => h.Count)
            .ToListAsync();

        return new DashboardViewModel
        {
            TotalEmployees = totalEmployees,
            DepartmentCount = departmentCount,
            PendingLeaveRequests = pendingLeaveRequests,
            UpcomingReviews = upcomingReviews,
            RecentHires = recentHires,
            EmployeesOnLeaveToday = allOnLeave,
            HeadcountByDepartment = headcount
        };
    }
}
