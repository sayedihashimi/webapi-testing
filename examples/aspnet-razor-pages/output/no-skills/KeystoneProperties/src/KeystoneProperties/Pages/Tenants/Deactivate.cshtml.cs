using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Tenants;

public class DeactivateModel : PageModel
{
    private readonly ITenantService _tenantService;
    public DeactivateModel(ITenantService tenantService) { _tenantService = tenantService; }

    public Tenant Tenant { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var tenant = await _tenantService.GetByIdAsync(id);
        if (tenant == null) return NotFound();
        Tenant = tenant;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var (success, error) = await _tenantService.DeactivateAsync(id);
        if (!success)
        {
            TempData["ErrorMessage"] = error;
            return RedirectToPage("Details", new { id });
        }
        TempData["SuccessMessage"] = "Tenant deactivated successfully.";
        return RedirectToPage("Index");
    }
}
