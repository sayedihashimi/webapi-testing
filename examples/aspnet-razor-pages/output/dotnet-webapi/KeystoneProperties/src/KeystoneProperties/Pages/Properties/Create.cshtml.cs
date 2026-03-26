using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KeystoneProperties.Pages.Properties;

public sealed class CreateModel(IPropertyService propertyService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

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

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
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

        var created = await propertyService.CreateAsync(property, ct);
        TempData["SuccessMessage"] = $"Property \"{created.Name}\" was created successfully.";
        return RedirectToPage("Details", new { id = created.Id });
    }
}
