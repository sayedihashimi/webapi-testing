using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models.Enums;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Units;

public class EditModel : PageModel
{
    private readonly IUnitService _unitService;

    public EditModel(IUnitService unitService)
    {
        _unitService = unitService;
    }

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    public string UnitNumber { get; set; } = string.Empty;
    [BindProperty] public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required, MaxLength(20), Display(Name = "Unit Number")]
        public string UnitNumber { get; set; } = string.Empty;
        public int? Floor { get; set; }
        [Required, Range(0, 5)] public int Bedrooms { get; set; }
        [Required, Range(0.5, 4.0)] public decimal Bathrooms { get; set; }
        [Required, Range(1, int.MaxValue), Display(Name = "Square Feet")]
        public int SquareFeet { get; set; }
        [Required, Range(0.01, double.MaxValue), Display(Name = "Monthly Rent")]
        public decimal MonthlyRent { get; set; }
        [Required, Range(0.01, double.MaxValue), Display(Name = "Deposit Amount")]
        public decimal DepositAmount { get; set; }
        public UnitStatus Status { get; set; }
        [MaxLength(1000)] public string? Amenities { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var unit = await _unitService.GetByIdAsync(id);
        if (unit == null) return NotFound();
        Id = id;
        UnitNumber = unit.UnitNumber;
        Input = new InputModel
        {
            UnitNumber = unit.UnitNumber, Floor = unit.Floor, Bedrooms = unit.Bedrooms,
            Bathrooms = unit.Bathrooms, SquareFeet = unit.SquareFeet,
            MonthlyRent = unit.MonthlyRent, DepositAmount = unit.DepositAmount,
            Status = unit.Status, Amenities = unit.Amenities
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) { UnitNumber = Input.UnitNumber; return Page(); }

        var unit = await _unitService.GetByIdAsync(Id);
        if (unit == null) return NotFound();

        unit.UnitNumber = Input.UnitNumber;
        unit.Floor = Input.Floor;
        unit.Bedrooms = Input.Bedrooms;
        unit.Bathrooms = Input.Bathrooms;
        unit.SquareFeet = Input.SquareFeet;
        unit.MonthlyRent = Input.MonthlyRent;
        unit.DepositAmount = Input.DepositAmount;
        unit.Status = Input.Status;
        unit.Amenities = Input.Amenities;

        await _unitService.UpdateAsync(unit);
        TempData["SuccessMessage"] = "Unit updated successfully.";
        return RedirectToPage("Details", new { id = Id });
    }
}
