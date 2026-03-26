using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public sealed class WaitlistModel : PageModel
{
    private readonly IRegistrationService _registrationService;
    private readonly IEventService _eventService;

    public WaitlistModel(IRegistrationService registrationService, IEventService eventService)
    {
        _registrationService = registrationService;
        _eventService = eventService;
    }

    public Event Event { get; set; } = default!;
    public List<Registration> WaitlistedRegistrations { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(int eventId)
    {
        var evt = await _eventService.GetByIdAsync(eventId);
        if (evt is null)
        {
            return NotFound();
        }

        Event = evt;
        WaitlistedRegistrations = await _registrationService.GetEventWaitlistAsync(eventId);
        return Page();
    }
}
