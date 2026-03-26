using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models.Enums;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Properties;

public class EditModel : PageModel
{
    private readonly IPropertyService _propertyService;

    public EditModel(IPropertyService propertyService)
    {
        _propertyService = propertyService;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

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
        [Required, Range(1, int.MaxValue)]
        [Display(Name = "Total Units")]
        public int TotalUnits { get; set; }
        [MaxLength(2000)]
        public string? Description { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var property = await _propertyService.GetByIdAsync(id);
        if (property == null) return NotFound();

        Id = id;
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

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var property = await _propertyService.GetByIdAsync(Id);
        if (property == null) return NotFound();

        property.Name = Input.Name;
        property.Address = Input.Address;
        property.City = Input.City;
        property.State = Input.State;
        property.ZipCode = Input.ZipCode;
        property.PropertyType = Input.PropertyType;
        property.YearBuilt = Input.YearBuilt;
        property.TotalUnits = Input.TotalUnits;
        property.Description = Input.Description;

        await _propertyService.UpdateAsync(property);
        TempData["SuccessMessage"] = "Property updated successfully.";
        return RedirectToPage("Details", new { id = Id });
    }
}
