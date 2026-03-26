using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Units;

public class EditModel(IUnitService unitService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<Property> AllProperties { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var unit = await unitService.GetUnitByIdAsync(id);
        if (unit is null)
        {
            return NotFound();
        }

        AllProperties = await unitService.GetAllPropertiesAsync();

        Input = new InputModel
        {
            Id = unit.Id,
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

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        AllProperties = await unitService.GetAllPropertiesAsync();
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var unit = await unitService.GetUnitByIdAsync(Input.Id);
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
        unit.UpdatedAt = DateTime.UtcNow;

        await unitService.UpdateUnitAsync(unit);
        TempData["SuccessMessage"] = "Unit updated successfully.";
        return RedirectToPage("/Units/Index");
    }

    public class InputModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Property")]
        public int PropertyId { get; set; }

        [Required]
        [MaxLength(20)]
        [Display(Name = "Unit Number")]
        public string UnitNumber { get; set; } = string.Empty;

        [Display(Name = "Floor")]
        public int? Floor { get; set; }

        [Required]
        [Range(0, 5)]
        public int Bedrooms { get; set; }

        [Required]
        [Range(typeof(decimal), "0.5", "4")]
        public decimal Bathrooms { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        [Display(Name = "Square Feet")]
        public int SquareFeet { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        [Display(Name = "Monthly Rent")]
        public decimal MonthlyRent { get; set; }

        [Required]
        [Range(0.01, double.MaxValue)]
        [Display(Name = "Deposit Amount")]
        public decimal DepositAmount { get; set; }

        public UnitStatus Status { get; set; }

        [MaxLength(1000)]
        public string? Amenities { get; set; }
    }
}
