using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Leases;

public class EditModel : PageModel
{
    private readonly ILeaseService _leaseService;

    public EditModel(ILeaseService leaseService)
    {
        _leaseService = leaseService;
    }

    public Lease Lease { get; set; } = null!;
    public bool IsActive { get; set; }

    [BindProperty]
    public LeaseEditInputModel Input { get; set; } = new();

    public class LeaseEditInputModel : IValidatableObject
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
        [DataType(DataType.Currency)]
        public decimal MonthlyRentAmount { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Deposit amount must be positive.")]
        [Display(Name = "Deposit Amount")]
        [DataType(DataType.Currency)]
        public decimal DepositAmount { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndDate <= StartDate)
            {
                yield return new ValidationResult(
                    "End date must be after the start date.",
                    new[] { nameof(EndDate) });
            }
        }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var lease = await _leaseService.GetWithDetailsAsync(id);
        if (lease is null)
        {
            return NotFound();
        }

        Lease = lease;
        IsActive = lease.Status == LeaseStatus.Active;

        Input = new LeaseEditInputModel
        {
            Id = lease.Id,
            StartDate = lease.StartDate,
            EndDate = lease.EndDate,
            MonthlyRentAmount = lease.MonthlyRentAmount,
            DepositAmount = lease.DepositAmount,
            Notes = lease.Notes
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

        Lease = lease;
        IsActive = lease.Status == LeaseStatus.Active;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (IsActive)
        {
            // Only allow editing Notes and MonthlyRentAmount for active leases
            lease.MonthlyRentAmount = Input.MonthlyRentAmount;
            lease.Notes = Input.Notes;
        }
        else
        {
            lease.StartDate = Input.StartDate;
            lease.EndDate = Input.EndDate;
            lease.MonthlyRentAmount = Input.MonthlyRentAmount;
            lease.DepositAmount = Input.DepositAmount;
            lease.Notes = Input.Notes;
        }

        var (success, error) = await _leaseService.UpdateAsync(lease);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Failed to update lease.");
            return Page();
        }

        TempData["SuccessMessage"] = "Lease updated successfully.";
        return RedirectToPage("Details", new { id = Input.Id });
    }
}
