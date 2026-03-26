using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.CheckIn;

public class IndexModel : PageModel
{
    private readonly IEventService _eventService;
    private readonly ICheckInService _checkInService;

    public IndexModel(IEventService eventService, ICheckInService checkInService)
    {
        _eventService = eventService;
        _checkInService = checkInService;
    }

    public Event? Event { get; set; }
    public int CheckedInCount { get; set; }
    public int TotalConfirmed { get; set; }
    public bool IsCheckInOpen { get; set; }
    public string? Search { get; set; }
    public List<Registration> SearchResults { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int eventId, string? search)
    {
        Event = await _eventService.GetEventByIdAsync(eventId);
        if (Event == null) return NotFound();

        var (checkedIn, total) = await _checkInService.GetCheckInStatsAsync(eventId);
        CheckedInCount = checkedIn;
        TotalConfirmed = total;
        IsCheckInOpen = await _checkInService.IsCheckInWindowOpenAsync(eventId);
        Search = search;

        if (!string.IsNullOrWhiteSpace(search))
        {
            SearchResults = await _checkInService.SearchForCheckInAsync(eventId, search);
        }
        else
        {
            SearchResults = await _checkInService.SearchForCheckInAsync(eventId, null);
        }

        return Page();
    }
}
