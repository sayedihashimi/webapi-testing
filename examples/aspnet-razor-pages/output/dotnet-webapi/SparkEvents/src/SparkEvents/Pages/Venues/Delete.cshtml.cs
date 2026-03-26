using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Venues;

public sealed class DeleteModel : PageModel
{
    private readonly IVenueService _venueService;

    public DeleteModel(IVenueService venueService)
    {
        _venueService = venueService;
    }

    public Venue Venue { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var venue = await _venueService.GetByIdAsync(id, ct);
        if (venue is null)
        {
            return NotFound();
        }

        Venue = venue;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken ct)
    {
        var venue = await _venueService.GetByIdAsync(id, ct);
        if (venue is null)
        {
            return NotFound();
        }

        if (await _venueService.HasFutureEventsAsync(id, ct))
        {
            Venue = venue;
            ModelState.AddModelError(string.Empty, "Cannot delete this venue because it has future events scheduled.");
            return Page();
        }

        await _venueService.DeleteAsync(id, ct);

        TempData["SuccessMessage"] = "Venue deleted successfully.";
        return RedirectToPage("Index");
    }
}
