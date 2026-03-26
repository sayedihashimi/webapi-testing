using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Leases;

public class TerminateModel : PageModel
{
    private readonly ILeaseService _leaseService;

    public TerminateModel(ILeaseService leaseService)
    {
        _leaseService = leaseService;
    }

    public Lease Lease { get; set; } = null!;

    [BindProperty]
    public TerminateInputModel Input { get; set; } = new();

    public class TerminateInputModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Termination reason is required.")]
        [MaxLength(2000)]
        [Display(Name = "Termination Reason")]
        public string TerminationReason { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Deposit Status")]
        public DepositStatus DepositStatus { get; set; }

        [Display(Name = "Deposit Return Amount")]
        [DataType(DataType.Currency)]
        [Range(0, double.MaxValue, ErrorMessage = "Deposit return amount must be non-negative.")]
        public decimal? DepositReturnAmount { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var lease = await _leaseService.GetWithDetailsAsync(id);
        if (lease is null)
        {
            return NotFound();
        }

        if (lease.Status != LeaseStatus.Active && lease.Status != LeaseStatus.Pending)
        {
            TempData["ErrorMessage"] = "Only active or pending leases can be terminated.";
            return RedirectToPage("Details", new { id });
        }

        Lease = lease;
        Input = new TerminateInputModel { Id = id };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var lease = await _leaseService.GetWithDetailsAsync(Input.Id);
        if (lease is null)
        {
            return NotFound();
        }

        if (lease.Status != LeaseStatus.Active && lease.Status != LeaseStatus.Pending)
        {
            TempData["ErrorMessage"] = "Only active or pending leases can be terminated.";
            return RedirectToPage("Details", new { id = Input.Id });
        }

        Lease = lease;

        // Require DepositReturnAmount when deposit is being returned
        if ((Input.DepositStatus == DepositStatus.Returned || Input.DepositStatus == DepositStatus.PartiallyReturned)
            && !Input.DepositReturnAmount.HasValue)
        {
            ModelState.AddModelError("Input.DepositReturnAmount", "Deposit return amount is required when returning the deposit.");
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var (success, error) = await _leaseService.TerminateAsync(
            Input.Id,
            Input.TerminationReason,
            Input.DepositStatus,
            Input.DepositReturnAmount);

        if (success)
        {
            TempData["SuccessMessage"] = "Lease terminated successfully.";
        }
        else
        {
            TempData["ErrorMessage"] = error ?? "Failed to terminate lease.";
        }

        return RedirectToPage("Details", new { id = Input.Id });
    }
}
