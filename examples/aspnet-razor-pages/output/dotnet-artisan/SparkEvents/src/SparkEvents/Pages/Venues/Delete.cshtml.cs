using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Pages.Venues;

public sealed class DeleteModel(SparkEventsDbContext db) : PageModel
{
    public Venue Venue { get; set; } = null!;

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
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var venue = await db.Venues
            .Include(v => v.Events)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (venue is null)
        {
            return NotFound();
        }

        var futureEvents = venue.Events.Count(e => e.StartDate >= DateTime.UtcNow);
        if (futureEvents > 0)
        {
            TempData["ErrorMessage"] = $"Cannot delete \"{venue.Name}\" because it has {futureEvents} future event(s) scheduled.";
            return RedirectToPage("Index");
        }

        db.Venues.Remove(venue);
        await db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Venue \"{venue.Name}\" deleted successfully.";
        return RedirectToPage("Index");
    }
}
