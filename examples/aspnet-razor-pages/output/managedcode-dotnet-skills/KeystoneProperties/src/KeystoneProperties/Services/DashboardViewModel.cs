using KeystoneProperties.Models;

namespace KeystoneProperties.Services;

public class DashboardViewModel
{
    public int TotalProperties { get; set; }
    public int TotalUnits { get; set; }
    public int OccupiedUnits { get; set; }
    public decimal OccupancyRate { get; set; }
    public decimal RentCollectedThisMonth { get; set; }
    public int OverduePaymentsCount { get; set; }
    public int OpenMaintenanceRequests { get; set; }
    public List<Lease> ExpiringLeases { get; set; } = [];
}
