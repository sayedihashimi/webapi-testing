using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Venues;

public class IndexModel : PageModel
{
    private readonly IVenueService _venueService;

    public IndexModel(IVenueService venueService)
    {
        _venueService = venueService;
    }

    public List<Venue> Venues { get; set; } = new();

    public async Task OnGetAsync()
    {
        Venues = await _venueService.GetAllVenuesAsync();
    }
}
