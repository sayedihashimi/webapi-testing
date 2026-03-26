using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Events;

public sealed class CancelModel(IEventService eventService) : PageModel
{
    public Event Event { get; set; } = null!;
    public int ActiveRegistrationCount { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public sealed class InputModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "A cancellation reason is required.")]
        [MaxLength(1000)]
        [Display(Name = "Cancellation Reason")]
        public string CancellationReason { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var evt = await eventService.GetEventByIdAsync(id);
        if (evt is null)
        {
            return NotFound();
        }

        if (evt.Status is EventStatus.Completed or EventStatus.Cancelled)
        {
            TempData["StatusMessage"] = "Error: This event cannot be cancelled.";
            return RedirectToPage("Details", new { id });
        }

        Event = evt;
        ActiveRegistrationCount = evt.CurrentRegistrations + evt.WaitlistCount;
        Input.Id = id;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            var evt = await eventService.GetEventByIdAsync(Input.Id);
            if (evt is null)
            {
                return NotFound();
            }

            Event = evt;
            ActiveRegistrationCount = evt.CurrentRegistrations + evt.WaitlistCount;
            return Page();
        }

        try
        {
            await eventService.CancelEventAsync(Input.Id, Input.CancellationReason);
            TempData["StatusMessage"] = "Event cancelled successfully.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["StatusMessage"] = $"Error: {ex.Message}";
        }

        return RedirectToPage("Details", new { id = Input.Id });
    }
}
