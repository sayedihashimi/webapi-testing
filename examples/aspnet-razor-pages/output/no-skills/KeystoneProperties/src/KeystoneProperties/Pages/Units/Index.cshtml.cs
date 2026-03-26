using KeystoneProperties.Data;
using KeystoneProperties.Models;
using KeystoneProperties.Models.Enums;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Pages.Units;

public class IndexModel : PageModel
{
    private readonly IUnitService _unitService;
    private readonly ApplicationDbContext _context;

    public IndexModel(IUnitService unitService, ApplicationDbContext context)
    {
        _unitService = unitService;
        _context = context;
    }

    public List<Unit> Units { get; set; } = new();
    public List<Property> PropertyList { get; set; } = new();
    [BindProperty(SupportsGet = true)] public string? Search { get; set; }
    [BindProperty(SupportsGet = true)] public int? PropertyId { get; set; }
    [BindProperty(SupportsGet = true)] public UnitStatus? StatusFilter { get; set; }
    [BindProperty(SupportsGet = true)] public int? Bedrooms { get; set; }
    [BindProperty(SupportsGet = true)] public decimal? MinRent { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }

    public async Task OnGetAsync()
    {
        PropertyList = await _context.Properties.OrderBy(p => p.Name).ToListAsync();
        const int pageSize = 10;
        var (items, totalCount) = await _unitService.GetUnitsAsync(PropertyId, StatusFilter, Bedrooms, MinRent, null, Search, PageNumber, pageSize);
        Units = items;
        CurrentPage = PageNumber;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }
}
