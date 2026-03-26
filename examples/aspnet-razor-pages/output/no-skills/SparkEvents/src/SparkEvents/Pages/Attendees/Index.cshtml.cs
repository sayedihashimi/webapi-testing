using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Attendees;

public class IndexModel : PageModel
{
    private readonly IAttendeeService _attendeeService;

    public IndexModel(IAttendeeService attendeeService)
    {
        _attendeeService = attendeeService;
    }

    public PaginatedList<Attendee> Attendees { get; set; } = null!;
    public string? Search { get; set; }

    public async Task OnGetAsync(string? search, int pageNumber = 1)
    {
        Search = search;
        Attendees = await _attendeeService.GetAttendeesAsync(search, pageNumber, 10);
    }
}
