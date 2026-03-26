using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Models.Enums;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Properties;

public class CreateModel : PageModel
{
    private readonly IPropertyService _propertyService;

    public CreateModel(IPropertyService propertyService)
    {
        _propertyService = propertyService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required, MaxLength(200)]
        [Display(Name = "Property Name")]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [Required, MaxLength(2)]
        public string State { get; set; } = string.Empty;

        [Required, MaxLength(10)]
        [Display(Name = "Zip Code")]
        public string ZipCode { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Property Type")]
        public PropertyType PropertyType { get; set; }

        [Display(Name = "Year Built")]
        public int? YearBuilt { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Total units must be positive")]
        [Display(Name = "Total Units")]
        public int TotalUnits { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var property = new Property
        {
            Name = Input.Name,
            Address = Input.Address,
            City = Input.City,
            State = Input.State,
            ZipCode = Input.ZipCode,
            PropertyType = Input.PropertyType,
            YearBuilt = Input.YearBuilt,
            TotalUnits = Input.TotalUnits,
            Description = Input.Description
        };

        await _propertyService.CreateAsync(property);
        TempData["SuccessMessage"] = $"Property '{property.Name}' created successfully.";
        return RedirectToPage("Details", new { id = property.Id });
    }
}
