using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public class RosterModel : PageModel
{
    private readonly IEventService _eventService;
    private readonly IRegistrationService _registrationService;

    public RosterModel(IEventService eventService, IRegistrationService registrationService)
    {
        _eventService = eventService;
        _registrationService = registrationService;
    }

    public Event Event { get; set; } = null!;
    public PaginatedList<Registration> Registrations { get; set; } = null!;
    public string? Search { get; set; }

    public async Task<IActionResult> OnGetAsync(int eventId, string? search, int pageNumber = 1)
    {
        var evt = await _eventService.GetEventByIdAsync(eventId);
        if (evt == null) return NotFound();
        Event = evt;
        Search = search;
        Registrations = await _registrationService.GetEventRosterAsync(eventId, search, pageNumber, 10);
        return Page();
    }
}
