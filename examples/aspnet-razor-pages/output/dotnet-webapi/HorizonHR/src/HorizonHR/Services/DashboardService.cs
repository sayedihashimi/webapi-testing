using HorizonHR.Data;
using HorizonHR.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HorizonHR.Services;

public sealed class DashboardService(HorizonDbContext db) : IDashboardService
{
    public async Task<DashboardStats> GetStatsAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var thirtyDaysAgo = today.AddDays(-30);

        var totalEmployees = await db.Employees.CountAsync(e => e.Status != EmployeeStatus.Terminated, ct);
        var totalDepartments = await db.Departments.CountAsync(d => d.IsActive, ct);
        var pendingLeave = await db.LeaveRequests.CountAsync(lr => lr.Status == LeaveRequestStatus.Submitted, ct);

        var upcomingReviews = await db.PerformanceReviews.CountAsync(r =>
            r.Status != ReviewStatus.Completed, ct);

        var recentHires = await db.Employees.CountAsync(e =>
            e.HireDate >= thirtyDaysAgo && e.Status != EmployeeStatus.Terminated, ct);

        var onLeaveToday = await db.LeaveRequests.CountAsync(lr =>
            lr.Status == LeaveRequestStatus.Approved &&
            lr.StartDate <= today && lr.EndDate >= today, ct);

        var headcount = await db.Departments
            .AsNoTracking()
            .Where(d => d.IsActive)
            .Select(d => new DepartmentHeadcount(
                d.Name,
                d.Employees.Count(e => e.Status != EmployeeStatus.Terminated)))
            .OrderByDescending(h => h.Count)
            .ToListAsync(ct);

        return new DashboardStats
        {
            TotalEmployees = totalEmployees,
            TotalDepartments = totalDepartments,
            PendingLeaveRequests = pendingLeave,
            UpcomingReviews = upcomingReviews,
            RecentHires = recentHires,
            EmployeesOnLeaveToday = onLeaveToday,
            HeadcountByDepartment = headcount
        };
    }
}
