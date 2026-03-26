using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using KeystoneProperties.Models;
using KeystoneProperties.Services;

namespace KeystoneProperties.Pages.Units;

public class IndexModel : PageModel
{
    private readonly IUnitService _unitService;
    private readonly IPropertyService _propertyService;

    public IndexModel(IUnitService unitService, IPropertyService propertyService)
    {
        _unitService = unitService;
        _propertyService = propertyService;
    }

    public PaginatedList<Unit> Units { get; set; } = default!;
    public SelectList PropertyList { get; set; } = default!;

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

    public async Task OnGetAsync()
    {
        var properties = await _propertyService.GetAllActiveAsync();
        PropertyList = new SelectList(properties, "Id", "Name");

        Units = await _unitService.GetUnitsAsync(
            PropertyId, Status, Bedrooms, MinRent, MaxRent, Search, PageNumber, 10);
    }
}
