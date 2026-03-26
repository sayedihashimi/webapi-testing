using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KeystoneProperties.Pages.Leases;

public sealed class CreateModel(
    ILeaseService leaseService,
    ITenantService tenantService,
    IUnitService unitService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<SelectListItem> TenantOptions { get; set; } = [];
    public List<SelectListItem> UnitOptions { get; set; } = [];

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

        var (success, error) = await leaseService.CreateAsync(lease);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, error!);
            await LoadDropdownsAsync();
            return Page();
        }

        TempData["SuccessMessage"] = "Lease was created successfully.";
        return RedirectToPage("Index");
    }

    private async Task LoadDropdownsAsync()
    {
        var tenants = await tenantService.GetActiveTenantsAsync();
        TenantOptions = tenants
            .Select(t => new SelectListItem(t.FullName, t.Id.ToString()))
            .ToList();

        var units = await unitService.GetAvailableUnitsAsync();
        UnitOptions = units
            .Select(u => new SelectListItem($"{u.Property.Name} — {u.UnitNumber}", u.Id.ToString()))
            .ToList();
    }

    public sealed class InputModel
    {
        [Required]
        [Display(Name = "Tenant")]
        public int TenantId { get; set; }

        [Required]
        [Display(Name = "Unit")]
        public int UnitId { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        public DateOnly StartDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

        [Required]
        [Display(Name = "End Date")]
        public DateOnly EndDate { get; set; } = DateOnly.FromDateTime(DateTime.Today.AddYears(1));

        [Required, Range(0.01, double.MaxValue, ErrorMessage = "Monthly rent must be positive")]
        [Display(Name = "Monthly Rent")]
        [DataType(DataType.Currency)]
        public decimal MonthlyRentAmount { get; set; }

        [Required, Range(0.01, double.MaxValue, ErrorMessage = "Deposit must be positive")]
        [Display(Name = "Deposit Amount")]
        [DataType(DataType.Currency)]
        public decimal DepositAmount { get; set; }

        [Display(Name = "Status")]
        public LeaseStatus Status { get; set; } = LeaseStatus.Pending;

        [MaxLength(2000)]
        public string? Notes { get; set; }
    }
}
