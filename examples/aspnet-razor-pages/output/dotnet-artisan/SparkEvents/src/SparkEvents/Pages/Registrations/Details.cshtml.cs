using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Registrations;

public sealed class DetailsModel(IRegistrationService registrationService) : PageModel
{
    public Registration Registration { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var registration = await registrationService.GetRegistrationByIdAsync(id);
        if (registration is null)
        {
            return NotFound();
        }

        Registration = registration;
        return Page();
    }

    public bool CanCancel => Registration.Status is RegistrationStatus.Confirmed or RegistrationStatus.Waitlisted;
}
