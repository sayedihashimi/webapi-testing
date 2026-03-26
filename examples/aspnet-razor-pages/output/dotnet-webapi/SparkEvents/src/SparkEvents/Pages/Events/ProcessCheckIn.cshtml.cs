using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public sealed class ProcessCheckInModel : PageModel
{
    private readonly ICheckInService _checkInService;
    private readonly IRegistrationService _registrationService;
    private readonly IEventService _eventService;

    public ProcessCheckInModel(
        ICheckInService checkInService,
        IRegistrationService registrationService,
        IEventService eventService)
    {
        _checkInService = checkInService;
        _registrationService = registrationService;
        _eventService = eventService;
    }

    public Event Event { get; set; } = default!;
    public Registration Registration { get; set; } = default!;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public sealed class InputModel
    {
        [Required]
        [MaxLength(200)]
        [Display(Name = "Checked In By")]
        public string CheckedInBy { get; set; } = string.Empty;

        [MaxLength(500)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int eventId, int registrationId)
    {
        var evt = await _eventService.GetByIdAsync(eventId);
        if (evt is null)
        {
            return NotFound();
        }

        var registration = await _registrationService.GetByIdWithDetailsAsync(registrationId);
        if (registration is null || registration.EventId != eventId)
        {
            return NotFound();
        }

        Event = evt;
        Registration = registration;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int eventId, int registrationId)
    {
        var evt = await _eventService.GetByIdAsync(eventId);
        if (evt is null)
        {
            return NotFound();
        }

        var registration = await _registrationService.GetByIdWithDetailsAsync(registrationId);
        if (registration is null || registration.EventId != eventId)
        {
            return NotFound();
        }

        Event = evt;
        Registration = registration;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            await _checkInService.ProcessCheckInAsync(registrationId, Input.CheckedInBy, Input.Notes);
            TempData["SuccessMessage"] = $"{registration.Attendee?.FullName} has been checked in successfully.";
            return RedirectToPage("CheckIn", new { eventId });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage("CheckIn", new { eventId });
        }
    }
}
