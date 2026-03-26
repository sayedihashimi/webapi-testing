using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Leases;

public sealed class DetailsModel(ILeaseService leaseService) : PageModel
{
    public Lease Lease { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var lease = await leaseService.GetWithDetailsAsync(id);
        if (lease is null)
        {
            return NotFound();
        }

        Lease = lease;
        return Page();
    }
}
