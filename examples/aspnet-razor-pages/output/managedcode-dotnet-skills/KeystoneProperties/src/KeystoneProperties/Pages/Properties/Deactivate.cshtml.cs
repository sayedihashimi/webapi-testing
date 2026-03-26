using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Properties;

public class DeactivateModel(IPropertyService propertyService) : PageModel
{
    public Property Property { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var property = await propertyService.GetPropertyByIdAsync(id);
        if (property is null)
        {
            return NotFound();
        }

        Property = property;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var (success, errorMessage) = await propertyService.DeactivatePropertyAsync(id);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage ?? "Failed to deactivate property.";
            return RedirectToPage("/Properties/Deactivate", new { id });
        }

        TempData["SuccessMessage"] = "Property deactivated successfully.";
        return RedirectToPage("/Properties/Index");
    }
}
