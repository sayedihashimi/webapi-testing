using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KeystoneProperties.Pages.Units;

public sealed class EditModel(IUnitService unitService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public int UnitId { get; set; }
    public List<SelectListItem> PropertyList { get; set; } = [];
    public List<SelectListItem> StatusList { get; set; } = [];

    public sealed class InputModel
    {
        [Required(ErrorMessage = "Property is required.")]
        [Display(Name = "Property")]
        public int PropertyId { get; set; }

        [Required, MaxLength(20)]
        [Display(Name = "Unit Number")]
        public string UnitNumber { get; set; } = string.Empty;

        [Display(Name = "Floor")]
        public int? Floor { get; set; }

        [Required, Range(0, 5)]
        [Display(Name = "Bedrooms")]
        public int Bedrooms { get; set; }

        [Required, Range(0.5, 4.0)]
        [Display(Name = "Bathrooms")]
        public decimal Bathrooms { get; set; } = 1;

        [Required, Range(1, int.MaxValue, ErrorMessage = "Square feet must be positive.")]
        [Display(Name = "Square Feet")]
        public int SquareFeet { get; set; }

        [Required, Range(0.01, double.MaxValue, ErrorMessage = "Monthly rent must be positive.")]
        [Display(Name = "Monthly Rent")]
        public decimal MonthlyRent { get; set; }

        [Required, Range(0.01, double.MaxValue, ErrorMessage = "Deposit amount must be positive.")]
        [Display(Name = "Deposit Amount")]
        public decimal DepositAmount { get; set; }

        [Display(Name = "Status")]
        public UnitStatus Status { get; set; }

        [MaxLength(1000)]
        [Display(Name = "Amenities")]
        public string? Amenities { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var unit = await unitService.GetByIdAsync(id, ct);

        if (unit is null)
        {
            return NotFound();
        }

        UnitId = id;

        Input = new InputModel
        {
            PropertyId = unit.PropertyId,
            UnitNumber = unit.UnitNumber,
            Floor = unit.Floor,
            Bedrooms = unit.Bedrooms,
            Bathrooms = unit.Bathrooms,
            SquareFeet = unit.SquareFeet,
            MonthlyRent = unit.MonthlyRent,
            DepositAmount = unit.DepositAmount,
            Status = unit.Status,
            Amenities = unit.Amenities
        };

        await PopulateDropdownsAsync(ct);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            UnitId = id;
            await PopulateDropdownsAsync(ct);
            return Page();
        }

        var unit = await unitService.GetByIdAsync(id, ct);

        if (unit is null)
        {
            return NotFound();
        }

        unit.PropertyId = Input.PropertyId;
        unit.UnitNumber = Input.UnitNumber;
        unit.Floor = Input.Floor;
        unit.Bedrooms = Input.Bedrooms;
        unit.Bathrooms = Input.Bathrooms;
        unit.SquareFeet = Input.SquareFeet;
        unit.MonthlyRent = Input.MonthlyRent;
        unit.DepositAmount = Input.DepositAmount;
        unit.Status = Input.Status;
        unit.Amenities = Input.Amenities;

        await unitService.UpdateAsync(unit, ct);

        TempData["SuccessMessage"] = $"Unit {unit.UnitNumber} updated successfully.";
        return RedirectToPage("Details", new { id });
    }

    private async Task PopulateDropdownsAsync(CancellationToken ct)
    {
        var properties = await unitService.GetAllPropertiesAsync(ct);
        PropertyList = properties
            .Select(p => new SelectListItem(p.Name, p.Id.ToString()))
            .ToList();

        StatusList = Enum.GetValues<UnitStatus>()
            .Select(s => new SelectListItem(s.ToString(), s.ToString()))
            .ToList();
    }
}
