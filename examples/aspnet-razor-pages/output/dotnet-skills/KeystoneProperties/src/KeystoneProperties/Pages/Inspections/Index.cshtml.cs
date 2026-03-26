using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Inspections;

public class IndexModel : PageModel
{
    private readonly IInspectionService _inspectionService;

    public IndexModel(IInspectionService inspectionService)
    {
        _inspectionService = inspectionService;
    }

    public PaginatedList<Inspection> Inspections { get; set; } = null!;

    [BindProperty(SupportsGet = true)]
    public InspectionType? TypeFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? UnitId { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? FromDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? ToDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public async Task OnGetAsync()
    {
        Inspections = await _inspectionService.GetInspectionsAsync(
            TypeFilter, UnitId, FromDate, ToDate, PageNumber, 10);
    }
}
