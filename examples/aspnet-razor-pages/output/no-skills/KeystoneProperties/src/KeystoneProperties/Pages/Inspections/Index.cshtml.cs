using KeystoneProperties.Models;
using KeystoneProperties.Models.Enums;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Inspections;

public class IndexModel : PageModel
{
    private readonly IInspectionService _inspectionService;
    public IndexModel(IInspectionService inspectionService) { _inspectionService = inspectionService; }

    public List<Inspection> Inspections { get; set; } = new();
    [BindProperty(SupportsGet = true)] public InspectionType? TypeFilter { get; set; }
    [BindProperty(SupportsGet = true)] public DateOnly? FromDate { get; set; }
    [BindProperty(SupportsGet = true)] public DateOnly? ToDate { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }

    public async Task OnGetAsync()
    {
        const int pageSize = 10;
        var (items, totalCount) = await _inspectionService.GetInspectionsAsync(TypeFilter, null, FromDate, ToDate, PageNumber, pageSize);
        Inspections = items;
        CurrentPage = PageNumber;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }
}
