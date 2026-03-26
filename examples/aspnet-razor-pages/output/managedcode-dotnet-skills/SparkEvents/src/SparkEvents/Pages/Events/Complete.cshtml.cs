using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public class CompleteModel(IEventService eventService) : PageModel
{
    public Event Event { get; set; } = null!;

    [BindProperty]
    public int EventId { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var ev = await eventService.GetByIdAsync(id);
        if (ev is null)
        {
            return NotFound();
        }

        if (ev.EndDate >= DateTime.UtcNow)
        {
            TempData["ErrorMessage"] = "Cannot complete an event that has not yet ended.";
            return RedirectToPage("Details", new { id });
        }

        if (ev.Status is EventStatus.Cancelled or EventStatus.Completed)
        {
            TempData["ErrorMessage"] = $"Cannot complete a {ev.Status.ToString().ToLowerInvariant()} event.";
            return RedirectToPage("Details", new { id });
        }

        Event = ev;
        EventId = id;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var ev = await eventService.GetByIdAsync(EventId);
        if (ev is null)
        {
            return NotFound();
        }

        if (ev.EndDate >= DateTime.UtcNow)
        {
            TempData["ErrorMessage"] = "Cannot complete an event that has not yet ended.";
            return RedirectToPage("Details", new { id = EventId });
        }

        await eventService.CompleteAsync(EventId);
        TempData["SuccessMessage"] = "Event has been marked as completed.";
        return RedirectToPage("Details", new { id = EventId });
    }
}
