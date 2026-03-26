using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Units;

public class DetailsModel(IUnitService unitService) : PageModel
{
    public Unit Unit { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var unit = await unitService.GetUnitWithDetailsAsync(id);
        if (unit is null)
        {
            return NotFound();
        }

        Unit = unit;
        return Page();
    }
}
