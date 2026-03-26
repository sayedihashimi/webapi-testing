using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Tenants;

public class IndexModel : PageModel
{
    private readonly ITenantService _tenantService;
    public IndexModel(ITenantService tenantService) { _tenantService = tenantService; }

    public List<Tenant> Tenants { get; set; } = new();
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public bool? IsActiveFilter { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }

    public async Task OnGetAsync()
    {
        const int pageSize = 10;
        var (items, totalCount) = await _tenantService.GetTenantsAsync(Search, IsActiveFilter, PageNumber, pageSize);
        Tenants = items;
        CurrentPage = PageNumber;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }
}
