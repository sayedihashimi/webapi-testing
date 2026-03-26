using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Categories;

public sealed class IndexModel(IEventCategoryService categoryService) : PageModel
{
    public PaginatedList<EventCategory> Categories { get; set; } = default!;

    public async Task OnGetAsync(int pageNumber = 1, CancellationToken ct = default)
    {
        Categories = await categoryService.GetAllAsync(pageNumber, 10, ct);
    }
}
