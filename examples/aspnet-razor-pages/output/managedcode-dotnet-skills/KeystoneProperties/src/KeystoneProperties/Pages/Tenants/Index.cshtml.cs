using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Tenants;

public class IndexModel(ITenantService tenantService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool? IsActive { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public PaginatedList<Tenant> Tenants { get; set; } = default!;

    public async Task OnGetAsync()
    {
        Tenants = await tenantService.GetTenantsAsync(SearchTerm, IsActive, PageNumber, 10);

        ViewData["PageIndex"] = Tenants.PageIndex;
        ViewData["TotalPages"] = Tenants.TotalPages;

        var queryParams = new List<string>();
        if (!string.IsNullOrEmpty(SearchTerm)) queryParams.Add($"searchTerm={Uri.EscapeDataString(SearchTerm)}");
        if (IsActive.HasValue) queryParams.Add($"isActive={IsActive}");
        ViewData["PageUrl"] = "/Tenants?" + string.Join("&", queryParams);
    }
}
