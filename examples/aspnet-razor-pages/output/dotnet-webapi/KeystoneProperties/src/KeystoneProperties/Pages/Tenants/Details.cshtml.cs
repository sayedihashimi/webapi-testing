using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Tenants;

public sealed class DetailsModel(ITenantService tenantService) : PageModel
{
    public Tenant Tenant { get; set; } = null!;

    public List<Lease> CurrentLeases { get; set; } = [];

    public List<Lease> PastLeases { get; set; } = [];

    public List<Payment> RecentPayments { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var tenant = await tenantService.GetByIdAsync(id, ct);
        if (tenant is null)
        {
            return NotFound();
        }

        Tenant = tenant;

        CurrentLeases = tenant.Leases
            .Where(l => l.Status is LeaseStatus.Active or LeaseStatus.Pending)
            .OrderByDescending(l => l.StartDate)
            .ToList();

        PastLeases = tenant.Leases
            .Where(l => l.Status is LeaseStatus.Expired or LeaseStatus.Terminated or LeaseStatus.Renewed)
            .OrderByDescending(l => l.EndDate)
            .ToList();

        RecentPayments = tenant.Leases
            .SelectMany(l => l.Payments)
            .OrderByDescending(p => p.PaymentDate)
            .Take(10)
            .ToList();

        return Page();
    }
}
