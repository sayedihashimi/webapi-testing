using KeystoneProperties.Models;
using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages.Units;

public class DetailsModel : PageModel
{
    private readonly IUnitService _unitService;

    public DetailsModel(IUnitService unitService)
    {
        _unitService = unitService;
    }

    public Unit Unit { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var unit = await _unitService.GetWithDetailsAsync(id);
        if (unit == null) return NotFound();
        Unit = unit;
        return Page();
    }
}
