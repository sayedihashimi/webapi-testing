using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Tenants;

public sealed class IndexModel(ITenantService tenantService) : PageModel
{
    public PaginatedList<Tenant> Tenants { get; set; } = null!;

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool? IsActive { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public async Task OnGetAsync(CancellationToken ct)
    {
        Tenants = await tenantService.GetAllAsync(Search, IsActive, PageNumber, 10, ct);
    }
}
