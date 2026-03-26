using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Tenants;

public class DetailsModel(ITenantService tenantService) : PageModel
{
    public Tenant Tenant { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var tenant = await tenantService.GetTenantWithDetailsAsync(id);
        if (tenant is null)
        {
            return NotFound();
        }

        Tenant = tenant;
        return Page();
    }
}
