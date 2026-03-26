using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Leases;

public class CreateModel(ILeaseService leaseService, IUnitService unitService, ITenantService tenantService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<Unit> AvailableUnits { get; set; } = [];
    public List<Tenant> ActiveTenants { get; set; } = [];

    public class InputModel
    {
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

    public async Task OnGetAsync()
    {
        await LoadDropdownsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadDropdownsAsync();
            return Page();
        }

        if (Input.EndDate <= Input.StartDate)
        {
            ModelState.AddModelError("Input.EndDate", "End date must be after the start date.");
            await LoadDropdownsAsync();
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
            Notes = Input.Notes,
            Status = LeaseStatus.Pending,
            DepositStatus = DepositStatus.Held
        };

        var (success, errorMessage) = await leaseService.CreateLeaseAsync(lease);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage;
            await LoadDropdownsAsync();
            return Page();
        }

        TempData["SuccessMessage"] = "Lease created successfully.";
        return RedirectToPage("Index");
    }

    private async Task LoadDropdownsAsync()
    {
        AvailableUnits = await unitService.GetAvailableUnitsAsync();
        ActiveTenants = await tenantService.GetActiveTenantsAsync();
    }
}
