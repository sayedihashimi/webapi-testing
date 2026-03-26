using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Leases;

public sealed class RenewModel(ILeaseService leaseService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public Lease Lease { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var lease = await leaseService.GetByIdAsync(id);
        if (lease is null)
        {
            return NotFound();
        }

        Lease = lease;

        var newStart = lease.EndDate.AddDays(1);
        Input = new InputModel
        {
            LeaseId = id,
            NewStartDate = newStart,
            NewEndDate = newStart.AddYears(1),
            NewMonthlyRent = lease.MonthlyRentAmount
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            var lease = await leaseService.GetByIdAsync(Input.LeaseId);
            if (lease is null)
            {
                return NotFound();
            }
            Lease = lease;
            return Page();
        }

        var (success, error, newLease) = await leaseService.RenewAsync(
            Input.LeaseId,
            Input.NewEndDate,
            Input.NewMonthlyRent);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, error!);
            var lease = await leaseService.GetByIdAsync(Input.LeaseId);
            if (lease is null)
            {
                return NotFound();
            }
            Lease = lease;
            return Page();
        }

        TempData["SuccessMessage"] = $"Lease was renewed successfully. New Lease #{newLease!.Id} created.";
        return RedirectToPage("Details", new { id = newLease.Id });
    }

    public sealed class InputModel
    {
        public int LeaseId { get; set; }

        [Display(Name = "New Start Date")]
        public DateOnly NewStartDate { get; set; }

        [Required]
        [Display(Name = "New End Date")]
        public DateOnly NewEndDate { get; set; }

        [Required, Range(0.01, double.MaxValue, ErrorMessage = "Monthly rent must be positive")]
        [Display(Name = "New Monthly Rent")]
        [DataType(DataType.Currency)]
        public decimal NewMonthlyRent { get; set; }
    }
}
