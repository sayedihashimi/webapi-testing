using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KeystoneProperties.Pages.Maintenance;

public sealed class IndexModel(
    IMaintenanceService maintenanceService,
    IPropertyService propertyService) : PageModel
{
    private const int PageSize = 10;

    public PaginatedList<MaintenanceRequest> Requests { get; set; } = null!;
    public SelectList PropertyList { get; set; } = null!;

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
        var properties = await propertyService.GetPropertiesAsync(null, null, true, 1, int.MaxValue);
        PropertyList = new SelectList(properties.Items, "Id", "Name");

        Requests = await maintenanceService.GetRequestsAsync(
            Status, Priority, PropertyId, Category, PageNumber, PageSize);
    }

    public string GetFilterUrl()
    {
        var parts = new List<string>();

        if (Status.HasValue)
            parts.Add($"status={Status}");
        if (Priority.HasValue)
            parts.Add($"priority={Priority}");
        if (PropertyId.HasValue)
            parts.Add($"propertyId={PropertyId}");
        if (Category.HasValue)
            parts.Add($"category={Category}");

        return parts.Count > 0
            ? $"/Maintenance?{string.Join('&', parts)}"
            : "/Maintenance?";
    }
}
