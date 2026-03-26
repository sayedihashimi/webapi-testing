using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Attendees;

public class IndexModel(IAttendeeService attendeeService) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;

    public IReadOnlyList<Attendee> Attendees { get; set; } = [];

    public PaginationModel Pagination { get; set; } = null!;

    public async Task OnGetAsync()
    {
        var (items, totalCount) = await attendeeService.GetFilteredAsync(Search, PageNumber, PageSize);

        Attendees = items;

        var totalPages = (int)Math.Ceiling(totalCount / (double)PageSize);
        Pagination = new PaginationModel
        {
            CurrentPage = PageNumber,
            TotalPages = totalPages,
            PageUrl = page => Url.Page("Index", new
            {
                pageNumber = page,
                search = Search
            })!
        };
    }
}
