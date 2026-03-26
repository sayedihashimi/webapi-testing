using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KeystoneProperties.Pages.Leases;

public class CreateModel : PageModel
{
    private readonly ILeaseService _leaseService;
    private readonly IUnitService _unitService;
    private readonly ITenantService _tenantService;

    public CreateModel(ILeaseService leaseService, IUnitService unitService, ITenantService tenantService)
    {
        _leaseService = leaseService;
        _unitService = unitService;
        _tenantService = tenantService;
    }

    [BindProperty]
    public LeaseInputModel Input { get; set; } = new();

    public SelectList TenantList { get; set; } = null!;
    public SelectList UnitList { get; set; } = null!;

    public class LeaseInputModel : IValidatableObject
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

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Monthly rent must be positive.")]
        [Display(Name = "Monthly Rent")]
        [DataType(DataType.Currency)]
        public decimal MonthlyRentAmount { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Deposit amount must be positive.")]
        [Display(Name = "Deposit Amount")]
        [DataType(DataType.Currency)]
        public decimal DepositAmount { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (EndDate <= StartDate)
            {
                yield return new ValidationResult(
                    "End date must be after the start date.",
                    new[] { nameof(EndDate) });
            }
        }
    }

    public async Task OnGetAsync()
    {
        await PopulateDropdowns();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdowns();
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

        var (success, error) = await _leaseService.CreateAsync(lease);
        if (!success)
        {
            ModelState.AddModelError(string.Empty, error ?? "Failed to create lease.");
            await PopulateDropdowns();
            return Page();
        }

        TempData["SuccessMessage"] = "Lease created successfully.";
        return RedirectToPage("Index");
    }

    private async Task PopulateDropdowns()
    {
        var tenants = await _tenantService.GetAllActiveAsync();
        TenantList = new SelectList(tenants, "Id", "FullName");

        var units = await _unitService.GetAvailableUnitsAsync();
        var unitItems = units.Select(u => new
        {
            u.Id,
            DisplayName = $"{u.Property.Name} - Unit {u.UnitNumber}"
        });
        UnitList = new SelectList(unitItems, "Id", "DisplayName");
    }
}
