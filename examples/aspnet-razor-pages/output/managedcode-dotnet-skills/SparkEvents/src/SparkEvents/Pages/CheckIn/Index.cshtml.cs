using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.CheckIn;

public class IndexModel(
    IEventService eventService,
    ICheckInService checkInService) : PageModel
{
    public Event Event { get; set; } = null!;

    public int TotalConfirmed { get; set; }

    public int CheckedInCount { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    public Registration? FoundRegistration { get; set; }

    public string? LookupMessage { get; set; }

    public bool CheckInWindowOpen { get; set; }

    public string? WindowMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int eventId)
    {
        var ev = await eventService.GetByIdAsync(eventId);
        if (ev is null)
        {
            return NotFound();
        }

        Event = ev;
        var (totalConfirmed, checkedIn) = await checkInService.GetCheckInStatsAsync(eventId);
        TotalConfirmed = totalConfirmed;
        CheckedInCount = checkedIn;

        ValidateCheckInWindow();

        if (!string.IsNullOrWhiteSpace(SearchTerm) && CheckInWindowOpen)
        {
            FoundRegistration = await checkInService.LookupForCheckInAsync(eventId, SearchTerm);
            if (FoundRegistration is null)
            {
                LookupMessage = "No matching registration found.";
            }
        }

        return Page();
    }

    private void ValidateCheckInWindow()
    {
        var now = DateTime.UtcNow;
        var windowStart = Event.StartDate.AddHours(-1);
        var windowEnd = Event.EndDate;

        if (now < windowStart)
        {
            CheckInWindowOpen = false;
            WindowMessage = $"Check-in opens at {windowStart:MMM dd, yyyy h:mm tt} (1 hour before event start).";
        }
        else if (now > windowEnd)
        {
            CheckInWindowOpen = false;
            WindowMessage = "Check-in is closed. The event has ended.";
        }
        else
        {
            CheckInWindowOpen = true;
        }
    }
}
