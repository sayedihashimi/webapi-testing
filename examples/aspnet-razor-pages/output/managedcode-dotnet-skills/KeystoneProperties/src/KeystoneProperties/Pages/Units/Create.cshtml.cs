using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Units;

public class CreateModel(IUnitService unitService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<Property> AllProperties { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(int? propertyId)
    {
        AllProperties = await unitService.GetAllPropertiesAsync();
        if (propertyId.HasValue)
        {
            Input.PropertyId = propertyId.Value;
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        AllProperties = await unitService.GetAllPropertiesAsync();
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var unit = new Unit
        {
            PropertyId = Input.PropertyId,
            UnitNumber = Input.UnitNumber,
            Floor = Input.Floor,
            Bedrooms = Input.Bedrooms,
            Bathrooms = Input.Bathrooms,
            SquareFeet = Input.SquareFeet,
            MonthlyRent = Input.MonthlyRent,
            DepositAmount = Input.DepositAmount,
            Amenities = Input.Amenities,
            Status = UnitStatus.Available
        };

        await unitService.CreateUnitAsync(unit);
        TempData["SuccessMessage"] = "Unit created successfully.";
        return RedirectToPage("/Units/Index");
    }

    public class InputModel
    {
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

        [MaxLength(1000)]
        public string? Amenities { get; set; }
    }
}
