using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages;

public class IndexModel(
    IEventService eventService,
    IRegistrationService registrationService,
    ICheckInService checkInService) : PageModel
{
    public int TotalEvents { get; set; }

    public int TotalRegistrations { get; set; }

    public int EventsThisMonth { get; set; }

    public List<Event> TodayEvents { get; set; } = [];

    public List<Event> UpcomingEvents { get; set; } = [];

    public List<Registration> RecentRegistrations { get; set; } = [];

    public Dictionary<int, (int TotalConfirmed, int CheckedIn)> CheckInStats { get; set; } = [];

    public async Task OnGetAsync()
    {
        var (totalEvents, totalRegistrations, eventsThisMonth) = await eventService.GetStatsAsync();
        TotalEvents = totalEvents;
        TotalRegistrations = totalRegistrations;
        EventsThisMonth = eventsThisMonth;

        TodayEvents = await eventService.GetTodayEventsAsync();
        UpcomingEvents = await eventService.GetUpcomingEventsAsync(7);
        RecentRegistrations = await registrationService.GetRecentRegistrationsAsync(10);

        foreach (var ev in TodayEvents)
        {
            var stats = await checkInService.GetCheckInStatsAsync(ev.Id);
            CheckInStats[ev.Id] = stats;
        }
    }
}
