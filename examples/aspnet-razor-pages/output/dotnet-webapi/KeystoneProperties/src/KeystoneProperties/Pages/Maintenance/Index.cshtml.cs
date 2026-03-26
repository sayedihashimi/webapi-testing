using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Maintenance;

public sealed class IndexModel(IMaintenanceService maintenanceService, IUnitService unitService) : PageModel
{
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

    public PaginatedList<MaintenanceRequest> Requests { get; set; } = default!;

    public List<Property> Properties { get; set; } = [];

    public async Task OnGetAsync(CancellationToken ct)
    {
        if (PageNumber < 1) PageNumber = 1;

        Requests = await maintenanceService.GetAllAsync(Status, Priority, PropertyId, Category, PageNumber, 10, ct);
        Properties = await unitService.GetAllPropertiesAsync(ct);
    }
}
