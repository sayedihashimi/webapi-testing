using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public sealed class DetailsModel(IEventService eventService) : PageModel
{
    public Event Event { get; set; } = null!;

    [TempData]
    public string? StatusMessage { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var evt = await eventService.GetEventByIdAsync(id);
        if (evt is null)
        {
            return NotFound();
        }

        Event = evt;
        return Page();
    }

    public async Task<IActionResult> OnPostPublishAsync(int id)
    {
        try
        {
            await eventService.PublishEventAsync(id);
            TempData["StatusMessage"] = "Event published successfully.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["StatusMessage"] = $"Error: {ex.Message}";
        }

        return RedirectToPage(new { id });
    }
}
