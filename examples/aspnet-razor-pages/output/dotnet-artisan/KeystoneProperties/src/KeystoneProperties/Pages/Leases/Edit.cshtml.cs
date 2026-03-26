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

    public Lease Lease { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var lease = await leaseService.GetByIdAsync(id);
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

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            var lease = await leaseService.GetByIdAsync(Input.Id);
            if (lease is null)
            {
                return NotFound();
            }
            Lease = lease;
            return Page();
        }

        var existing = await leaseService.GetByIdAsync(Input.Id);
        if (existing is null)
        {
            return NotFound();
        }

        existing.StartDate = Input.StartDate;
        existing.EndDate = Input.EndDate;
        existing.MonthlyRentAmount = Input.MonthlyRentAmount;
        existing.DepositAmount = Input.DepositAmount;
        existing.Status = Input.Status;
        existing.Notes = Input.Notes;

        var (success, error) = await leaseService.UpdateAsync(existing);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, error!);
            Lease = existing;
            return Page();
        }

        TempData["SuccessMessage"] = "Lease was updated successfully.";
        return RedirectToPage("Details", new { id = Input.Id });
    }

    public sealed class InputModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        public DateOnly StartDate { get; set; }

        [Required]
        [Display(Name = "End Date")]
        public DateOnly EndDate { get; set; }

        [Required, Range(0.01, double.MaxValue, ErrorMessage = "Monthly rent must be positive")]
        [Display(Name = "Monthly Rent")]
        [DataType(DataType.Currency)]
        public decimal MonthlyRentAmount { get; set; }

        [Required, Range(0.01, double.MaxValue, ErrorMessage = "Deposit must be positive")]
        [Display(Name = "Deposit Amount")]
        [DataType(DataType.Currency)]
        public decimal DepositAmount { get; set; }

        [Display(Name = "Status")]
        public LeaseStatus Status { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }
}
