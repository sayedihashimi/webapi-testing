using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public class CancelModel(IEventService eventService) : PageModel
{
    public Event Event { get; set; } = null!;

    [BindProperty]
    public int EventId { get; set; }

    [BindProperty]
    public CancelInputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var ev = await eventService.GetByIdAsync(id);
        if (ev is null)
        {
            return NotFound();
        }

        if (ev.Status == EventStatus.Completed)
        {
            TempData["ErrorMessage"] = "Cannot cancel a completed event.";
            return RedirectToPage("Details", new { id });
        }

        if (ev.Status == EventStatus.Cancelled)
        {
            TempData["ErrorMessage"] = "This event is already cancelled.";
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

        if (ev.Status == EventStatus.Completed)
        {
            TempData["ErrorMessage"] = "Cannot cancel a completed event.";
            return RedirectToPage("Details", new { id = EventId });
        }

        if (!ModelState.IsValid)
        {
            Event = ev;
            return Page();
        }

        await eventService.CancelAsync(EventId, Input.CancellationReason);
        TempData["SuccessMessage"] = "Event has been cancelled.";
        return RedirectToPage("Details", new { id = EventId });
    }

    public class CancelInputModel
    {
        [Required(ErrorMessage = "A cancellation reason is required.")]
        [MaxLength(2000)]
        [Display(Name = "Cancellation Reason")]
        public string CancellationReason { get; set; } = string.Empty;
    }
}
