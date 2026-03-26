using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public class CompleteModel : PageModel
{
    private readonly IEventService _eventService;
    public CompleteModel(IEventService eventService) => _eventService = eventService;

    public Event? Event { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Event = await _eventService.GetEventByIdAsync(id);
        if (Event == null) return NotFound();
        if (Event.EndDate > DateTime.UtcNow)
        {
            TempData["ErrorMessage"] = "Cannot complete an event that hasn't ended yet.";
            return RedirectToPage("Details", new { id });
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var result = await _eventService.CompleteEventAsync(id);
        if (!result)
        {
            TempData["ErrorMessage"] = "Cannot mark this event as completed.";
            return RedirectToPage("Details", new { id });
        }
        TempData["SuccessMessage"] = "Event marked as completed.";
        return RedirectToPage("Details", new { id });
    }
}
