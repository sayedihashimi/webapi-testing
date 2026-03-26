using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KeystoneProperties.Pages.Payments;

public sealed class CreateModel(
    IPaymentService paymentService,
    ILeaseService leaseService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList Leases { get; set; } = null!;

    public sealed class InputModel
    {
        [Required]
        [Display(Name = "Lease")]
        public int LeaseId { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be positive")]
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
            ReferenceNumber = Input.ReferenceNumber,
            Notes = Input.Notes
        };

        var (success, error) = await paymentService.RecordPaymentAsync(payment);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "An error occurred while recording the payment.");
            await LoadLeasesAsync();
            return Page();
        }

        TempData["SuccessMessage"] = "Payment recorded successfully.";
        return RedirectToPage("Index");
    }

    private async Task LoadLeasesAsync()
    {
        var activeLeases = await leaseService.GetActiveLeasesAsync();
        var leaseItems = activeLeases.Select(l => new
        {
            l.Id,
            Display = $"{l.Tenant.FullName} - {l.Unit.Property.Name} #{l.Unit.UnitNumber}"
        });
        Leases = new SelectList(leaseItems, "Id", "Display");
    }
}
