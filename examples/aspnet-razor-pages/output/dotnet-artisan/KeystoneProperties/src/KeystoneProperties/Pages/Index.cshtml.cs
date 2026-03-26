using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages;

public sealed class IndexModel(
    IPropertyService propertyService,
    IUnitService unitService,
    IPaymentService paymentService,
    IMaintenanceService maintenanceService,
    ILeaseService leaseService) : PageModel
{
    public int TotalProperties { get; set; }
    public int TotalUnits { get; set; }
    public double OccupancyRate { get; set; }
    public decimal RentCollected { get; set; }
    public int OverdueCount { get; set; }
    public int OpenMaintenanceCount { get; set; }
    public List<Lease> ExpiringLeases { get; set; } = [];

    public async Task OnGetAsync()
    {
        TotalProperties = await propertyService.GetTotalCountAsync();
        TotalUnits = await unitService.GetTotalCountAsync();
        var occupied = await unitService.GetOccupiedCountAsync();
        OccupancyRate = TotalUnits > 0 ? (double)occupied / TotalUnits : 0;
        RentCollected = await paymentService.GetRentCollectedThisMonthAsync();
        OverdueCount = await paymentService.GetOverdueCountAsync();
        OpenMaintenanceCount = await maintenanceService.GetOpenCountAsync();
        ExpiringLeases = await leaseService.GetExpiringLeasesAsync(30);
    }
}
