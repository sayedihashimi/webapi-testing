using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public sealed class DetailsModel : PageModel
{
    private readonly IEventService _eventService;

    public DetailsModel(IEventService eventService)
    {
        _eventService = eventService;
    }

    public Event Event { get; set; } = null!;
    public int CapacityPercent => Event.TotalCapacity > 0
        ? (int)((double)Event.CurrentRegistrations / Event.TotalCapacity * 100)
        : 0;

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var evt = await _eventService.GetByIdWithDetailsAsync(id, ct);
        if (evt is null)
        {
            return NotFound();
        }

        Event = evt;
        return Page();
    }

    public async Task<IActionResult> OnPostPublishAsync(int id, CancellationToken ct)
    {
        try
        {
            var success = await _eventService.PublishAsync(id, ct);
            if (!success)
            {
                TempData["ErrorMessage"] = "Event not found.";
                return RedirectToPage("/Events/Index");
            }

            TempData["SuccessMessage"] = "Event published successfully!";
            return RedirectToPage(new { id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage(new { id });
        }
    }
}
