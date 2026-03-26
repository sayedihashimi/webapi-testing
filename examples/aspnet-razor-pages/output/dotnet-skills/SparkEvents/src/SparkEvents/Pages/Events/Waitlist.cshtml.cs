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

    public Event? Event { get; set; }
    public List<Registration> Waitlist { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int eventId)
    {
        Event = await _eventService.GetEventByIdAsync(eventId);
        if (Event == null) return NotFound();
        Waitlist = await _registrationService.GetEventWaitlistAsync(eventId);
        return Page();
    }
}
