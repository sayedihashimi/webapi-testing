using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public class IndexModel : PageModel
{
    private readonly IEventService _eventService;
    private readonly ICategoryService _categoryService;

    public IndexModel(IEventService eventService, ICategoryService categoryService)
    {
        _eventService = eventService;
        _categoryService = categoryService;
    }

    public PaginatedList<Event> Events { get; set; } = null!;
    public List<EventCategory> AllCategories { get; set; } = new();
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public EventStatus? Status { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    public async Task OnGetAsync(string? search, int? categoryId, EventStatus? status, DateTime? fromDate, DateTime? toDate, int pageNumber = 1)
    {
        Search = search;
        CategoryId = categoryId;
        Status = status;
        FromDate = fromDate;
        ToDate = toDate;
        AllCategories = await _categoryService.GetAllCategoriesAsync();
        Events = await _eventService.GetEventsAsync(search, categoryId, status, fromDate, toDate, pageNumber);
    }
}
