using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Leases;

public sealed class DetailsModel(ILeaseService leaseService) : PageModel
{
    public Lease Lease { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var lease = await leaseService.GetByIdAsync(id, ct);
        if (lease is null)
        {
            return NotFound();
        }

        Lease = lease;
        return Page();
    }
}
