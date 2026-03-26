using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Venues;

public class DeleteModel(IVenueService venueService) : PageModel
{
    public Venue? Venue { get; set; }

    public bool HasFutureEvents { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Venue = await venueService.GetByIdAsync(id);
        if (Venue is null)
        {
            return NotFound();
        }

        HasFutureEvents = await venueService.HasFutureEventsAsync(id);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var venue = await venueService.GetByIdAsync(id);
        if (venue is null)
        {
            return NotFound();
        }

        if (await venueService.HasFutureEventsAsync(id))
        {
            TempData["ErrorMessage"] = "Cannot delete a venue that has future events scheduled.";
            return RedirectToPage("Index");
        }

        await venueService.DeleteAsync(id);

        TempData["SuccessMessage"] = "Venue deleted successfully.";
        return RedirectToPage("Index");
    }
}
