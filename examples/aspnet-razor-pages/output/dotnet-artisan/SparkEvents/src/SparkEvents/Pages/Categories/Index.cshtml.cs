using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Pages.Categories;

public sealed class IndexModel(SparkEventsDbContext db) : PageModel
{
    public IReadOnlyList<EventCategory> Categories { get; set; } = [];

    public async Task OnGetAsync()
    {
        Categories = await db.EventCategories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();
    }
}
