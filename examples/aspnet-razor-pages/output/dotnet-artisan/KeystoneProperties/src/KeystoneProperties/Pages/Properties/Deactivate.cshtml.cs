using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Properties;

public sealed class DeactivateModel(IPropertyService propertyService) : PageModel
{
    public Property Property { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var property = await propertyService.GetByIdAsync(id);
        if (property is null)
        {
            return NotFound();
        }

        Property = property;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var (success, error) = await propertyService.DeactivateAsync(id);

        if (!success)
        {
            TempData["ErrorMessage"] = error ?? "Failed to deactivate property.";
            return RedirectToPage("Deactivate", new { id });
        }

        TempData["SuccessMessage"] = "Property was deactivated successfully.";
        return RedirectToPage("Index");
    }
}
