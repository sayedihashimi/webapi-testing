using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Inspections;

public sealed class IndexModel(
    IInspectionService inspectionService,
    IUnitService unitService) : PageModel
{
    public PaginatedList<Inspection> Inspections { get; set; } = null!;
    public IReadOnlyList<Unit> Units { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public InspectionType? CurrentType { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? CurrentUnitId { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? CurrentFromDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? CurrentToDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public async Task OnGetAsync(CancellationToken ct)
    {
        var unitsResult = await unitService.GetAllAsync(null, null, null, null, null, null, 1, 1000, ct);
        Units = unitsResult.Items;

        Inspections = await inspectionService.GetAllAsync(
            CurrentType, CurrentUnitId, CurrentFromDate, CurrentToDate,
            PageNumber, 10, ct);
    }
}
