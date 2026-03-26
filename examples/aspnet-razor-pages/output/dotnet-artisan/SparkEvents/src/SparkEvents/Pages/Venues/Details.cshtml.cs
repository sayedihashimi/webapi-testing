using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Pages.Venues;

public sealed class DetailsModel(SparkEventsDbContext db) : PageModel
{
    public Venue Venue { get; set; } = null!;
    public IReadOnlyList<Event> UpcomingEvents { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var venue = await db.Venues
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.Id == id);

        if (venue is null)
        {
            return NotFound();
        }

        Venue = venue;

        UpcomingEvents = await db.Events
            .AsNoTracking()
            .Include(e => e.EventCategory)
            .Where(e => e.VenueId == id && e.StartDate >= DateTime.UtcNow)
            .OrderBy(e => e.StartDate)
            .ToListAsync();

        return Page();
    }
}
