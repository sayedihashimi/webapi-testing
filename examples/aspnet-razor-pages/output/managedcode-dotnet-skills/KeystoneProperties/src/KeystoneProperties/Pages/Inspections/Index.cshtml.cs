using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Inspections;

public class IndexModel(IInspectionService inspectionService) : PageModel
{
    public PaginatedList<Inspection> Inspections { get; set; } = default!;

    [BindProperty(SupportsGet = true)]
    public InspectionType? InspectionType { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? UnitId { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? StartDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? EndDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public async Task OnGetAsync()
    {
        Inspections = await inspectionService.GetInspectionsAsync(InspectionType, UnitId, StartDate, EndDate, PageNumber, 10);
    }
}
