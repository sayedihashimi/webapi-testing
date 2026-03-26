using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Maintenance;

public class DetailsModel : PageModel
{
    private readonly IMaintenanceService _maintenanceService;
    public DetailsModel(IMaintenanceService maintenanceService) { _maintenanceService = maintenanceService; }

    public new MaintenanceRequest Request { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var req = await _maintenanceService.GetWithDetailsAsync(id);
        if (req == null) return NotFound();
        Request = req;
        return Page();
    }
}
