using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Pages.Venues;

public sealed class IndexModel(SparkEventsDbContext db) : PageModel
{
    public IReadOnlyList<Venue> Venues { get; set; } = [];

    public async Task OnGetAsync()
    {
        Venues = await db.Venues
            .AsNoTracking()
            .OrderBy(v => v.Name)
            .ToListAsync();
    }
}
