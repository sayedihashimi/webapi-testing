using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Inspections;

public sealed class IndexModel(IInspectionService inspectionService) : PageModel
{
    public PaginatedList<Inspection> Inspections { get; set; } = default!;

    [BindProperty(SupportsGet = true)]
    public InspectionType? InspectionType { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? UnitId { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? FromDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? ToDate { get; set; }

    public async Task OnGetAsync(int pageNumber = 1)
    {
        Inspections = await inspectionService.GetInspectionsAsync(
            InspectionType, UnitId, FromDate, ToDate, pageNumber, 10);
    }
}
