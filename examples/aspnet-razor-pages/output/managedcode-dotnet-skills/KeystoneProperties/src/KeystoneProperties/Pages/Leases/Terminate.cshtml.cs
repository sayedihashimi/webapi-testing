using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Leases;

public class TerminateModel(ILeaseService leaseService) : PageModel
{
    public Lease Lease { get; set; } = default!;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [MaxLength(2000)]
        [Display(Name = "Reason for Termination")]
        public string TerminationReason { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Deposit Disposition")]
        public DepositStatus DepositDisposition { get; set; }

        [Display(Name = "Deposit Return Amount")]
        public decimal? DepositReturnAmount { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var lease = await leaseService.GetLeaseWithDetailsAsync(id);

        if (lease is null)
        {
            return NotFound();
        }

        Lease = lease;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var lease = await leaseService.GetLeaseWithDetailsAsync(id);

        if (lease is null)
        {
            return NotFound();
        }

        Lease = lease;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (Input.DepositDisposition == DepositStatus.PartiallyReturned)
        {
            if (!Input.DepositReturnAmount.HasValue || Input.DepositReturnAmount <= 0)
            {
                ModelState.AddModelError("Input.DepositReturnAmount",
                    "Deposit return amount is required and must be greater than zero for partial returns.");
                return Page();
            }

            if (Input.DepositReturnAmount >= lease.DepositAmount)
            {
                ModelState.AddModelError("Input.DepositReturnAmount",
                    $"Deposit return amount must be less than the original deposit of {lease.DepositAmount:C}.");
                return Page();
            }
        }

        var depositReturnAmount = Input.DepositDisposition switch
        {
            DepositStatus.Returned => lease.DepositAmount,
            DepositStatus.PartiallyReturned => Input.DepositReturnAmount,
            _ => null
        };

        var (success, errorMessage) = await leaseService.TerminateLeaseAsync(
            id, Input.TerminationReason, Input.DepositDisposition, depositReturnAmount);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage;
            return Page();
        }

        TempData["SuccessMessage"] = "Lease terminated successfully.";
        return RedirectToPage("Details", new { id });
    }
}
