using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Attendees;

public class DetailsModel : PageModel
{
    private readonly IAttendeeService _attendeeService;
    public DetailsModel(IAttendeeService attendeeService) => _attendeeService = attendeeService;

    public Attendee? Attendee { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Attendee = await _attendeeService.GetAttendeeByIdAsync(id);
        if (Attendee == null) return NotFound();
        return Page();
    }
}
