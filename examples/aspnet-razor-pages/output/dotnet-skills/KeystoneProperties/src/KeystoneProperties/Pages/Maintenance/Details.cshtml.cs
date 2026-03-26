using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Maintenance;

public class DetailsModel : PageModel
{
    private readonly IMaintenanceService _maintenanceService;

    public DetailsModel(IMaintenanceService maintenanceService)
    {
        _maintenanceService = maintenanceService;
    }

    public new MaintenanceRequest Request { get; set; } = default!;
    public bool IsTerminalState { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var request = await _maintenanceService.GetWithDetailsAsync(id);
        if (request == null)
        {
            return NotFound();
        }

        Request = request;
        var validNext = await _maintenanceService.GetValidNextStatuses(request.Status);
        IsTerminalState = validNext.Count == 0;

        return Page();
    }
}
