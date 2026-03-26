using KeystoneProperties.Models;

namespace KeystoneProperties.Services;

public class DashboardViewModel
{
    public int TotalProperties { get; set; }
    public int TotalUnits { get; set; }
    public decimal OccupancyRate { get; set; }
    public decimal RentCollectedThisMonth { get; set; }
    public int OverduePaymentsCount { get; set; }
    public int OpenMaintenanceCount { get; set; }
    public List<Lease> ExpiringLeases { get; set; } = new();
}

public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardAsync();
}
