using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Venues;

public class DetailsModel(IVenueService venueService, IEventService eventService) : PageModel
{
    public Venue? Venue { get; set; }

    public IReadOnlyList<Event> UpcomingEvents { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Venue = await venueService.GetByIdAsync(id);
        if (Venue is null)
        {
            return NotFound();
        }

        UpcomingEvents = await eventService.GetUpcomingByVenueAsync(id);

        return Page();
    }
}
