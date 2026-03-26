using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public class WaitlistModel : PageModel
{
    private readonly IEventService _eventService;
    private readonly IRegistrationService _registrationService;

    public WaitlistModel(IEventService eventService, IRegistrationService registrationService)
    {
        _eventService = eventService;
        _registrationService = registrationService;
    }

    public Event Event { get; set; } = null!;
    public List<Registration> WaitlistRegistrations { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int eventId)
    {
        var evt = await _eventService.GetEventByIdAsync(eventId);
        if (evt == null) return NotFound();
        Event = evt;
        WaitlistRegistrations = await _registrationService.GetEventWaitlistAsync(eventId);
        return Page();
    }
}
