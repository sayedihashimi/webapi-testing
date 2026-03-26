using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KeystoneProperties.Pages.Units;

public sealed class EditModel(IUnitService unitService, IPropertyService propertyService) : PageModel
{
    public SelectList Properties { get; set; } = null!;
    public int UnitId { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public sealed class InputModel
    {
        [Required]
        [Display(Name = "Property")]
        public int PropertyId { get; set; }

        [Required, MaxLength(20)]
        [Display(Name = "Unit Number")]
        public string UnitNumber { get; set; } = string.Empty;

        public int? Floor { get; set; }

        [Required, Range(0, 5)]
        public int Bedrooms { get; set; }

        [Required, Range(0.5, 4.0)]
        public decimal Bathrooms { get; set; } = 1;

        [Required, Range(1, int.MaxValue, ErrorMessage = "Square feet must be positive")]
        [Display(Name = "Square Feet")]
        public int SquareFeet { get; set; }

        [Required, Range(0.01, double.MaxValue, ErrorMessage = "Monthly rent must be positive")]
        [Display(Name = "Monthly Rent")]
        [DataType(DataType.Currency)]
        public decimal MonthlyRent { get; set; }

        [Required, Range(0.01, double.MaxValue, ErrorMessage = "Deposit amount must be positive")]
        [Display(Name = "Deposit Amount")]
        [DataType(DataType.Currency)]
        public decimal DepositAmount { get; set; }

        public UnitStatus Status { get; set; }

        [MaxLength(1000)]
        public string? Amenities { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var unit = await unitService.GetByIdAsync(id);
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

        await LoadPropertiesAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!ModelState.IsValid)
        {
            UnitId = id;
            await LoadPropertiesAsync();
            return Page();
        }

        var unit = new Unit
        {
            Id = id,
            PropertyId = Input.PropertyId,
            UnitNumber = Input.UnitNumber,
            Floor = Input.Floor,
            Bedrooms = Input.Bedrooms,
            Bathrooms = Input.Bathrooms,
            SquareFeet = Input.SquareFeet,
            MonthlyRent = Input.MonthlyRent,
            DepositAmount = Input.DepositAmount,
            Status = Input.Status,
            Amenities = Input.Amenities
        };

        await unitService.UpdateAsync(unit);

        TempData["SuccessMessage"] = $"Unit {unit.UnitNumber} updated successfully.";
        return RedirectToPage("Details", new { id });
    }

    private async Task LoadPropertiesAsync()
    {
        var properties = await propertyService.GetPropertiesAsync(null, null, true, 1, int.MaxValue);
        Properties = new SelectList(properties.Items, "Id", "Name");
    }
}
