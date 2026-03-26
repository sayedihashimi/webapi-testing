using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using KeystoneProperties.Models;
using KeystoneProperties.Services;

namespace KeystoneProperties.Pages.Properties;

public class IndexModel : PageModel
{
    private readonly IPropertyService _propertyService;

    public IndexModel(IPropertyService propertyService)
    {
        _propertyService = propertyService;
    }

    public PaginatedList<Property> Properties { get; set; } = default!;

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public PropertyType? Type { get; set; }

    [BindProperty(SupportsGet = true)]
    public bool? IsActive { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 10;

    public async Task OnGetAsync()
    {
        Properties = await _propertyService.GetPropertiesAsync(Search, Type, IsActive, PageNumber, PageSize);
    }
}
