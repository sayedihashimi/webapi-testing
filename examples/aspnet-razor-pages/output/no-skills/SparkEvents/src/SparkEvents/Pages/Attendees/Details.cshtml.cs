using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Attendees;

public class DetailsModel : PageModel
{
    private readonly IAttendeeService _attendeeService;

    public DetailsModel(IAttendeeService attendeeService)
    {
        _attendeeService = attendeeService;
    }

    public Attendee Attendee { get; set; } = null!;
    public List<Registration> Registrations { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var attendee = await _attendeeService.GetAttendeeByIdAsync(id);
        if (attendee == null) return NotFound();
        Attendee = attendee;
        Registrations = await _attendeeService.GetAttendeeRegistrationsAsync(id);
        return Page();
    }
}
