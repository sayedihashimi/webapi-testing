using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Inspections;

public sealed class DetailsModel(IInspectionService inspectionService) : PageModel
{
    public Inspection Inspection { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var inspection = await inspectionService.GetByIdAsync(id);
        if (inspection is null)
        {
            return NotFound();
        }

        Inspection = inspection;
        return Page();
    }
}
