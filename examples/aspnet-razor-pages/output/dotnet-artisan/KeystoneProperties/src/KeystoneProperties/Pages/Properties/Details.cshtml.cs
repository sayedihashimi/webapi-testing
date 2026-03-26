using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Properties;

public sealed class DetailsModel(IPropertyService propertyService) : PageModel
{
    public Property Property { get; set; } = default!;
    public int OccupiedCount { get; set; }
    public double OccupancyRate { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var property = await propertyService.GetWithUnitsAsync(id);
        if (property is null)
        {
            return NotFound();
        }

        Property = property;
        OccupiedCount = Property.Units.Count(u => u.Status == UnitStatus.Occupied);
        OccupancyRate = Property.TotalUnits > 0
            ? (double)OccupiedCount / Property.TotalUnits
            : 0;

        return Page();
    }
}
