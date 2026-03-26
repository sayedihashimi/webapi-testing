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

    public Lease Lease { get; set; } = null!;
    public DateOnly NewStartDate { get; set; }

    public sealed class InputModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "New End Date")]
        public DateOnly NewEndDate { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Monthly rent must be positive.")]
        [Display(Name = "New Monthly Rent")]
        public decimal NewMonthlyRent { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var lease = await leaseService.GetByIdAsync(id, ct);
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
        NewStartDate = lease.EndDate.AddDays(1);
        Input = new InputModel
        {
            Id = lease.Id,
            NewEndDate = NewStartDate.AddYears(1),
            NewMonthlyRent = lease.MonthlyRentAmount
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            var current = await leaseService.GetByIdAsync(Input.Id, ct);
            if (current is null) return NotFound();
            Lease = current;
            NewStartDate = current.EndDate.AddDays(1);
            return Page();
        }

        var (newLease, error) = await leaseService.RenewAsync(
            Input.Id,
            Input.NewEndDate,
            Input.NewMonthlyRent,
            ct);

        if (error is not null)
        {
            ModelState.AddModelError(string.Empty, error);
            var current = await leaseService.GetByIdAsync(Input.Id, ct);
            if (current is null) return NotFound();
            Lease = current;
            NewStartDate = current.EndDate.AddDays(1);
            return Page();
        }

        TempData["SuccessMessage"] = "Lease renewed successfully.";
        return RedirectToPage("Details", new { id = newLease!.Id });
    }
}
