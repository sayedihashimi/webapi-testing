using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Venues;

public class DetailsModel : PageModel
{
    private readonly IVenueService _venueService;
    public DetailsModel(IVenueService venueService) => _venueService = venueService;

    public Venue? Venue { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Venue = await _venueService.GetVenueByIdAsync(id);
        if (Venue == null) return NotFound();
        return Page();
    }
}
