using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Leases;

public class EditModel(ILeaseService leaseService, IUnitService unitService, ITenantService tenantService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public Lease Lease { get; set; } = default!;
    public List<Unit> AvailableUnits { get; set; } = [];
    public List<Tenant> ActiveTenants { get; set; } = [];
    public bool IsActive { get; set; }

    public class InputModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Tenant")]
        public int TenantId { get; set; }

        [Required]
        [Display(Name = "Unit")]
        public int UnitId { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        public DateOnly StartDate { get; set; }

        [Required]
        [Display(Name = "End Date")]
        public DateOnly EndDate { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        [Display(Name = "Monthly Rent")]
        public decimal MonthlyRentAmount { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        [Display(Name = "Security Deposit")]
        public decimal DepositAmount { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var lease = await leaseService.GetLeaseWithDetailsAsync(id);

        if (lease is null)
        {
            return NotFound();
        }

        Lease = lease;
        IsActive = lease.Status == LeaseStatus.Active;

        Input = new InputModel
        {
            Id = lease.Id,
            TenantId = lease.TenantId,
            UnitId = lease.UnitId,
            StartDate = lease.StartDate,
            EndDate = lease.EndDate,
            MonthlyRentAmount = lease.MonthlyRentAmount,
            DepositAmount = lease.DepositAmount,
            Notes = lease.Notes
        };

        await LoadDropdownsAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var lease = await leaseService.GetLeaseByIdAsync(Input.Id);

        if (lease is null)
        {
            return NotFound();
        }

        IsActive = lease.Status == LeaseStatus.Active;

        if (IsActive)
        {
            // Active leases: only update Notes
            lease.Notes = Input.Notes;
        }
        else
        {
            if (!ModelState.IsValid)
            {
                Lease = (await leaseService.GetLeaseWithDetailsAsync(Input.Id))!;
                await LoadDropdownsAsync();
                return Page();
            }

            if (Input.EndDate <= Input.StartDate)
            {
                ModelState.AddModelError("Input.EndDate", "End date must be after the start date.");
                Lease = (await leaseService.GetLeaseWithDetailsAsync(Input.Id))!;
                await LoadDropdownsAsync();
                return Page();
            }

            lease.TenantId = Input.TenantId;
            lease.UnitId = Input.UnitId;
            lease.StartDate = Input.StartDate;
            lease.EndDate = Input.EndDate;
            lease.MonthlyRentAmount = Input.MonthlyRentAmount;
            lease.DepositAmount = Input.DepositAmount;
            lease.Notes = Input.Notes;
        }

        var (success, errorMessage) = await leaseService.UpdateLeaseAsync(lease);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage;
            Lease = (await leaseService.GetLeaseWithDetailsAsync(Input.Id))!;
            await LoadDropdownsAsync();
            return Page();
        }

        TempData["SuccessMessage"] = "Lease updated successfully.";
        return RedirectToPage("Details", new { id = Input.Id });
    }

    private async Task LoadDropdownsAsync()
    {
        AvailableUnits = await unitService.GetAvailableUnitsAsync();
        ActiveTenants = await tenantService.GetActiveTenantsAsync();
    }
}
