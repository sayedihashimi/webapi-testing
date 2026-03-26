using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Inspections;

public class DetailsModel : PageModel
{
    private readonly IInspectionService _inspectionService;

    public DetailsModel(IInspectionService inspectionService)
    {
        _inspectionService = inspectionService;
    }

    public Inspection Inspection { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var inspection = await _inspectionService.GetWithDetailsAsync(id);
        if (inspection is null)
        {
            return NotFound();
        }

        Inspection = inspection;
        return Page();
    }
}
