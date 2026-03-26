using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages;

public sealed class IndexModel : PageModel
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

    public List<Event> UpcomingEvents { get; set; } = [];
    public List<Event> TodaysEvents { get; set; } = [];
    public List<Registration> RecentRegistrations { get; set; } = [];
    public int TotalEvents { get; set; }
    public int TotalRegistrations { get; set; }
    public int EventsThisMonth { get; set; }
    public Dictionary<int, int> CheckInCounts { get; set; } = [];
    public Dictionary<int, int> ConfirmedCounts { get; set; } = [];

    public async Task OnGetAsync(CancellationToken ct)
    {
        UpcomingEvents = await _eventService.GetUpcomingEventsAsync(7, ct);
        TodaysEvents = await _eventService.GetTodaysEventsAsync(ct);
        RecentRegistrations = await _registrationService.GetRecentRegistrationsAsync(10, ct);
        TotalEvents = await _eventService.GetTotalEventsCountAsync(ct);
        TotalRegistrations = await _registrationService.GetTotalRegistrationsCountAsync(ct);
        EventsThisMonth = await _eventService.GetEventsThisMonthCountAsync(ct);

        foreach (var evt in TodaysEvents)
        {
            CheckInCounts[evt.Id] = await _checkInService.GetCheckInCountAsync(evt.Id, ct);
            ConfirmedCounts[evt.Id] = await _checkInService.GetTotalConfirmedCountAsync(evt.Id, ct);
        }
    }
}
