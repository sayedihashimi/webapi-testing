using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public class RosterModel(IEventService eventService, IRegistrationService registrationService) : PageModel
{
    public Event Event { get; set; } = null!;

    public IReadOnlyList<Registration> Registrations { get; set; } = [];

    public int TotalCount { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public PaginationModel Pagination { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int eventId)
    {
        var ev = await eventService.GetByIdAsync(eventId);
        if (ev is null)
        {
            return NotFound();
        }

        Event = ev;

        var (items, totalCount) = await registrationService.GetEventRosterAsync(
            eventId, Search, PageNumber, PageSize);

        Registrations = items;
        TotalCount = totalCount;

        var totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
        Pagination = new PaginationModel
        {
            CurrentPage = PageNumber,
            TotalPages = totalPages,
            PageUrl = page => Url.Page("Roster", new
            {
                eventId,
                pageNumber = page,
                search = Search
            })!
        };

        return Page();
    }
}
