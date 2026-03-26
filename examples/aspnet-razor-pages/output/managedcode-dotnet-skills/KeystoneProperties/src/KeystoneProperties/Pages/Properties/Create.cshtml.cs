using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Properties;

public class CreateModel(IPropertyService propertyService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [MaxLength(2)]
        public string State { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string ZipCode { get; set; } = string.Empty;

        [Required]
        public PropertyType PropertyType { get; set; }

        [Range(1800, 2030)]
        public int? YearBuilt { get; set; }

        [Required]
        [Range(1, 1000)]
        public int TotalUnits { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

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

        await propertyService.CreatePropertyAsync(property);

        TempData["SuccessMessage"] = "Property created successfully.";
        return RedirectToPage("/Properties/Index");
    }
}
