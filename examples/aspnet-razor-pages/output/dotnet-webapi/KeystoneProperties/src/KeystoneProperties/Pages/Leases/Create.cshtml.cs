using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Leases;

public sealed class CreateModel(
    ILeaseService leaseService,
    IUnitService unitService,
    ITenantService tenantService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<Tenant> Tenants { get; set; } = [];
    public List<Unit> AvailableUnits { get; set; } = [];

    public sealed class InputModel
    {
        [Required(ErrorMessage = "Please select a tenant.")]
        [Display(Name = "Tenant")]
        public int TenantId { get; set; }

        [Required(ErrorMessage = "Please select a unit.")]
        [Display(Name = "Unit")]
        public int UnitId { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Required]
        [Display(Name = "End Date")]
        public DateOnly EndDate { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddYears(1));

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Monthly rent must be positive.")]
        [Display(Name = "Monthly Rent")]
        public decimal MonthlyRentAmount { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Deposit amount must be positive.")]
        [Display(Name = "Deposit Amount")]
        public decimal DepositAmount { get; set; }

        [Display(Name = "Status")]
        public LeaseStatus Status { get; set; } = LeaseStatus.Pending;

        [MaxLength(2000)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }
    }

    public async Task OnGetAsync(CancellationToken ct)
    {
        await PopulateDropdownsAsync(ct);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(ct);
            return Page();
        }

        var lease = new Lease
        {
            TenantId = Input.TenantId,
            UnitId = Input.UnitId,
            StartDate = Input.StartDate,
            EndDate = Input.EndDate,
            MonthlyRentAmount = Input.MonthlyRentAmount,
            DepositAmount = Input.DepositAmount,
            Status = Input.Status,
            Notes = Input.Notes
        };

        var (created, error) = await leaseService.CreateAsync(lease, ct);
        if (error is not null)
        {
            ModelState.AddModelError(string.Empty, error);
            await PopulateDropdownsAsync(ct);
            return Page();
        }

        TempData["SuccessMessage"] = "Lease created successfully.";
        return RedirectToPage("Details", new { id = created!.Id });
    }

    private async Task PopulateDropdownsAsync(CancellationToken ct)
    {
        Tenants = await tenantService.GetActiveTenantsAsync(ct);
        AvailableUnits = await unitService.GetAvailableUnitsAsync(ct);
    }
}
