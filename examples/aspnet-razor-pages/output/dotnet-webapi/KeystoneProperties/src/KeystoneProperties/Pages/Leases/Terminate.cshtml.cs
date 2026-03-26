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

    public Lease Lease { get; set; } = null!;

    public sealed class InputModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Termination Date")]
        public DateOnly TerminationDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Required(ErrorMessage = "Please provide a reason for termination.")]
        [MaxLength(2000)]
        [Display(Name = "Reason for Termination")]
        public string TerminationReason { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Deposit Status")]
        public DepositStatus DepositStatus { get; set; } = DepositStatus.Returned;

        [Display(Name = "Deposit Return Amount")]
        [Range(0, double.MaxValue, ErrorMessage = "Deposit return amount must be zero or positive.")]
        public decimal? DepositReturnAmount { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var lease = await leaseService.GetByIdAsync(id, ct);
        if (lease is null)
        {
            return NotFound();
        }

        if (lease.Status != LeaseStatus.Active)
        {
            TempData["ErrorMessage"] = "Only active leases can be terminated.";
            return RedirectToPage("Details", new { id });
        }

        Lease = lease;
        Input = new InputModel
        {
            Id = lease.Id,
            DepositReturnAmount = lease.DepositAmount
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            var current = await leaseService.GetByIdAsync(Input.Id, ct);
            if (current is null) return NotFound();
            Lease = current;
            return Page();
        }

        var (success, error) = await leaseService.TerminateAsync(
            Input.Id,
            Input.TerminationDate,
            Input.TerminationReason,
            Input.DepositStatus,
            Input.DepositReturnAmount,
            ct);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "An error occurred while terminating the lease.");
            var current = await leaseService.GetByIdAsync(Input.Id, ct);
            if (current is null) return NotFound();
            Lease = current;
            return Page();
        }

        TempData["SuccessMessage"] = "Lease terminated successfully.";
        return RedirectToPage("Details", new { id = Input.Id });
    }
}
