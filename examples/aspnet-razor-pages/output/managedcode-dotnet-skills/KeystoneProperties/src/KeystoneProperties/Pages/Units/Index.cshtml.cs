using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Units;

public class IndexModel(IUnitService unitService) : PageModel
{
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
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public PaginatedList<Unit> Units { get; set; } = default!;
    public List<Property> AllProperties { get; set; } = [];

    public async Task OnGetAsync()
    {
        AllProperties = await unitService.GetAllPropertiesAsync();
        Units = await unitService.GetUnitsAsync(PropertyId, Status, Bedrooms, MinRent, MaxRent, SearchTerm, PageNumber, 10);

        ViewData["PageIndex"] = Units.PageIndex;
        ViewData["TotalPages"] = Units.TotalPages;

        var queryParams = new List<string>();
        if (PropertyId.HasValue) queryParams.Add($"propertyId={PropertyId}");
        if (Status.HasValue) queryParams.Add($"status={Status}");
        if (Bedrooms.HasValue) queryParams.Add($"bedrooms={Bedrooms}");
        if (MinRent.HasValue) queryParams.Add($"minRent={MinRent}");
        if (MaxRent.HasValue) queryParams.Add($"maxRent={MaxRent}");
        if (!string.IsNullOrEmpty(SearchTerm)) queryParams.Add($"searchTerm={Uri.EscapeDataString(SearchTerm)}");
        ViewData["PageUrl"] = "/Units?" + string.Join("&", queryParams);
    }
}
