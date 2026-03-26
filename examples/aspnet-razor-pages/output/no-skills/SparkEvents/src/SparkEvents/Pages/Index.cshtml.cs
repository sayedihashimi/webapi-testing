using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages;

public class IndexModel : PageModel
{
    private readonly IEventService _eventService;
    private readonly IRegistrationService _registrationService;
    private readonly ICheckInService _checkInService;

    public IndexModel(IEventService eventService, IRegistrationService registrationService, ICheckInService checkInService)
    {
        _eventService = eventService;
        _registrationService = registrationService;
        _checkInService = checkInService;
    }

    public int TotalEvents { get; set; }
    public int TotalRegistrations { get; set; }
    public int EventsThisMonth { get; set; }
    public List<Event> TodaysEvents { get; set; } = new();
    public List<Event> UpcomingEvents { get; set; } = new();
    public List<Registration> RecentRegistrations { get; set; } = new();
    public Dictionary<int, (int CheckedIn, int Total)> CheckInStats { get; set; } = new();

    public async Task OnGetAsync()
    {
        TotalEvents = await _eventService.GetTotalEventsCountAsync();
        TotalRegistrations = await _registrationService.GetTotalRegistrationsCountAsync();
        EventsThisMonth = await _eventService.GetEventsThisMonthCountAsync();
        TodaysEvents = await _eventService.GetTodaysEventsAsync();
        UpcomingEvents = await _eventService.GetUpcomingEventsAsync(7);
        RecentRegistrations = await _registrationService.GetRecentRegistrationsAsync(5);

        foreach (var evt in TodaysEvents)
        {
            CheckInStats[evt.Id] = await _checkInService.GetCheckInStatsAsync(evt.Id);
        }
    }
}
