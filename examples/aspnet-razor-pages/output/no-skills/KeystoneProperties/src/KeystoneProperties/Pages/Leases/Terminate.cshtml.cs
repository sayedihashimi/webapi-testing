using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Models.Enums;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Leases;

public class TerminateModel : PageModel
{
    private readonly ILeaseService _leaseService;
    public TerminateModel(ILeaseService leaseService) { _leaseService = leaseService; }

    public Lease Lease { get; set; } = null!;
    [BindProperty] public int LeaseId { get; set; }
    [BindProperty] public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required, Display(Name = "Termination Date")]
        public DateOnly TerminationDate { get; set; }
        [Required, Display(Name = "Termination Reason")]
        public string TerminationReason { get; set; } = string.Empty;
        [Display(Name = "Deposit Disposition")]
        public DepositStatus DepositDisposition { get; set; } = DepositStatus.Returned;
        [Display(Name = "Deposit Return Amount"), DataType(DataType.Currency)]
        public decimal? DepositReturnAmount { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var lease = await _leaseService.GetByIdAsync(id);
        if (lease == null) return NotFound();
        Lease = lease;
        LeaseId = id;
        Input.TerminationDate = DateOnly.FromDateTime(DateTime.Today);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var lease = await _leaseService.GetByIdAsync(LeaseId);
        if (lease == null) return NotFound();
        Lease = lease;

        if (!ModelState.IsValid) return Page();

        var (success, error) = await _leaseService.TerminateAsync(
            LeaseId, Input.TerminationReason, Input.TerminationDate,
            Input.DepositDisposition, Input.DepositReturnAmount);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, error!);
            return Page();
        }

        TempData["SuccessMessage"] = "Lease terminated successfully.";
        return RedirectToPage("Details", new { id = LeaseId });
    }
}
