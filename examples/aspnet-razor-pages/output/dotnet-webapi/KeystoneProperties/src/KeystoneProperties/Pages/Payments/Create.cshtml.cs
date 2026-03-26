using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KeystoneProperties.Pages.Payments;

public sealed class CreateModel(IPaymentService paymentService, ILeaseService leaseService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<Lease> ActiveLeases { get; set; } = [];
    public List<SelectListItem> LeaseOptions { get; set; } = [];

    public sealed class InputModel
    {
        [Required(ErrorMessage = "Please select a lease.")]
        [Display(Name = "Lease")]
        public int LeaseId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be positive.")]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Payment Date")]
        public DateOnly PaymentDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Required]
        [Display(Name = "Due Date")]
        public DateOnly DueDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Required]
        [Display(Name = "Payment Method")]
        public PaymentMethod PaymentMethod { get; set; }

        [Required]
        [Display(Name = "Payment Type")]
        public PaymentType PaymentType { get; set; } = PaymentType.Rent;

        [Required]
        public PaymentStatus Status { get; set; } = PaymentStatus.Completed;

        [MaxLength(100)]
        [Display(Name = "Reference Number")]
        public string? ReferenceNumber { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public async Task OnGetAsync(CancellationToken ct)
    {
        await LoadLeasesAsync(ct);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await LoadLeasesAsync(ct);
            return Page();
        }

        var payment = new Payment
        {
            LeaseId = Input.LeaseId,
            Amount = Input.Amount,
            PaymentDate = Input.PaymentDate,
            DueDate = Input.DueDate,
            PaymentMethod = Input.PaymentMethod,
            PaymentType = Input.PaymentType,
            Status = Input.Status,
            ReferenceNumber = Input.ReferenceNumber,
            Notes = Input.Notes
        };

        var (created, lateFee, error) = await paymentService.CreateAsync(payment, ct);

        if (error is not null)
        {
            TempData["ErrorMessage"] = error;
            await LoadLeasesAsync(ct);
            return Page();
        }

        if (lateFee is not null)
        {
            TempData["SuccessMessage"] = $"Payment recorded successfully. A late fee of {lateFee.Amount:C} was also automatically generated.";
        }
        else
        {
            TempData["SuccessMessage"] = "Payment recorded successfully.";
        }

        return RedirectToPage("Details", new { id = created!.Id });
    }

    private async Task LoadLeasesAsync(CancellationToken ct)
    {
        ActiveLeases = await leaseService.GetActiveLeasesAsync(ct);
        LeaseOptions = ActiveLeases
            .Select(l => new SelectListItem(
                $"{l.Tenant.FullName} - {l.Unit.Property.Name} Unit {l.Unit.UnitNumber}",
                l.Id.ToString()))
            .ToList();
    }
}
