using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Tenants;

public sealed class DeactivateModel(ITenantService tenantService) : PageModel
{
    public Tenant Tenant { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var tenant = await tenantService.GetByIdAsync(id);
        if (tenant is null)
        {
            return NotFound();
        }

        Tenant = tenant;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var (success, error) = await tenantService.DeactivateAsync(id);

        if (!success)
        {
            TempData["ErrorMessage"] = error ?? "Failed to deactivate tenant.";
            return RedirectToPage("Details", new { id });
        }

        TempData["SuccessMessage"] = "Tenant deactivated successfully.";
        return RedirectToPage("Details", new { id });
    }
}
