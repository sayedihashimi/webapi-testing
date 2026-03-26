using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Pages.Attendees;

public sealed class DetailsModel(SparkEventsDbContext db) : PageModel
{
    public Attendee Attendee { get; set; } = null!;
    public IReadOnlyList<Registration> Registrations { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var attendee = await db.Attendees
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);

        if (attendee is null)
        {
            return NotFound();
        }

        Attendee = attendee;

        Registrations = await db.Registrations
            .AsNoTracking()
            .Include(r => r.Event)
            .Include(r => r.TicketType)
            .Where(r => r.AttendeeId == id)
            .OrderByDescending(r => r.RegistrationDate)
            .ToListAsync();

        return Page();
    }
}
