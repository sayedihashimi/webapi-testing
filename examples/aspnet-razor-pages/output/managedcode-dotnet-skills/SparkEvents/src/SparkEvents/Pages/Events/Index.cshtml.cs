using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public class IndexModel(IEventService eventService, ICategoryService categoryService) : PageModel
{
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

    public IReadOnlyList<Event> Events { get; set; } = [];
    public IReadOnlyList<EventCategory> Categories { get; set; } = [];
    public PaginationModel Pagination { get; set; } = null!;

    public async Task OnGetAsync()
    {
        Categories = await categoryService.GetAllAsync();

        var (items, totalCount) = await eventService.GetFilteredAsync(
            Search, CategoryId, Status, FromDate, ToDate, PageNumber, PageSize);

        Events = items;

        var totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
        Pagination = new PaginationModel
        {
            CurrentPage = PageNumber,
            TotalPages = totalPages,
            PageUrl = page => Url.Page("Index", new
            {
                pageNumber = page,
                pageSize = PageSize,
                search = Search,
                categoryId = CategoryId,
                status = Status?.ToString(),
                fromDate = FromDate?.ToString("yyyy-MM-dd"),
                toDate = ToDate?.ToString("yyyy-MM-dd")
            })!
        };
    }
}
