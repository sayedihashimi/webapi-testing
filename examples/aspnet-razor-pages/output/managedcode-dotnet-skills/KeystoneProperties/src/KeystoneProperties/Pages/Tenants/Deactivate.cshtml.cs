using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Tenants;

public class DeactivateModel(ITenantService tenantService) : PageModel
{
    public Tenant Tenant { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var tenant = await tenantService.GetTenantByIdAsync(id);
        if (tenant is null)
        {
            return NotFound();
        }

        Tenant = tenant;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var (success, errorMessage) = await tenantService.DeactivateTenantAsync(id);
        if (success)
        {
            TempData["SuccessMessage"] = "Tenant deactivated successfully.";
            return RedirectToPage("/Tenants/Index");
        }

        TempData["ErrorMessage"] = errorMessage ?? "Failed to deactivate tenant.";

        var tenant = await tenantService.GetTenantByIdAsync(id);
        if (tenant is null)
        {
            return NotFound();
        }

        Tenant = tenant;
        return Page();
    }
}
