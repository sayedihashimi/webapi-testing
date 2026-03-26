using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Tenants;

public class DetailsModel : PageModel
{
    private readonly ITenantService _tenantService;

    public DetailsModel(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    public Tenant Tenant { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var tenant = await _tenantService.GetWithDetailsAsync(id);
        if (tenant == null)
        {
            return NotFound();
        }

        Tenant = tenant;
        return Page();
    }
}
