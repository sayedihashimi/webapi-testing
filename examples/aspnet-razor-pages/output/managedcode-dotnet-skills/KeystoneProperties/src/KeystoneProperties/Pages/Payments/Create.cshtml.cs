using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KeystoneProperties.Pages.Payments;

public class CreateModel(IPaymentService paymentService, ILeaseService leaseService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<Lease> ActiveLeases { get; set; } = [];

    public SelectList LeaseSelectList => new(
        ActiveLeases.Select(l => new
        {
            l.Id,
            DisplayText = $"{l.Tenant.FullName} - Unit {l.Unit.UnitNumber} at {l.Unit.Property.Name}"
        }),
        "Id",
        "DisplayText",
        Input.LeaseId);

    public async Task OnGetAsync(int? leaseId)
    {
        await LoadActiveLeasesAsync();

        if (leaseId.HasValue)
        {
            Input.LeaseId = leaseId.Value;
            Input.DueDate = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);
        }

        Input.PaymentDate = DateOnly.FromDateTime(DateTime.Today);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadActiveLeasesAsync();

        if (!ModelState.IsValid)
        {
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
            Status = PaymentStatus.Completed,
            ReferenceNumber = Input.ReferenceNumber,
            Notes = Input.Notes
        };

        var (success, errorMessage, lateFeePayment) = await paymentService.RecordPaymentAsync(payment);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, errorMessage ?? "Failed to record payment.");
            return Page();
        }

        TempData["SuccessMessage"] = $"Payment of {payment.Amount:C} recorded successfully.";

        if (lateFeePayment is not null)
        {
            TempData["LateFeeMessage"] = $"A late fee of {lateFeePayment.Amount:C} was automatically generated.";
        }

        return RedirectToPage("Index");
    }

    private async Task LoadActiveLeasesAsync()
    {
        var leases = await leaseService.GetLeasesAsync(LeaseStatus.Active, null, 1, int.MaxValue);
        ActiveLeases = [.. leases];
    }

    public class InputModel
    {
        [Required]
        [Display(Name = "Lease")]
        public int LeaseId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero.")]
        public decimal Amount { get; set; }

        [Required]
        [Display(Name = "Payment Date")]
        public DateOnly PaymentDate { get; set; }

        [Required]
        [Display(Name = "Due Date")]
        public DateOnly DueDate { get; set; }

        [Required]
        [Display(Name = "Payment Method")]
        public PaymentMethod PaymentMethod { get; set; }

        [Required]
        [Display(Name = "Payment Type")]
        public PaymentType PaymentType { get; set; }

        [MaxLength(100)]
        [Display(Name = "Reference Number")]
        public string? ReferenceNumber { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
