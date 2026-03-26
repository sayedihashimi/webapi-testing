using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Venues;

public sealed class DetailsModel : PageModel
{
    private readonly IVenueService _venueService;

    public DetailsModel(IVenueService venueService)
    {
        _venueService = venueService;
    }

    public Venue Venue { get; set; } = default!;
    public List<Event> UpcomingEvents { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var venue = await _venueService.GetByIdWithEventsAsync(id, ct);
        if (venue is null)
        {
            return NotFound();
        }

        Venue = venue;
        UpcomingEvents = venue.Events
            .Where(e => e.StartDate >= DateTime.UtcNow)
            .OrderBy(e => e.StartDate)
            .ToList();

        return Page();
    }
}
