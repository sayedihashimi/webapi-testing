using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Leases;

public sealed class TerminateModel(ILeaseService leaseService) : PageModel
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
        Input.LeaseId = id;
        Input.TerminationDate = DateOnly.FromDateTime(DateTime.Today);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            var lease = await leaseService.GetByIdAsync(Input.LeaseId);
            if (lease is null)
            {
                return NotFound();
            }
            Lease = lease;
            return Page();
        }

        var (success, error) = await leaseService.TerminateAsync(
            Input.LeaseId,
            Input.TerminationDate,
            Input.TerminationReason,
            Input.DepositStatus,
            Input.DepositReturnAmount);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, error!);
            var lease = await leaseService.GetByIdAsync(Input.LeaseId);
            if (lease is null)
            {
                return NotFound();
            }
            Lease = lease;
            return Page();
        }

        TempData["SuccessMessage"] = "Lease was terminated successfully.";
        return RedirectToPage("Details", new { id = Input.LeaseId });
    }

    public sealed class InputModel
    {
        public int LeaseId { get; set; }

        [Required]
        [Display(Name = "Termination Date")]
        public DateOnly TerminationDate { get; set; }

        [Required, MaxLength(2000)]
        [Display(Name = "Termination Reason")]
        public string TerminationReason { get; set; } = string.Empty;

        [Display(Name = "Deposit Disposition")]
        public DepositStatus DepositStatus { get; set; } = DepositStatus.Held;

        [Display(Name = "Deposit Return Amount")]
        [DataType(DataType.Currency)]
        [Range(0, double.MaxValue, ErrorMessage = "Amount must be zero or positive")]
        public decimal? DepositReturnAmount { get; set; }
    }
}
