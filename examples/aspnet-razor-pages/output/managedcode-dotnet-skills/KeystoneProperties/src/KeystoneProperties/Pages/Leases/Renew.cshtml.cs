using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Leases;

public class RenewModel(ILeaseService leaseService) : PageModel
{
    public Lease OriginalLease { get; set; } = default!;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [Display(Name = "New End Date")]
        public DateOnly NewEndDate { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        [Display(Name = "New Monthly Rent")]
        public decimal NewMonthlyRent { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var lease = await leaseService.GetLeaseWithDetailsAsync(id);

        if (lease is null)
        {
            return NotFound();
        }

        OriginalLease = lease;

        Input = new InputModel
        {
            NewEndDate = lease.EndDate.AddYears(1),
            NewMonthlyRent = lease.MonthlyRentAmount
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var lease = await leaseService.GetLeaseWithDetailsAsync(id);

        if (lease is null)
        {
            return NotFound();
        }

        OriginalLease = lease;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var (success, errorMessage) = await leaseService.RenewLeaseAsync(
            id, Input.NewEndDate, Input.NewMonthlyRent);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage;
            return Page();
        }

        TempData["SuccessMessage"] = "Lease renewed successfully.";
        return RedirectToPage("Details", new { id });
    }
}
