using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Maintenance;

public sealed class DetailsModel(IMaintenanceService maintenanceService) : PageModel
{
    public new MaintenanceRequest Request { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var request = await maintenanceService.GetByIdAsync(id);

        if (request is null)
        {
            return NotFound();
        }

        Request = request;
        return Page();
    }

    public bool IsTimelineStepCompleted(MaintenanceStatus step) =>
        Request.Status >= step && Request.Status != MaintenanceStatus.Cancelled;

    public bool IsTimelineStepActive(MaintenanceStatus step) =>
        Request.Status == step;
}
