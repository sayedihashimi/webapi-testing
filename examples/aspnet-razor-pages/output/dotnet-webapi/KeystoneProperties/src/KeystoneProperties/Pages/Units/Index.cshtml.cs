using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KeystoneProperties.Pages.Units;

public sealed class IndexModel(IUnitService unitService) : PageModel
{
    public PaginatedList<Unit> Units { get; set; } = null!;

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

    public List<SelectListItem> PropertyList { get; set; } = [];
    public List<SelectListItem> StatusList { get; set; } = [];
    public List<SelectListItem> BedroomList { get; set; } = [];

    public async Task OnGetAsync(CancellationToken ct)
    {
        await PopulateFiltersAsync(ct);

        Units = await unitService.GetAllAsync(
            PropertyId, Status, Bedrooms, MinRent, MaxRent, Search,
            PageNumber, pageSize: 10, ct);
    }

    private async Task PopulateFiltersAsync(CancellationToken ct)
    {
        var properties = await unitService.GetAllPropertiesAsync(ct);
        PropertyList = properties
            .Select(p => new SelectListItem(p.Name, p.Id.ToString()))
            .ToList();

        StatusList = Enum.GetValues<UnitStatus>()
            .Select(s => new SelectListItem(s.ToString(), s.ToString()))
            .ToList();

        BedroomList = Enumerable.Range(0, 6)
            .Select(b => new SelectListItem(b == 0 ? "Studio" : $"{b}", b.ToString()))
            .ToList();
    }
}
