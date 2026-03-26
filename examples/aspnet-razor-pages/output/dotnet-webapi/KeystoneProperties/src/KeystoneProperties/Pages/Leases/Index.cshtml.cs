using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Leases;

public sealed class IndexModel(ILeaseService leaseService, IUnitService unitService) : PageModel
{
    public PaginatedList<Lease> Leases { get; set; } = null!;
    public List<Property> Properties { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public LeaseStatus? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? PropertyId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public async Task OnGetAsync(CancellationToken ct)
    {
        Properties = await unitService.GetAllPropertiesAsync(ct);
        Leases = await leaseService.GetAllAsync(Status, PropertyId, PageNumber, 10, ct);
    }
}
