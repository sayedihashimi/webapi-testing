using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Data;
using KeystoneProperties.Models;
using KeystoneProperties.Models.Enums;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Pages.Payments;

public class CreateModel : PageModel
{
    private readonly IPaymentService _paymentService;
    private readonly ApplicationDbContext _context;

    public CreateModel(IPaymentService paymentService, ApplicationDbContext context)
    {
        _paymentService = paymentService;
        _context = context;
    }

    [BindProperty] public InputModel Input { get; set; } = new();
    public List<Lease> ActiveLeases { get; set; } = new();

    public class InputModel
    {
        [Required, Display(Name = "Lease")] public int LeaseId { get; set; }
        [Required, Range(0.01, double.MaxValue), DataType(DataType.Currency)]
        public decimal Amount { get; set; }
        [Required, Display(Name = "Payment Date")] public DateOnly PaymentDate { get; set; }
        [Required, Display(Name = "Due Date")] public DateOnly DueDate { get; set; }
        [Required, Display(Name = "Payment Method")] public PaymentMethod PaymentMethod { get; set; }
        [Required, Display(Name = "Payment Type")] public PaymentType PaymentType { get; set; }
        [MaxLength(100), Display(Name = "Reference Number")] public string? ReferenceNumber { get; set; }
        [MaxLength(500)] public string? Notes { get; set; }
    }

    public async Task OnGetAsync()
    {
        await LoadLeases();
        Input.PaymentDate = DateOnly.FromDateTime(DateTime.Today);
        Input.DueDate = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadLeases();
        if (!ModelState.IsValid) return Page();

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

        var (success, error, lateFee) = await _paymentService.RecordPaymentAsync(payment);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error!);
            return Page();
        }

        var msg = "Payment recorded successfully.";
        if (lateFee != null)
            msg += $" A late fee of {lateFee.Amount:C} was also applied.";
        TempData["SuccessMessage"] = msg;
        return RedirectToPage("Details", new { id = payment.Id });
    }

    private async Task LoadLeases()
    {
        ActiveLeases = await _context.Leases
            .Include(l => l.Tenant)
            .Include(l => l.Unit).ThenInclude(u => u.Property)
            .Where(l => l.Status == LeaseStatus.Active)
            .OrderBy(l => l.Tenant.LastName)
            .ToListAsync();
    }
}
