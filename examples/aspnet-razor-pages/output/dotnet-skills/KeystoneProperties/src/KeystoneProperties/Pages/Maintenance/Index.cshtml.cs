using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KeystoneProperties.Pages.Maintenance;

public class IndexModel : PageModel
{
    private readonly IMaintenanceService _maintenanceService;
    private readonly IPropertyService _propertyService;

    public IndexModel(IMaintenanceService maintenanceService, IPropertyService propertyService)
    {
        _maintenanceService = maintenanceService;
        _propertyService = propertyService;
    }

    public PaginatedList<MaintenanceRequest> Requests { get; set; } = default!;
    public SelectList PropertyList { get; set; } = default!;

    [BindProperty(SupportsGet = true)]
    public MaintenanceStatus? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public MaintenancePriority? Priority { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? PropertyId { get; set; }

    [BindProperty(SupportsGet = true)]
    public MaintenanceCategory? Category { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public async Task OnGetAsync()
    {
        var properties = await _propertyService.GetAllActiveAsync();
        PropertyList = new SelectList(properties, "Id", "Name");

        Requests = await _maintenanceService.GetRequestsAsync(
            Status, Priority, PropertyId, Category, PageNumber, 10);
    }
}
