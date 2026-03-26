using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public sealed class CheckInModel : PageModel
{
    private readonly ICheckInService _checkInService;
    private readonly IRegistrationService _registrationService;
    private readonly IEventService _eventService;

    public CheckInModel(
        ICheckInService checkInService,
        IRegistrationService registrationService,
        IEventService eventService)
    {
        _checkInService = checkInService;
        _registrationService = registrationService;
        _eventService = eventService;
    }

    public Event Event { get; set; } = default!;
    public int CheckedInCount { get; set; }
    public int TotalConfirmedCount { get; set; }
    public List<Registration> SearchResults { get; set; } = [];

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    public async Task<IActionResult> OnGetAsync(int eventId)
    {
        var evt = await _eventService.GetByIdAsync(eventId);
        if (evt is null)
        {
            return NotFound();
        }

        Event = evt;
        CheckedInCount = await _checkInService.GetCheckInCountAsync(eventId);
        TotalConfirmedCount = await _checkInService.GetTotalConfirmedCountAsync(eventId);

        if (!string.IsNullOrWhiteSpace(Search))
        {
            SearchResults = await _registrationService.SearchForCheckInAsync(eventId, Search);
        }

        return Page();
    }
}
