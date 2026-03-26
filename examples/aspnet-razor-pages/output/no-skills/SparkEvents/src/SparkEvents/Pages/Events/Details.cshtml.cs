using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public class DetailsModel : PageModel
{
    private readonly IEventService _eventService;

    public DetailsModel(IEventService eventService)
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

    public async Task<IActionResult> OnPostPublishAsync(int id)
    {
        var result = await _eventService.PublishEventAsync(id);
        if (!result)
            TempData["ErrorMessage"] = "Cannot publish event. Ensure it has at least one active ticket type.";
        else
            TempData["SuccessMessage"] = "Event published successfully.";
        return RedirectToPage("Details", new { id });
    }
}
