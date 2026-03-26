using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public sealed class RosterModel : PageModel
{
    private readonly IRegistrationService _registrationService;
    private readonly IEventService _eventService;

    public RosterModel(IRegistrationService registrationService, IEventService eventService)
    {
        _registrationService = registrationService;
        _eventService = eventService;
    }

    public Event Event { get; set; } = default!;
    public PaginatedList<Registration> Registrations { get; set; } = default!;

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 10;

    public async Task<IActionResult> OnGetAsync(int eventId)
    {
        var evt = await _eventService.GetByIdAsync(eventId);
        if (evt is null)
        {
            return NotFound();
        }

        Event = evt;
        Registrations = await _registrationService.GetEventRosterAsync(eventId, Search, PageNumber, PageSize);
        return Page();
    }
}
