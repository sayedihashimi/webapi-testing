using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public class WaitlistModel(IEventService eventService, IRegistrationService registrationService) : PageModel
{
    public Event Event { get; set; } = null!;

    public List<Registration> WaitlistedRegistrations { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(int eventId)
    {
        var ev = await eventService.GetByIdAsync(eventId);
        if (ev is null)
        {
            return NotFound();
        }

        Event = ev;
        WaitlistedRegistrations = await registrationService.GetEventWaitlistAsync(eventId);

        return Page();
    }
}
