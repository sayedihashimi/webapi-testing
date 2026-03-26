using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Tenants;

public class IndexModel : PageModel
{
    private readonly ITenantService _tenantService;

    public IndexModel(ITenantService tenantService)
    {
        _tenantService = tenantService;
    }

    public PaginatedList<Tenant> Tenants { get; set; } = null!;

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool? IsActive { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public async Task OnGetAsync()
    {
        Tenants = await _tenantService.GetTenantsAsync(Search, IsActive, PageNumber, 10);
    }
}
