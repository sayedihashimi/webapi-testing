using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Attendees;

public sealed class DetailsModel : PageModel
{
    private readonly IAttendeeService _attendeeService;

    public DetailsModel(IAttendeeService attendeeService)
    {
        _attendeeService = attendeeService;
    }

    public Attendee Attendee { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var attendee = await _attendeeService.GetByIdWithRegistrationsAsync(id);
        if (attendee is null)
        {
            return NotFound();
        }

        Attendee = attendee;
        return Page();
    }
}
