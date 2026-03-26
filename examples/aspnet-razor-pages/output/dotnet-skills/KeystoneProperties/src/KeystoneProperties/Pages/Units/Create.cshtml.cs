using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using KeystoneProperties.Models;
using KeystoneProperties.Services;

namespace KeystoneProperties.Pages.Units;

public class CreateModel : PageModel
{
    private readonly IUnitService _unitService;
    private readonly IPropertyService _propertyService;

    public CreateModel(IUnitService unitService, IPropertyService propertyService)
    {
        _unitService = unitService;
        _propertyService = propertyService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public SelectList PropertyList { get; set; } = default!;

    public class InputModel
    {
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
        public UnitStatus Status { get; set; } = UnitStatus.Available;

        [MaxLength(1000)]
        [Display(Name = "Amenities")]
        public string? Amenities { get; set; }
    }

    public async Task OnGetAsync()
    {
        await LoadPropertyListAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadPropertyListAsync();
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
            Status = Input.Status,
            Amenities = Input.Amenities
        };

        await _unitService.CreateAsync(unit);
        return RedirectToPage("Index");
    }

    private async Task LoadPropertyListAsync()
    {
        var properties = await _propertyService.GetAllActiveAsync();
        PropertyList = new SelectList(properties, "Id", "Name");
    }
}
