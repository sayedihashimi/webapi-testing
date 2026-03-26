using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Venues;

public class IndexModel : PageModel
{
    private readonly IVenueService _venueService;
    public IndexModel(IVenueService venueService) => _venueService = venueService;

    public PaginatedList<Venue> Venues { get; set; } = null!;

    public async Task OnGetAsync(int pageNumber = 1)
    {
        Venues = await _venueService.GetVenuesAsync(pageNumber);
    }
}
