using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Models.Enums;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Leases;

public class CreateModel : PageModel
{
    private readonly ILeaseService _leaseService;
    private readonly ITenantService _tenantService;
    private readonly IUnitService _unitService;

    public CreateModel(ILeaseService leaseService, ITenantService tenantService, IUnitService unitService)
    {
        _leaseService = leaseService;
        _tenantService = tenantService;
        _unitService = unitService;
    }

    [BindProperty] public InputModel Input { get; set; } = new();
    public List<Tenant> TenantList { get; set; } = new();
    public List<Unit> AvailableUnits { get; set; } = new();

    public class InputModel
    {
        [Required, Display(Name = "Tenant")] public int TenantId { get; set; }
        [Required, Display(Name = "Unit")] public int UnitId { get; set; }
        [Required, Display(Name = "Start Date")] public DateOnly StartDate { get; set; }
        [Required, Display(Name = "End Date")] public DateOnly EndDate { get; set; }
        [Required, Range(0.01, double.MaxValue), Display(Name = "Monthly Rent"), DataType(DataType.Currency)]
        public decimal MonthlyRentAmount { get; set; }
        [Required, Range(0.01, double.MaxValue), Display(Name = "Deposit Amount"), DataType(DataType.Currency)]
        public decimal DepositAmount { get; set; }
        public LeaseStatus Status { get; set; } = LeaseStatus.Pending;
        [MaxLength(2000)] public string? Notes { get; set; }
    }

    public async Task OnGetAsync()
    {
        TenantList = await _tenantService.GetActiveTenantsAsync();
        AvailableUnits = await _unitService.GetAvailableUnitsAsync();
        Input.StartDate = DateOnly.FromDateTime(DateTime.Today);
        Input.EndDate = DateOnly.FromDateTime(DateTime.Today.AddYears(1));
    }

    public async Task<IActionResult> OnPostAsync()
    {
        TenantList = await _tenantService.GetActiveTenantsAsync();
        AvailableUnits = await _unitService.GetAvailableUnitsAsync();
        if (!ModelState.IsValid) return Page();

        var lease = new Lease
        {
            TenantId = Input.TenantId, UnitId = Input.UnitId,
            StartDate = Input.StartDate, EndDate = Input.EndDate,
            MonthlyRentAmount = Input.MonthlyRentAmount,
            DepositAmount = Input.DepositAmount,
            Status = Input.Status, Notes = Input.Notes
        };

        var (success, error) = await _leaseService.CreateAsync(lease);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error!);
            return Page();
        }

        TempData["SuccessMessage"] = "Lease created successfully.";
        return RedirectToPage("Details", new { id = lease.Id });
    }
}
