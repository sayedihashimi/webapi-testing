using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public sealed class CompleteModel : PageModel
{
    private readonly IEventService _eventService;

    public CompleteModel(IEventService eventService)
    {
        _eventService = eventService;
    }

    public Event Event { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var evt = await _eventService.GetByIdWithDetailsAsync(id, ct);
        if (evt is null)
        {
            return NotFound();
        }

        if (evt.EndDate >= DateTime.UtcNow)
        {
            TempData["ErrorMessage"] = "Cannot complete an event that has not ended yet.";
            return RedirectToPage("Details", new { id });
        }

        Event = evt;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken ct)
    {
        var evt = await _eventService.GetByIdAsync(id, ct);
        if (evt is null)
        {
            return NotFound();
        }

        if (evt.EndDate >= DateTime.UtcNow)
        {
            TempData["ErrorMessage"] = "Cannot complete an event that has not ended yet.";
            return RedirectToPage("Details", new { id });
        }

        var success = await _eventService.CompleteAsync(id, ct);
        if (!success)
        {
            TempData["ErrorMessage"] = "Unable to complete this event.";
            return RedirectToPage("Details", new { id });
        }

        TempData["SuccessMessage"] = "Event has been marked as completed.";
        return RedirectToPage("Details", new { id });
    }
}
