using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Leases;

public sealed class EditModel(ILeaseService leaseService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public Lease Lease { get; set; } = null!;

    public sealed class InputModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        public DateOnly StartDate { get; set; }

        [Required]
        [Display(Name = "End Date")]
        public DateOnly EndDate { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Monthly rent must be positive.")]
        [Display(Name = "Monthly Rent")]
        public decimal MonthlyRentAmount { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Deposit amount must be positive.")]
        [Display(Name = "Deposit Amount")]
        public decimal DepositAmount { get; set; }

        [Display(Name = "Status")]
        public LeaseStatus Status { get; set; }

        [MaxLength(2000)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var lease = await leaseService.GetByIdAsync(id, ct);
        if (lease is null)
        {
            return NotFound();
        }

        Lease = lease;
        Input = new InputModel
        {
            Id = lease.Id,
            StartDate = lease.StartDate,
            EndDate = lease.EndDate,
            MonthlyRentAmount = lease.MonthlyRentAmount,
            DepositAmount = lease.DepositAmount,
            Status = lease.Status,
            Notes = lease.Notes
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
            return Page();
        }

        var lease = await leaseService.GetByIdAsync(Input.Id, ct);
        if (lease is null)
        {
            return NotFound();
        }

        // For active leases, only allow editing Notes
        if (lease.Status == LeaseStatus.Active)
        {
            lease.Notes = Input.Notes;
        }
        else
        {
            lease.StartDate = Input.StartDate;
            lease.EndDate = Input.EndDate;
            lease.MonthlyRentAmount = Input.MonthlyRentAmount;
            lease.DepositAmount = Input.DepositAmount;
            lease.Status = Input.Status;
            lease.Notes = Input.Notes;
        }

        var error = await leaseService.UpdateAsync(lease, ct);
        if (error is not null)
        {
            ModelState.AddModelError(string.Empty, error);
            Lease = lease;
            return Page();
        }

        TempData["SuccessMessage"] = "Lease updated successfully.";
        return RedirectToPage("Details", new { id = Input.Id });
    }
}
