using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Maintenance;

public class IndexModel(IMaintenanceService maintenanceService, IUnitService unitService) : PageModel
{
    public PaginatedList<MaintenanceRequest> Requests { get; set; } = default!;

    public List<Property> AllProperties { get; set; } = [];

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
        AllProperties = await unitService.GetAllPropertiesAsync();
        Requests = await maintenanceService.GetRequestsAsync(Status, Priority, PropertyId, Category, PageNumber, 10);
    }
}
