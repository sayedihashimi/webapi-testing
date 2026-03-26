using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KeystoneProperties.Pages.Units;

public sealed class IndexModel(IUnitService unitService, IPropertyService propertyService) : PageModel
{
    public PaginatedList<Unit> Units { get; set; } = null!;
    public SelectList Properties { get; set; } = null!;

    [BindProperty(SupportsGet = true)]
    public int? PropertyId { get; set; }

    [BindProperty(SupportsGet = true)]
    public UnitStatus? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? Bedrooms { get; set; }

    [BindProperty(SupportsGet = true)]
    public decimal? MinRent { get; set; }

    [BindProperty(SupportsGet = true)]
    public decimal? MaxRent { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 10;

    public async Task OnGetAsync()
    {
        var properties = await propertyService.GetPropertiesAsync(null, null, true, 1, int.MaxValue);
        Properties = new SelectList(properties.Items, "Id", "Name");

        Units = await unitService.GetUnitsAsync(PropertyId, Status, Bedrooms, MinRent, MaxRent, Search, PageNumber, PageSize);
    }
}
