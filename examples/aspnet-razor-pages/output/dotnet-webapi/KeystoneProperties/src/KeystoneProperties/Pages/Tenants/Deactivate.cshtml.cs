using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Tenants;

public sealed class DeactivateModel(ITenantService tenantService) : PageModel
{
    public Tenant Tenant { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var tenant = await tenantService.GetByIdAsync(id, ct);
        if (tenant is null)
        {
            return NotFound();
        }

        Tenant = tenant;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken ct)
    {
        var tenant = await tenantService.GetByIdAsync(id, ct);
        if (tenant is null)
        {
            return NotFound();
        }

        var (success, error) = await tenantService.DeactivateAsync(id, ct);
        if (!success)
        {
            Tenant = tenant;
            TempData["ErrorMessage"] = error ?? "Unable to deactivate tenant.";
            return Page();
        }

        TempData["SuccessMessage"] = "Tenant deactivated successfully.";
        return RedirectToPage("Index");
    }
}
