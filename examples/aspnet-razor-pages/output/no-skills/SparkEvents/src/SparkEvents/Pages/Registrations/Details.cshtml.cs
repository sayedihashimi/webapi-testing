using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Registrations;

public class DetailsModel : PageModel
{
    private readonly IRegistrationService _registrationService;

    public DetailsModel(IRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    public Registration Registration { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var reg = await _registrationService.GetRegistrationByIdAsync(id);
        if (reg == null) return NotFound();
        Registration = reg;
        return Page();
    }
}
