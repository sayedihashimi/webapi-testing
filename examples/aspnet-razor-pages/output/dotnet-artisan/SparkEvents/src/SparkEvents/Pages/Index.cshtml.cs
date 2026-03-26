using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Pages;

public sealed class IndexModel(SparkEventsDbContext db) : PageModel
{
    public int TotalEvents { get; set; }
    public int TotalRegistrations { get; set; }
    public int EventsThisMonth { get; set; }
    public int TotalAttendees { get; set; }
    public List<Event> TodaysEvents { get; set; } = [];
    public List<Event> UpcomingEvents { get; set; } = [];
    public List<Registration> RecentRegistrations { get; set; } = [];

    public async Task OnGetAsync()
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var startOfMonth = new DateTime(today.Year, today.Month, 1);
        var nextWeek = today.AddDays(7);

        TotalEvents = await db.Events.CountAsync();
        TotalRegistrations = await db.Registrations.CountAsync(r => r.Status != RegistrationStatus.Cancelled);
        EventsThisMonth = await db.Events.CountAsync(e => e.StartDate >= startOfMonth && e.StartDate < startOfMonth.AddMonths(1));
        TotalAttendees = await db.Attendees.CountAsync();

        TodaysEvents = await db.Events
            .Include(e => e.Venue)
            .Include(e => e.Registrations)
            .Where(e => e.StartDate.Date == today && e.Status != EventStatus.Cancelled)
            .OrderBy(e => e.StartDate)
            .ToListAsync();

        UpcomingEvents = await db.Events
            .Include(e => e.Venue)
            .AsNoTracking()
            .Where(e => e.StartDate > today && e.StartDate <= nextWeek && e.Status != EventStatus.Cancelled && e.Status != EventStatus.Completed)
            .OrderBy(e => e.StartDate)
            .Take(10)
            .ToListAsync();

        RecentRegistrations = await db.Registrations
            .Include(r => r.Attendee)
            .Include(r => r.Event)
            .AsNoTracking()
            .OrderByDescending(r => r.RegistrationDate)
            .Take(10)
            .ToListAsync();
    }
}
