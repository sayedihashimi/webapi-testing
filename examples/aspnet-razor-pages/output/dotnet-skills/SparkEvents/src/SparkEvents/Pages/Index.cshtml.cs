using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages;

public class IndexModel : PageModel
{
    private readonly IEventService _eventService;
    private readonly IRegistrationService _registrationService;

    public IndexModel(IEventService eventService, IRegistrationService registrationService)
    {
        _eventService = eventService;
        _registrationService = registrationService;
    }

    public int TotalEvents { get; set; }
    public int TotalRegistrations { get; set; }
    public int EventsThisMonth { get; set; }
    public List<Event> UpcomingEvents { get; set; } = new();
    public List<Event> TodaysEvents { get; set; } = new();
    public List<Registration> RecentRegistrations { get; set; } = new();

    public async Task OnGetAsync()
    {
        TotalEvents = await _eventService.GetTotalEventsCountAsync();
        TotalRegistrations = await _registrationService.GetTotalRegistrationsCountAsync();
        EventsThisMonth = await _eventService.GetEventsThisMonthCountAsync();
        UpcomingEvents = await _eventService.GetUpcomingEventsAsync();
        TodaysEvents = await _eventService.GetTodaysEventsAsync();
        RecentRegistrations = await _registrationService.GetRecentRegistrationsAsync(5);
    }
}
