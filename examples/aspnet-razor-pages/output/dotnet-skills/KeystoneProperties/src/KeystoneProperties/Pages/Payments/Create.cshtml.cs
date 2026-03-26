using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KeystoneProperties.Pages.Payments;

public class CreateModel : PageModel
{
    private readonly IPaymentService _paymentService;
    private readonly ILeaseService _leaseService;

    public CreateModel(IPaymentService paymentService, ILeaseService leaseService)
    {
        _paymentService = paymentService;
        _leaseService = leaseService;
    }

    [BindProperty]
    public PaymentInputModel Input { get; set; } = new();

    public SelectList LeaseList { get; set; } = null!;

    public class PaymentInputModel
    {
        [Required]
        [Display(Name = "Lease")]
        public int LeaseId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be positive.")]
        [DataType(DataType.Currency)]
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
        public PaymentType PaymentType { get; set; }

        [Required]
        public PaymentStatus Status { get; set; } = PaymentStatus.Completed;

        [MaxLength(100)]
        [Display(Name = "Reference Number")]
        public string? ReferenceNumber { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }
    }

    public async Task OnGetAsync()
    {
        await LoadLeasesAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadLeasesAsync();
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

        var (success, error, lateFeePayment) = await _paymentService.RecordPaymentAsync(payment);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "An error occurred while recording the payment.");
            await LoadLeasesAsync();
            return Page();
        }

        if (lateFeePayment is not null)
        {
            TempData["SuccessMessage"] = $"Payment recorded. Late fee of {lateFeePayment.Amount.ToString("C")} was automatically generated.";
        }
        else
        {
            TempData["SuccessMessage"] = "Payment recorded successfully.";
        }

        return RedirectToPage("Index");
    }

    private async Task LoadLeasesAsync()
    {
        var leases = await _leaseService.GetLeasesAsync(
            status: LeaseStatus.Active,
            propertyId: null,
            search: null,
            pageNumber: 1,
            pageSize: 500);

        var leaseItems = leases.Select(l => new
        {
            l.Id,
            Display = $"{l.Tenant.FullName} - {l.Unit.Property.Name} Unit# {l.Unit.UnitNumber} (Lease #{l.Id})"
        });

        LeaseList = new SelectList(leaseItems, "Id", "Display");
    }
}
