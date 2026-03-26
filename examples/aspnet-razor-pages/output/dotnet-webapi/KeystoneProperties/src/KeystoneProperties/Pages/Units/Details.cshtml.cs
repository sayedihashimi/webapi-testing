using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Units;

public sealed class DetailsModel(IUnitService unitService) : PageModel
{
    public Unit Unit { get; set; } = null!;
    public Lease? CurrentLease { get; set; }
    public List<Lease> LeaseHistory { get; set; } = [];
    public List<MaintenanceRequest> MaintenanceHistory { get; set; } = [];
    public List<Inspection> Inspections { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var unit = await unitService.GetByIdAsync(id, ct);

        if (unit is null)
        {
            return NotFound();
        }

        Unit = unit;

        CurrentLease = unit.Leases
            .Where(l => l.Status == LeaseStatus.Active)
            .OrderByDescending(l => l.StartDate)
            .FirstOrDefault();

        LeaseHistory = unit.Leases
            .OrderByDescending(l => l.StartDate)
            .ToList();

        MaintenanceHistory = unit.MaintenanceRequests
            .OrderByDescending(m => m.SubmittedDate)
            .ToList();

        Inspections = unit.Inspections
            .OrderByDescending(i => i.ScheduledDate)
            .ToList();

        return Page();
    }
}
