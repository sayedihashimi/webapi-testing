using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Maintenance;

public sealed class DetailsModel(IMaintenanceService maintenanceService) : PageModel
{
    public new MaintenanceRequest Request { get; set; } = default!;

    public bool IsTerminal => Request.Status is MaintenanceStatus.Completed or MaintenanceStatus.Cancelled;

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var request = await maintenanceService.GetByIdAsync(id, ct);
        if (request is null)
        {
            return NotFound();
        }

        Request = request;
        return Page();
    }
}
