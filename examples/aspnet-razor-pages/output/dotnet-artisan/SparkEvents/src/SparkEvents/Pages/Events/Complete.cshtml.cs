using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public sealed class CompleteModel(IEventService eventService) : PageModel
{
    public Event Event { get; set; } = null!;

    [BindProperty]
    public int Id { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var evt = await eventService.GetEventByIdAsync(id);
        if (evt is null)
        {
            return NotFound();
        }

        if (evt.Status is not (EventStatus.Published or EventStatus.SoldOut))
        {
            TempData["StatusMessage"] = "Error: Only published or sold-out events can be completed.";
            return RedirectToPage("Details", new { id });
        }

        if (evt.EndDate > DateTime.UtcNow)
        {
            TempData["StatusMessage"] = "Error: Cannot complete an event before its end date.";
            return RedirectToPage("Details", new { id });
        }

        Event = evt;
        Id = id;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            await eventService.CompleteEventAsync(Id);
            TempData["StatusMessage"] = "Event marked as completed.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["StatusMessage"] = $"Error: {ex.Message}";
        }

        return RedirectToPage("Details", new { id = Id });
    }
}
