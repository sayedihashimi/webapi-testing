using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Venues;

public class IndexModel(IVenueService venueService) : PageModel
{
    public IReadOnlyList<Venue> Venues { get; set; } = [];

    public async Task OnGetAsync()
    {
        Venues = await venueService.GetAllAsync();
    }
}
