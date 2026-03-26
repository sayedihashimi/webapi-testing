using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Maintenance;

public class DetailsModel(IMaintenanceService maintenanceService) : PageModel
{
    public new MaintenanceRequest Request { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var request = await maintenanceService.GetRequestWithDetailsAsync(id);
        if (request is null)
        {
            return NotFound();
        }

        Request = request;
        return Page();
    }
}
