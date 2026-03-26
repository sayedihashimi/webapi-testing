using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Registrations;

public sealed class CancelModel(IRegistrationService registrationService) : PageModel
{
    public Registration Registration { get; set; } = null!;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var registration = await registrationService.GetRegistrationByIdAsync(id);
        if (registration is null)
        {
            return NotFound();
        }

        Registration = registration;
        Input.RegistrationId = id;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var registration = await registrationService.GetRegistrationByIdAsync(Input.RegistrationId);
        if (registration is null)
        {
            return NotFound();
        }

        Registration = registration;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            await registrationService.CancelRegistrationAsync(Input.RegistrationId, Input.Reason);
            TempData["SuccessMessage"] = $"Registration {registration.ConfirmationNumber} has been cancelled.";
            return RedirectToPage("Details", new { id = Input.RegistrationId });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return Page();
        }
    }

    public sealed class InputModel
    {
        public int RegistrationId { get; set; }

        [MaxLength(1000)]
        [Display(Name = "Cancellation Reason")]
        public string? Reason { get; set; }
    }
}
