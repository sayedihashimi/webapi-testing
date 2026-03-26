using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public class CompleteModel : PageModel
{
    private readonly IEventService _eventService;

    public CompleteModel(IEventService eventService)
    {
        _eventService = eventService;
    }

    public Event Event { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var evt = await _eventService.GetEventByIdAsync(id);
        if (evt == null) return NotFound();
        Event = evt;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var result = await _eventService.CompleteEventAsync(id);
        if (!result)
        {
            TempData["ErrorMessage"] = "Cannot complete this event. Ensure it has ended and is Published or SoldOut.";
            return RedirectToPage("Details", new { id });
        }

        TempData["SuccessMessage"] = "Event marked as completed.";
        return RedirectToPage("Details", new { id });
    }
}
