using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Venues;

public class DeleteModel : PageModel
{
    private readonly IVenueService _venueService;
    public DeleteModel(IVenueService venueService) => _venueService = venueService;

    public Venue? Venue { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Venue = await _venueService.GetVenueByIdAsync(id);
        if (Venue == null) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var (success, error) = await _venueService.DeleteVenueAsync(id);
        if (!success)
        {
            TempData["ErrorMessage"] = error;
            return RedirectToPage("Index");
        }
        TempData["SuccessMessage"] = "Venue deleted successfully.";
        return RedirectToPage("Index");
    }
}
