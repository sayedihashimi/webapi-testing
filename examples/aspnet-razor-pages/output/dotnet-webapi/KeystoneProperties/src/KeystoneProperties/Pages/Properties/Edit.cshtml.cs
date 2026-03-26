using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KeystoneProperties.Pages.Properties;

public sealed class EditModel(IPropertyService propertyService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public int PropertyId { get; set; }

    public SelectList PropertyTypeOptions { get; } = new(
        Enum.GetValues<PropertyType>().Select(t => new { Value = t.ToString(), Text = t.ToString() }),
        "Value", "Text");

    public sealed class InputModel
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
        [Display(Name = "ZIP Code")]
        public string ZipCode { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Property Type")]
        public PropertyType PropertyType { get; set; }

        [Display(Name = "Year Built")]
        [Range(1800, 2100)]
        public int? YearBuilt { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Total units must be at least 1.")]
        [Display(Name = "Total Units")]
        public int TotalUnits { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var property = await propertyService.GetByIdAsync(id, ct);
        if (property is null)
        {
            return NotFound();
        }

        PropertyId = id;
        Input = new InputModel
        {
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

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            PropertyId = id;
            return Page();
        }

        var property = await propertyService.GetByIdAsync(id, ct);
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

        await propertyService.UpdateAsync(property, ct);
        TempData["SuccessMessage"] = $"Property \"{property.Name}\" was updated successfully.";
        return RedirectToPage("Details", new { id });
    }
}
