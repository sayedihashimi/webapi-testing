using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Properties;

public class EditModel(IPropertyService propertyService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        public int Id { get; set; }

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

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var property = await propertyService.GetPropertyByIdAsync(id);
        if (property is null)
        {
            return NotFound();
        }

        Input = new InputModel
        {
            Id = property.Id,
            Name = property.Name,
            Address = property.Address,
            City = property.City,
            State = property.State,
            ZipCode = property.ZipCode,
            PropertyType = property.PropertyType,
            YearBuilt = property.YearBuilt,
            TotalUnits = property.TotalUnits,
            Description = property.Description
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var property = await propertyService.GetPropertyByIdAsync(Input.Id);
        if (property is null)
        {
            return NotFound();
        }

        property.Name = Input.Name;
        property.Address = Input.Address;
        property.City = Input.City;
        property.State = Input.State;
        property.ZipCode = Input.ZipCode;
        property.PropertyType = Input.PropertyType;
        property.YearBuilt = Input.YearBuilt;
        property.TotalUnits = Input.TotalUnits;
        property.Description = Input.Description;

        await propertyService.UpdatePropertyAsync(property);

        TempData["SuccessMessage"] = "Property updated successfully.";
        return RedirectToPage("/Properties/Index");
    }
}
