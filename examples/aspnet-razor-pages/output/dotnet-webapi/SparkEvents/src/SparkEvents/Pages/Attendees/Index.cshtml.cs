using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Attendees;

public sealed class IndexModel : PageModel
{
    private readonly IAttendeeService _attendeeService;

    public IndexModel(IAttendeeService attendeeService)
    {
        _attendeeService = attendeeService;
    }

    public PaginatedList<Attendee> Attendees { get; set; } = default!;

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int PageSize { get; set; } = 10;

    public async Task OnGetAsync()
    {
        Attendees = await _attendeeService.GetAllAsync(Search, PageNumber, PageSize);
    }
}
