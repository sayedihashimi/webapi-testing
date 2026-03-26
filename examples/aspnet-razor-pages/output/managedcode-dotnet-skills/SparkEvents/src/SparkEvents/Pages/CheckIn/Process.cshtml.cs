using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.CheckIn;

public class ProcessModel(
    IEventService eventService,
    IRegistrationService registrationService,
    ICheckInService checkInService) : PageModel
{
    public Event Event { get; set; } = null!;

    public Registration Registration { get; set; } = null!;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int eventId, int registrationId)
    {
        var ev = await eventService.GetByIdAsync(eventId);
        if (ev is null)
        {
            return NotFound();
        }

        var registration = await registrationService.GetByIdAsync(registrationId);
        if (registration is null || registration.EventId != eventId)
        {
            return NotFound();
        }

        Event = ev;
        Registration = registration;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int eventId, int registrationId)
    {
        var ev = await eventService.GetByIdAsync(eventId);
        if (ev is null)
        {
            return NotFound();
        }

        var registration = await registrationService.GetByIdAsync(registrationId);
        if (registration is null || registration.EventId != eventId)
        {
            return NotFound();
        }

        Event = ev;
        Registration = registration;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            await checkInService.ProcessCheckInAsync(registrationId, Input.CheckedInBy, Input.Notes);
            TempData["SuccessMessage"] = $"Successfully checked in {registration.Attendee.FullName}.";
            return RedirectToPage("Index", new { eventId });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return Page();
        }
    }

    public class InputModel
    {
        [Required]
        [MaxLength(200)]
        [Display(Name = "Checked In By")]
        public string CheckedInBy { get; set; } = string.Empty;

        [MaxLength(500)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }
    }
}
