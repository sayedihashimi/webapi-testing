using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Leases;

public class RenewModel : PageModel
{
    private readonly ILeaseService _leaseService;

    public RenewModel(ILeaseService leaseService)
    {
        _leaseService = leaseService;
    }

    public Lease Lease { get; set; } = null!;

    [BindProperty]
    public RenewInputModel Input { get; set; } = new();

    public class RenewInputModel
    {
        public int Id { get; set; }

        [Display(Name = "New Start Date")]
        public DateOnly NewStartDate { get; set; }

        [Required]
        [Display(Name = "New End Date")]
        public DateOnly NewEndDate { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Monthly rent must be positive.")]
        [Display(Name = "New Monthly Rent")]
        [DataType(DataType.Currency)]
        public decimal NewRentAmount { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var lease = await _leaseService.GetWithDetailsAsync(id);
        if (lease is null)
        {
            return NotFound();
        }

        if (lease.Status != LeaseStatus.Active)
        {
            TempData["ErrorMessage"] = "Only active leases can be renewed.";
            return RedirectToPage("Details", new { id });
        }

        Lease = lease;

        var newStart = lease.EndDate.AddDays(1);
        Input = new RenewInputModel
        {
            Id = id,
            NewStartDate = newStart,
            NewEndDate = newStart.AddYears(1),
            NewRentAmount = lease.MonthlyRentAmount
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var lease = await _leaseService.GetWithDetailsAsync(Input.Id);
        if (lease is null)
        {
            return NotFound();
        }

        if (lease.Status != LeaseStatus.Active)
        {
            TempData["ErrorMessage"] = "Only active leases can be renewed.";
            return RedirectToPage("Details", new { id = Input.Id });
        }

        Lease = lease;

        if (Input.NewEndDate <= Input.NewStartDate)
        {
            ModelState.AddModelError("Input.NewEndDate", "New end date must be after the new start date.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var (success, error, newLease) = await _leaseService.RenewAsync(
            Input.Id,
            Input.NewEndDate,
            Input.NewRentAmount);

        if (success && newLease is not null)
        {
            TempData["SuccessMessage"] = "Lease renewed successfully.";
            return RedirectToPage("Details", new { id = newLease.Id });
        }

        TempData["ErrorMessage"] = error ?? "Failed to renew lease.";
        return RedirectToPage("Details", new { id = Input.Id });
    }
}
