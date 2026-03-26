using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Properties;

public sealed class EditModel(IPropertyService propertyService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public int PropertyId { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var property = await propertyService.GetByIdAsync(id);
        if (property is null)
        {
            return NotFound();
        }

        PropertyId = id;
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
            Description = property.Description,
            IsActive = property.IsActive
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            PropertyId = Input.Id;
            return Page();
        }

        var property = await propertyService.GetByIdAsync(Input.Id);
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
        property.IsActive = Input.IsActive;

        await propertyService.UpdateAsync(property);

        TempData["SuccessMessage"] = $"Property \"{property.Name}\" was updated successfully.";
        return RedirectToPage("Index");
    }

    public sealed class InputModel
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
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

        [Display(Name = "Property Type")]
        public PropertyType PropertyType { get; set; }

        [Display(Name = "Year Built")]
        public int? YearBuilt { get; set; }

        [Required, Range(1, int.MaxValue, ErrorMessage = "Total units must be positive")]
        [Display(Name = "Total Units")]
        public int TotalUnits { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; }
    }
}
