using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Attendees;

public class DetailsModel(IAttendeeService attendeeService) : PageModel
{
    public Attendee Attendee { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var attendee = await attendeeService.GetByIdWithRegistrationsAsync(id);
        if (attendee is null)
        {
            return NotFound();
        }

        Attendee = attendee;
        return Page();
    }
}
