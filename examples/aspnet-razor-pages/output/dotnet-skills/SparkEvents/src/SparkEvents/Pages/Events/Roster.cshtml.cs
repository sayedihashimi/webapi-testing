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

    public Event? Event { get; set; }
    public PaginatedList<Registration> Roster { get; set; } = null!;
    public string? Search { get; set; }

    public async Task<IActionResult> OnGetAsync(int eventId, string? search, int pageNumber = 1)
    {
        Event = await _eventService.GetEventByIdAsync(eventId);
        if (Event == null) return NotFound();
        Search = search;
        Roster = await _registrationService.GetEventRosterAsync(eventId, search, pageNumber);
        return Page();
    }
}
