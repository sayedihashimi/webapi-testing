using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using KeystoneProperties.Models;
using KeystoneProperties.Services;

namespace KeystoneProperties.Pages.Units;

public class EditModel : PageModel
{
    private readonly IUnitService _unitService;
    private readonly IPropertyService _propertyService;

    public EditModel(IUnitService unitService, IPropertyService propertyService)
    {
        _unitService = unitService;
        _propertyService = propertyService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList PropertyList { get; set; } = default!;

    public class InputModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Property")]
        public int PropertyId { get; set; }

        [Required, MaxLength(20)]
        [Display(Name = "Unit Number")]
        public string UnitNumber { get; set; } = string.Empty;

        [Display(Name = "Floor")]
        public int? Floor { get; set; }

        [Required]
        [Range(0, 5, ErrorMessage = "Bedrooms must be between 0 and 5.")]
        [Display(Name = "Bedrooms")]
        public int Bedrooms { get; set; }

        [Required]
        [Range(0.5, 4.0, ErrorMessage = "Bathrooms must be between 0.5 and 4.")]
        [Display(Name = "Bathrooms")]
        public decimal Bathrooms { get; set; } = 1;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Square feet must be positive.")]
        [Display(Name = "Square Feet")]
        public int SquareFeet { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Monthly rent must be positive.")]
        [Display(Name = "Monthly Rent")]
        [DataType(DataType.Currency)]
        public decimal MonthlyRent { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Deposit amount must be positive.")]
        [Display(Name = "Deposit Amount")]
        [DataType(DataType.Currency)]
        public decimal DepositAmount { get; set; }

        [Required]
        [Display(Name = "Status")]
        public UnitStatus Status { get; set; }

        [MaxLength(1000)]
        [Display(Name = "Amenities")]
        public string? Amenities { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var unit = await _unitService.GetByIdAsync(id);
        if (unit is null)
        {
            return NotFound();
        }

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

        await LoadPropertyListAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadPropertyListAsync();
            return Page();
        }

        var unit = await _unitService.GetByIdAsync(Input.Id);
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

        await _unitService.UpdateAsync(unit);
        return RedirectToPage("Details", new { id = unit.Id });
    }

    private async Task LoadPropertyListAsync()
    {
        var properties = await _propertyService.GetAllActiveAsync();
        PropertyList = new SelectList(properties, "Id", "Name");
    }
}
