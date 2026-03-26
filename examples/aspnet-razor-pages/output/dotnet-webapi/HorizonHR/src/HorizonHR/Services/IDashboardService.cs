namespace HorizonHR.Services;

public sealed record DashboardStats
{
    public int TotalEmployees { get; init; }
    public int TotalDepartments { get; init; }
    public int PendingLeaveRequests { get; init; }
    public int UpcomingReviews { get; init; }
    public int RecentHires { get; init; }
    public int EmployeesOnLeaveToday { get; init; }
    public IReadOnlyList<DepartmentHeadcount> HeadcountByDepartment { get; init; } = [];
}

public sealed record DepartmentHeadcount(string DepartmentName, int Count);

public interface IDashboardService
{
    Task<DashboardStats> GetStatsAsync(CancellationToken ct = default);
}
