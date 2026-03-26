using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public sealed class IndexModel : PageModel
{
    private readonly IEventService _eventService;
    private readonly IEventCategoryService _categoryService;

    public IndexModel(IEventService eventService, IEventCategoryService categoryService)
    {
        _eventService = eventService;
        _categoryService = categoryService;
    }

    public PaginatedList<Event> Events { get; set; } = null!;
    public List<SelectListItem> Categories { get; set; } = [];

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

    public async Task OnGetAsync(CancellationToken ct)
    {
        var categories = await _categoryService.GetAllForDropdownAsync(ct);
        Categories = categories
            .Select(c => new SelectListItem(c.Name, c.Id.ToString()))
            .ToList();

        Events = await _eventService.GetAllAsync(
            Search, CategoryId, Status, FromDate, ToDate, PageNumber, PageSize, ct);
    }
}
