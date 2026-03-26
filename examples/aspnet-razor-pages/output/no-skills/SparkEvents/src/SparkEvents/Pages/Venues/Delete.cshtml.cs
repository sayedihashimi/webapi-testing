using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Venues;

public class DeleteModel : PageModel
{
    private readonly IVenueService _venueService;

    public DeleteModel(IVenueService venueService)
    {
        _venueService = venueService;
    }

    public Venue Venue { get; set; } = null!;
    public bool HasFutureEvents { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var venue = await _venueService.GetVenueByIdAsync(id);
        if (venue == null) return NotFound();

        Venue = venue;
        HasFutureEvents = await _venueService.HasFutureEventsAsync(id);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var result = await _venueService.DeleteVenueAsync(id);
        if (!result)
        {
            TempData["ErrorMessage"] = "Cannot delete this venue. It may have future events.";
            return RedirectToPage("Index");
        }

        TempData["SuccessMessage"] = "Venue deleted successfully.";
        return RedirectToPage("Index");
    }
}
