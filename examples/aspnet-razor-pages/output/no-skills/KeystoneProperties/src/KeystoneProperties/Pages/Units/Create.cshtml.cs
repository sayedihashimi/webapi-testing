using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Data;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Pages.Units;

public class CreateModel : PageModel
{
    private readonly IUnitService _unitService;
    private readonly ApplicationDbContext _context;

    public CreateModel(IUnitService unitService, ApplicationDbContext context)
    {
        _unitService = unitService;
        _context = context;
    }

    [BindProperty] public InputModel Input { get; set; } = new();
    public List<Property> PropertyList { get; set; } = new();

    public class InputModel
    {
        [Required, Display(Name = "Property")]
        public int PropertyId { get; set; }
        [Required, MaxLength(20), Display(Name = "Unit Number")]
        public string UnitNumber { get; set; } = string.Empty;
        public int? Floor { get; set; }
        [Required, Range(0, 5)] public int Bedrooms { get; set; }
        [Required, Range(0.5, 4.0)] public decimal Bathrooms { get; set; } = 1.0m;
        [Required, Range(1, int.MaxValue), Display(Name = "Square Feet")]
        public int SquareFeet { get; set; }
        [Required, Range(0.01, double.MaxValue), Display(Name = "Monthly Rent"), DataType(DataType.Currency)]
        public decimal MonthlyRent { get; set; }
        [Required, Range(0.01, double.MaxValue), Display(Name = "Deposit Amount"), DataType(DataType.Currency)]
        public decimal DepositAmount { get; set; }
        [MaxLength(1000)] public string? Amenities { get; set; }
    }

    public async Task OnGetAsync()
    {
        PropertyList = await _context.Properties.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        PropertyList = await _context.Properties.Where(p => p.IsActive).OrderBy(p => p.Name).ToListAsync();
        if (!ModelState.IsValid) return Page();

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
            Amenities = Input.Amenities
        };

        await _unitService.CreateAsync(unit);
        TempData["SuccessMessage"] = $"Unit '{unit.UnitNumber}' created successfully.";
        return RedirectToPage("Details", new { id = unit.Id });
    }
}
