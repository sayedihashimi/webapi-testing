using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Registrations;

public sealed class DetailsModel : PageModel
{
    private readonly IRegistrationService _registrationService;

    public DetailsModel(IRegistrationService registrationService)
    {
        _registrationService = registrationService;
    }

    public Registration Registration { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var registration = await _registrationService.GetByIdWithDetailsAsync(id);
        if (registration is null)
        {
            return NotFound();
        }

        Registration = registration;
        return Page();
    }
}
