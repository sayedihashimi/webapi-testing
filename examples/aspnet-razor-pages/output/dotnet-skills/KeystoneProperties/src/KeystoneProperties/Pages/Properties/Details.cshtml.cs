using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using KeystoneProperties.Models;
using KeystoneProperties.Services;

namespace KeystoneProperties.Pages.Properties;

public class DetailsModel : PageModel
{
    private readonly IPropertyService _propertyService;

    public DetailsModel(IPropertyService propertyService)
    {
        _propertyService = propertyService;
    }

    public Property Property { get; set; } = default!;
    public int TotalUnits { get; set; }
    public int OccupiedUnits { get; set; }
    public int AvailableUnits { get; set; }
    public int MaintenanceUnits { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var property = await _propertyService.GetWithUnitsAsync(id);
        if (property is null)
        {
            return RedirectToPage("Index");
        }

        Property = property;
        TotalUnits = property.Units.Count;
        OccupiedUnits = property.Units.Count(u => u.Status == UnitStatus.Occupied);
        AvailableUnits = property.Units.Count(u => u.Status == UnitStatus.Available);
        MaintenanceUnits = property.Units.Count(u => u.Status == UnitStatus.Maintenance);

        return Page();
    }
}
