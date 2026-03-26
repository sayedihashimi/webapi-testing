using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public class DetailsModel(IEventService eventService) : PageModel
{
    public Event Event { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var ev = await eventService.GetByIdAsync(id);
        if (ev is null)
        {
            return NotFound();
        }

        Event = ev;
        return Page();
    }

    public async Task<IActionResult> OnPostPublishAsync(int id)
    {
        await eventService.PublishAsync(id);
        TempData["SuccessMessage"] = "Event published successfully.";
        return RedirectToPage("Details", new { id });
    }
}
