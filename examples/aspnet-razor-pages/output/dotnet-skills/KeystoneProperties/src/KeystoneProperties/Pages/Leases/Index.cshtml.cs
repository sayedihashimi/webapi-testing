using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KeystoneProperties.Pages.Leases;

public class IndexModel : PageModel
{
    private readonly ILeaseService _leaseService;
    private readonly IPropertyService _propertyService;

    public IndexModel(ILeaseService leaseService, IPropertyService propertyService)
    {
        _leaseService = leaseService;
        _propertyService = propertyService;
    }

    public PaginatedList<Lease> Leases { get; set; } = null!;
    public SelectList PropertyList { get; set; } = null!;

    [BindProperty(SupportsGet = true)]
    public LeaseStatus? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? PropertyId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public async Task OnGetAsync()
    {
        var properties = await _propertyService.GetAllActiveAsync();
        PropertyList = new SelectList(properties, "Id", "Name");

        Leases = await _leaseService.GetLeasesAsync(Status, PropertyId, Search, PageNumber, 10);
    }
}
