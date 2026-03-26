using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Leases;

public class DetailsModel : PageModel
{
    private readonly ILeaseService _leaseService;
    public DetailsModel(ILeaseService leaseService) { _leaseService = leaseService; }

    public Lease Lease { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var lease = await _leaseService.GetWithDetailsAsync(id);
        if (lease == null) return NotFound();
        Lease = lease;
        return Page();
    }
}
