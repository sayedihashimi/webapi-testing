using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Leases;

public class DetailsModel : PageModel
{
    private readonly ILeaseService _leaseService;

    public DetailsModel(ILeaseService leaseService)
    {
        _leaseService = leaseService;
    }

    public Lease Lease { get; set; } = null!;
    public Lease? RenewalLease { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var lease = await _leaseService.GetWithDetailsAsync(id);
        if (lease is null)
        {
            return NotFound();
        }

        Lease = lease;

        // Check if this lease was renewed (another lease references this one)
        var allLeases = await _leaseService.GetLeasesAsync(null, null, null, 1, int.MaxValue);
        RenewalLease = allLeases.FirstOrDefault(l => l.RenewalOfLeaseId == id);

        return Page();
    }

    public async Task<IActionResult> OnPostActivateAsync(int id)
    {
        var (success, error) = await _leaseService.ActivateAsync(id);
        if (success)
        {
            TempData["SuccessMessage"] = "Lease activated successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = error ?? "Failed to activate lease.";
        }

        return RedirectToPage("Details", new { id });
    }
}
