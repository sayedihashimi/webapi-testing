using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Leases;

public class IndexModel(ILeaseService leaseService, IPropertyService propertyService) : PageModel
{
    public PaginatedList<Lease> Leases { get; set; } = default!;
    public List<Property> AllProperties { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public LeaseStatus? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? PropertyId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public async Task OnGetAsync()
    {
        var propertiesResult = await propertyService.GetPropertiesAsync(null, null, null, 1, 1000);
        AllProperties = propertiesResult.ToList();

        Leases = await leaseService.GetLeasesAsync(Status, PropertyId, PageNumber, 10);

        ViewData["PageIndex"] = Leases.PageIndex;
        ViewData["TotalPages"] = Leases.TotalPages;
        ViewData["PageUrl"] = $"/Leases?status={Status}&propertyId={PropertyId}";
    }
}
