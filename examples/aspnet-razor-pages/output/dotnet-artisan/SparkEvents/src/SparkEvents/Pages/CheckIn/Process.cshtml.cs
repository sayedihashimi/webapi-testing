using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.CheckIn;

public sealed class ProcessModel(
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
        if (!await LoadDataAsync(eventId, registrationId))
        {
            return NotFound();
        }

        Input.EventId = eventId;
        Input.RegistrationId = registrationId;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!await LoadDataAsync(Input.EventId, Input.RegistrationId))
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            await checkInService.ProcessCheckInAsync(Input.RegistrationId, Input.CheckedInBy, Input.Notes);

            TempData["SuccessMessage"] = $"{Registration.Attendee.FullName} has been checked in successfully.";
            return RedirectToPage("/CheckIn/Index", new { eventId = Input.EventId });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return Page();
        }
    }

    private async Task<bool> LoadDataAsync(int eventId, int registrationId)
    {
        var evt = await eventService.GetEventByIdAsync(eventId);
        if (evt is null)
        {
            return false;
        }

        var registration = await registrationService.GetRegistrationByIdAsync(registrationId);
        if (registration is null || registration.EventId != eventId)
        {
            return false;
        }

        Event = evt;
        Registration = registration;
        return true;
    }

    public sealed class InputModel
    {
        public int EventId { get; set; }
        public int RegistrationId { get; set; }

        [Required, MaxLength(200)]
        [Display(Name = "Staff Name")]
        public string CheckedInBy { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Notes { get; set; }
    }
}
