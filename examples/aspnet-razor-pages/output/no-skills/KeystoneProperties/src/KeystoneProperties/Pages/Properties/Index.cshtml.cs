using KeystoneProperties.Models;
using KeystoneProperties.Models.Enums;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Properties;

public class IndexModel : PageModel
{
    private readonly IPropertyService _propertyService;

    public IndexModel(IPropertyService propertyService)
    {
        _propertyService = propertyService;
    }

    public List<Property> Properties { get; set; } = new();
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public PropertyType? TypeFilter { get; set; }
    [BindProperty(SupportsGet = true)] public bool? IsActiveFilter { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }

    public async Task OnGetAsync()
    {
        const int pageSize = 10;
        var (items, totalCount) = await _propertyService.GetPropertiesAsync(Search, TypeFilter, IsActiveFilter, PageNumber, pageSize);
        Properties = items;
        CurrentPage = PageNumber;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }
}
