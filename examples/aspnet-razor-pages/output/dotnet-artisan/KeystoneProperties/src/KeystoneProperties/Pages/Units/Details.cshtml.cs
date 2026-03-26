using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Units;

public sealed class DetailsModel(IUnitService unitService) : PageModel
{
    public Unit Unit { get; set; } = null!;
    public Lease? ActiveLease { get; set; }
    public List<Lease> LeaseHistory { get; set; } = [];
    public List<MaintenanceRequest> MaintenanceHistory { get; set; } = [];
    public List<Inspection> Inspections { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var unit = await unitService.GetWithDetailsAsync(id);
        if (unit is null)
        {
            return NotFound();
        }

        Unit = unit;
        ActiveLease = unit.Leases.FirstOrDefault(l => l.Status == LeaseStatus.Active);
        LeaseHistory = [.. unit.Leases.OrderByDescending(l => l.StartDate)];
        MaintenanceHistory = [.. unit.MaintenanceRequests.OrderByDescending(m => m.SubmittedDate)];
        Inspections = [.. unit.Inspections.OrderByDescending(i => i.ScheduledDate)];

        return Page();
    }
}
