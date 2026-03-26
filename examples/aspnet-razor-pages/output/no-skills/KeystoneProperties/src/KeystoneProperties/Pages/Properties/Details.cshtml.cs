using KeystoneProperties.Models;
using KeystoneProperties.Models.Enums;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Properties;

public class DetailsModel : PageModel
{
    private readonly IPropertyService _propertyService;

    public DetailsModel(IPropertyService propertyService)
    {
        _propertyService = propertyService;
    }

    public Property Property { get; set; } = null!;
    public int OccupiedCount { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var property = await _propertyService.GetWithUnitsAsync(id);
        if (property == null) return NotFound();
        Property = property;
        OccupiedCount = property.Units.Count(u => u.Status == UnitStatus.Occupied);
        return Page();
    }
}
