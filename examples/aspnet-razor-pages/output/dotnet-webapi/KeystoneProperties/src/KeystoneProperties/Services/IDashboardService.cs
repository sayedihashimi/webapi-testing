namespace KeystoneProperties.Services;

public sealed record DashboardViewModel
{
    public int TotalProperties { get; init; }
    public int TotalUnits { get; init; }
    public decimal OccupancyRate { get; init; }
    public decimal RentCollectedThisMonth { get; init; }
    public int OverduePaymentsCount { get; init; }
    public int OpenMaintenanceRequests { get; init; }
    public IReadOnlyList<LeaseExpirationItem> UpcomingLeaseExpirations { get; init; } = [];
}

public sealed record LeaseExpirationItem
{
    public int LeaseId { get; init; }
    public string TenantName { get; init; } = string.Empty;
    public string UnitInfo { get; init; } = string.Empty;
    public DateOnly EndDate { get; init; }
    public int DaysUntilExpiration { get; init; }
}

public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardAsync(CancellationToken ct = default);
}
