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
    public List<EventCategory> Categories { get; set; } = new();
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public EventStatus? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public async Task OnGetAsync(string? search, int? categoryId, int? status, DateTime? startDate, DateTime? endDate, int pageNumber = 1)
    {
        Search = search;
        CategoryId = categoryId;
        Status = status.HasValue ? (EventStatus)status.Value : null;
        StartDate = startDate;
        EndDate = endDate;

        Categories = await _categoryService.GetAllCategoriesAsync();
        Events = await _eventService.GetEventsAsync(search, categoryId, Status, startDate, endDate, pageNumber, 10);
    }
}
