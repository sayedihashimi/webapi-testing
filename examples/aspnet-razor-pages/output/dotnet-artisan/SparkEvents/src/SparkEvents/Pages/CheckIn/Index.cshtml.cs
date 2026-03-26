using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.CheckIn;

public sealed class IndexModel(
    IEventService eventService,
    ICheckInService checkInService) : PageModel
{
    public Event Event { get; set; } = null!;
    public int CheckedInCount { get; set; }
    public int TotalConfirmed { get; set; }
    public List<Registration> SearchResults { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public string? Query { get; set; }

    public async Task<IActionResult> OnGetAsync(int eventId)
    {
        var evt = await eventService.GetEventByIdAsync(eventId);
        if (evt is null)
        {
            return NotFound();
        }

        Event = evt;

        var (checkedIn, total) = await checkInService.GetCheckInStatsAsync(eventId);
        CheckedInCount = checkedIn;
        TotalConfirmed = total;

        if (!string.IsNullOrWhiteSpace(Query))
        {
            SearchResults = await checkInService.SearchForCheckInAsync(eventId, Query);
        }

        return Page();
    }
}
