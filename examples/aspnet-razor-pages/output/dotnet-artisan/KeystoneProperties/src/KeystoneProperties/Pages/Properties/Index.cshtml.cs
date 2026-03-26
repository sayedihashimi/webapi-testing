using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Properties;

public sealed class IndexModel(IPropertyService propertyService) : PageModel
{
    public PaginatedList<Property> Properties { get; set; } = default!;

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public PropertyType? PropertyTypeFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool? IsActiveFilter { get; set; }

    public async Task OnGetAsync(int pageNumber = 1, int pageSize = 10)
    {
        Properties = await propertyService.GetPropertiesAsync(
            Search, PropertyTypeFilter, IsActiveFilter, pageNumber, pageSize);
    }
}
