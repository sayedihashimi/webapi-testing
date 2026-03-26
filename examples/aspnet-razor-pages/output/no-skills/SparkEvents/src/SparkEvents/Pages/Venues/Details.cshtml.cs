using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Venues;

public class DetailsModel : PageModel
{
    private readonly IVenueService _venueService;

    public DetailsModel(IVenueService venueService)
    {
        _venueService = venueService;
    }

    public Venue Venue { get; set; } = null!;
    public List<Event> UpcomingEvents { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var venue = await _venueService.GetVenueByIdAsync(id);
        if (venue == null) return NotFound();

        Venue = venue;
        UpcomingEvents = await _venueService.GetUpcomingEventsForVenueAsync(id);
        return Page();
    }
}
