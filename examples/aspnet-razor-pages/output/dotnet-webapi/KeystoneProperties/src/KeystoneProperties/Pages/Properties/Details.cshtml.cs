using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Properties;

public sealed class DetailsModel(IPropertyService propertyService) : PageModel
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
}
