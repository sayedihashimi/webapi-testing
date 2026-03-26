using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Registrations;

public class CancelModel(IRegistrationService registrationService) : PageModel
{
    public Registration Registration { get; set; } = null!;

    public bool WithinCutoff { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var registration = await registrationService.GetByIdAsync(id);
        if (registration is null)
        {
            return NotFound();
        }

        Registration = registration;
        WithinCutoff = (registration.Event.StartDate - DateTime.UtcNow) <= TimeSpan.FromHours(24);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var registration = await registrationService.GetByIdAsync(id);
        if (registration is null)
        {
            return NotFound();
        }

        Registration = registration;
        WithinCutoff = (registration.Event.StartDate - DateTime.UtcNow) <= TimeSpan.FromHours(24);

        if (WithinCutoff)
        {
            TempData["ErrorMessage"] = "Cannot cancel within 24 hours of the event.";
            return Page();
        }

        try
        {
            await registrationService.CancelAsync(id, Input.CancellationReason);
            TempData["SuccessMessage"] = "Registration cancelled successfully.";
            return RedirectToPage("Details", new { id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return Page();
        }
    }

    public class InputModel
    {
        [MaxLength(1000)]
        [Display(Name = "Cancellation Reason")]
        public string? CancellationReason { get; set; }
    }
}
