using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Registrations;

public class DetailsModel(IRegistrationService registrationService) : PageModel
{
    public Registration Registration { get; set; } = null!;

    public bool CanCancel { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var registration = await registrationService.GetByIdAsync(id);
        if (registration is null)
        {
            return NotFound();
        }

        Registration = registration;
        CanCancel = registration.Status == RegistrationStatus.Confirmed
            && (registration.Event.StartDate - DateTime.UtcNow) > TimeSpan.FromHours(24);

        return Page();
    }
}
