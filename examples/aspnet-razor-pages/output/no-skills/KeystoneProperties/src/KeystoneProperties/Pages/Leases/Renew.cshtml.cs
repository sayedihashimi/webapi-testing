using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Leases;

public class RenewModel : PageModel
{
    private readonly ILeaseService _leaseService;
    public RenewModel(ILeaseService leaseService) { _leaseService = leaseService; }

    public Lease Lease { get; set; } = null!;
    [BindProperty] public int LeaseId { get; set; }
    public DateOnly NewStartDate { get; set; }
    [BindProperty] public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required, Display(Name = "New End Date")]
        public DateOnly NewEndDate { get; set; }
        [Required, Range(0.01, double.MaxValue), Display(Name = "New Monthly Rent"), DataType(DataType.Currency)]
        public decimal NewRentAmount { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var lease = await _leaseService.GetByIdAsync(id);
        if (lease == null) return NotFound();
        Lease = lease;
        LeaseId = id;
        NewStartDate = lease.EndDate.AddDays(1);
        Input.NewEndDate = NewStartDate.AddYears(1);
        Input.NewRentAmount = lease.MonthlyRentAmount;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var lease = await _leaseService.GetByIdAsync(LeaseId);
        if (lease == null) return NotFound();
        Lease = lease;
        NewStartDate = lease.EndDate.AddDays(1);

        if (!ModelState.IsValid) return Page();

        var (success, error, newLease) = await _leaseService.RenewAsync(LeaseId, Input.NewEndDate, Input.NewRentAmount);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error!);
            return Page();
        }

        TempData["SuccessMessage"] = "Lease renewed successfully.";
        return RedirectToPage("Details", new { id = newLease!.Id });
    }
}
