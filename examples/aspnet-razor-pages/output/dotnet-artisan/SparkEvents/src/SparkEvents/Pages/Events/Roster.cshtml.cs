using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public sealed class RosterModel(
    IEventService eventService,
    IRegistrationService registrationService) : PageModel
{
    public Event Event { get; set; } = null!;
    public PaginatedList<Registration> Registrations { get; set; } = null!;

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public async Task<IActionResult> OnGetAsync(int eventId, int pageNumber = 1)
    {
        var evt = await eventService.GetEventByIdAsync(eventId);
        if (evt is null)
        {
            return NotFound();
        }

        Event = evt;
        Registrations = await registrationService.GetEventRosterAsync(eventId, Search, pageNumber, 20);

        return Page();
    }
}
