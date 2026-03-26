using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Properties;

public sealed class DeactivateModel(IPropertyService propertyService) : PageModel
{
    public Property Property { get; set; } = null!;
    public int OccupiedUnitCount { get; set; }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var property = await propertyService.GetByIdAsync(id, ct);
        if (property is null)
        {
            return NotFound();
        }

        Property = property;
        OccupiedUnitCount = await propertyService.GetOccupiedUnitCountAsync(id, ct);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken ct)
    {
        var (success, error) = await propertyService.DeactivateAsync(id, ct);

        if (!success)
        {
            TempData["ErrorMessage"] = error ?? "Failed to deactivate property.";

            var property = await propertyService.GetByIdAsync(id, ct);
            if (property is null)
            {
                return NotFound();
            }

            Property = property;
            OccupiedUnitCount = await propertyService.GetOccupiedUnitCountAsync(id, ct);
            return Page();
        }

        TempData["SuccessMessage"] = "Property was deactivated successfully.";
        return RedirectToPage("Index");
    }
}
