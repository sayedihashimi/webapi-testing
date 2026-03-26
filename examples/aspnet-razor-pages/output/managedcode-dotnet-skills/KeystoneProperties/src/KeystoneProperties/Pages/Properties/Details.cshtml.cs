using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Properties;

public class DetailsModel(IPropertyService propertyService) : PageModel
{
    public Property Property { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var property = await propertyService.GetPropertyWithUnitsAsync(id);
        if (property is null)
        {
            return NotFound();
        }

        Property = property;
        return Page();
    }
}
