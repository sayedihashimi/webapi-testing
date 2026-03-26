using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Venues;

public sealed class IndexModel : PageModel
{
    private readonly IVenueService _venueService;
    private const int PageSize = 10;

    public IndexModel(IVenueService venueService)
    {
        _venueService = venueService;
    }

    public PaginatedList<Venue> Venues { get; set; } = default!;

    public async Task OnGetAsync([FromQuery] int pageNumber = 1, CancellationToken ct = default)
    {
        if (pageNumber < 1) pageNumber = 1;
        Venues = await _venueService.GetAllAsync(pageNumber, PageSize, ct);
    }
}
