using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SparkEvents.Data;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public sealed class IndexModel(IEventService eventService, SparkEventsDbContext db) : PageModel
{
    public PaginatedList<Event> Events { get; set; } = null!;
    public SelectList Categories { get; set; } = null!;
    public SelectList Statuses { get; set; } = null!;

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? CategoryId { get; set; }

    [BindProperty(SupportsGet = true)]
    public EventStatus? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? FromDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateTime? ToDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 10;

    public async Task OnGetAsync()
    {
        var categories = await db.EventCategories
            .OrderBy(c => c.Name)
            .AsNoTracking()
            .ToListAsync();

        Categories = new SelectList(categories, nameof(EventCategory.Id), nameof(EventCategory.Name));
        Statuses = new SelectList(Enum.GetValues<EventStatus>());

        Events = await eventService.GetEventsAsync(Search, CategoryId, Status, FromDate, ToDate, PageNumber, PageSize);
    }
}
