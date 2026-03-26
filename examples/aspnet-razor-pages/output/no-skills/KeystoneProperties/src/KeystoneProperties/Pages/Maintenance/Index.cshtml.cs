using KeystoneProperties.Data;
using KeystoneProperties.Models;
using KeystoneProperties.Models.Enums;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Pages.Maintenance;

public class IndexModel : PageModel
{
    private readonly IMaintenanceService _maintenanceService;
    private readonly ApplicationDbContext _context;
    public IndexModel(IMaintenanceService maintenanceService, ApplicationDbContext context) { _maintenanceService = maintenanceService; _context = context; }

    public List<MaintenanceRequest> Requests { get; set; } = new();
    public List<Property> PropertyList { get; set; } = new();
    [BindProperty(SupportsGet = true)] public MaintenanceStatus? StatusFilter { get; set; }
    [BindProperty(SupportsGet = true)] public MaintenancePriority? PriorityFilter { get; set; }
    [BindProperty(SupportsGet = true)] public int? PropertyId { get; set; }
    [BindProperty(SupportsGet = true)] public MaintenanceCategory? CategoryFilter { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }

    public async Task OnGetAsync()
    {
        PropertyList = await _context.Properties.OrderBy(p => p.Name).ToListAsync();
        const int pageSize = 10;
        var (items, totalCount) = await _maintenanceService.GetRequestsAsync(StatusFilter, PriorityFilter, PropertyId, CategoryFilter, PageNumber, pageSize);
        Requests = items;
        CurrentPage = PageNumber;
        TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
    }
}
