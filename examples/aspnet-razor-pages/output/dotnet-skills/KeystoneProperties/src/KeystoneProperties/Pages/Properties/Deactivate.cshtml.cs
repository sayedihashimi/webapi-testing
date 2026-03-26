using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using KeystoneProperties.Models;
using KeystoneProperties.Services;

namespace KeystoneProperties.Pages.Properties;

public class DeactivateModel : PageModel
{
    private readonly IPropertyService _propertyService;

    public DeactivateModel(IPropertyService propertyService)
    {
        _propertyService = propertyService;
    }

    public Property Property { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var property = await _propertyService.GetByIdAsync(id);
        if (property is null)
        {
            return RedirectToPage("Index");
        }

        Property = property;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var property = await _propertyService.GetByIdAsync(id);
        if (property is null)
        {
            return RedirectToPage("Index");
        }

        var (success, error) = await _propertyService.DeactivateAsync(id);
        if (!success)
        {
            TempData["Error"] = error ?? "Failed to deactivate property.";
            return RedirectToPage("Deactivate", new { id });
        }

        TempData["Success"] = "Property deactivated successfully.";
        return RedirectToPage("Index");
    }
}
