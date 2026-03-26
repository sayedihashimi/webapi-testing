using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Leases;

public class EditModel : PageModel
{
    private readonly ILeaseService _leaseService;
    public EditModel(ILeaseService leaseService) { _leaseService = leaseService; }

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    [BindProperty] public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required, Display(Name = "Start Date")] public DateOnly StartDate { get; set; }
        [Required, Display(Name = "End Date")] public DateOnly EndDate { get; set; }
        [Required, Range(0.01, double.MaxValue), Display(Name = "Monthly Rent")]
        public decimal MonthlyRentAmount { get; set; }
        [Required, Range(0.01, double.MaxValue), Display(Name = "Deposit Amount")]
        public decimal DepositAmount { get; set; }
        [MaxLength(2000)] public string? Notes { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var lease = await _leaseService.GetByIdAsync(id);
        if (lease == null) return NotFound();
        Id = id;
        Input = new InputModel
        {
            StartDate = lease.StartDate, EndDate = lease.EndDate,
            MonthlyRentAmount = lease.MonthlyRentAmount,
            DepositAmount = lease.DepositAmount, Notes = lease.Notes
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        var lease = await _leaseService.GetByIdAsync(Id);
        if (lease == null) return NotFound();

        lease.StartDate = Input.StartDate;
        lease.EndDate = Input.EndDate;
        lease.MonthlyRentAmount = Input.MonthlyRentAmount;
        lease.DepositAmount = Input.DepositAmount;
        lease.Notes = Input.Notes;

        var (success, error) = await _leaseService.UpdateAsync(lease);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error!);
            return Page();
        }

        TempData["SuccessMessage"] = "Lease updated successfully.";
        return RedirectToPage("Details", new { id = Id });
    }
}
