using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KeystoneProperties.Pages.Leases;

public sealed class IndexModel(ILeaseService leaseService, IPropertyService propertyService) : PageModel
{
    public PaginatedList<Lease> Leases { get; set; } = default!;
    public List<SelectListItem> PropertyOptions { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public LeaseStatus? StatusFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? PropertyFilter { get; set; }

    public async Task OnGetAsync(int pageNumber = 1, int pageSize = 10)
    {
        await LoadPropertyOptionsAsync();
        Leases = await leaseService.GetLeasesAsync(StatusFilter, PropertyFilter, pageNumber, pageSize);
    }

    private async Task LoadPropertyOptionsAsync()
    {
        var properties = await propertyService.GetPropertiesAsync(null, null, true, 1, int.MaxValue);
        PropertyOptions = properties.Items
            .Select(p => new SelectListItem(p.Name, p.Id.ToString()))
            .ToList();
    }
}
