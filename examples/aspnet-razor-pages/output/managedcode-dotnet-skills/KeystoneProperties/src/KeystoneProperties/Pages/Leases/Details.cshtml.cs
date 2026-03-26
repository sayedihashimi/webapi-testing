using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Leases;

public class DetailsModel(ILeaseService leaseService) : PageModel
{
    public Lease Lease { get; set; } = default!;
    public List<Lease> RenewalChain { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var lease = await leaseService.GetLeaseWithDetailsAsync(id);

        if (lease is null)
        {
            return NotFound();
        }

        Lease = lease;

        // Build renewal chain by following RenewalOfLeaseId links backwards
        RenewalChain = [];
        var current = lease;
        while (current.RenewalOfLeaseId.HasValue)
        {
            var previous = await leaseService.GetLeaseByIdAsync(current.RenewalOfLeaseId.Value);
            if (previous is null)
            {
                break;
            }

            RenewalChain.Insert(0, previous);
            current = previous;
        }

        // Add the current lease at the end of the chain
        RenewalChain.Add(lease);

        return Page();
    }
}
