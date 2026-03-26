using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KeystoneProperties.Pages.Units;

public sealed class CreateModel(IUnitService unitService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

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
        public UnitStatus Status { get; set; } = UnitStatus.Available;

        [MaxLength(1000)]
        [Display(Name = "Amenities")]
        public string? Amenities { get; set; }
    }

    public async Task OnGetAsync(CancellationToken ct)
    {
        await PopulateDropdownsAsync(ct);
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDropdownsAsync(ct);
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

        var created = await unitService.CreateAsync(unit, ct);

        TempData["SuccessMessage"] = $"Unit {created.UnitNumber} created successfully.";
        return RedirectToPage("Details", new { id = created.Id });
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
