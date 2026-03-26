using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using KeystoneProperties.Models;
using KeystoneProperties.Services;

namespace KeystoneProperties.Pages.Units;

public class DetailsModel : PageModel
{
    private readonly IUnitService _unitService;

    public DetailsModel(IUnitService unitService)
    {
        _unitService = unitService;
    }

    public Unit Unit { get; set; } = default!;
    public Lease? CurrentLease { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var unit = await _unitService.GetWithDetailsAsync(id);
        if (unit is null)
        {
            return NotFound();
        }

        Unit = unit;
        CurrentLease = unit.Leases
            .FirstOrDefault(l => l.Status == LeaseStatus.Active);

        return Page();
    }
}
