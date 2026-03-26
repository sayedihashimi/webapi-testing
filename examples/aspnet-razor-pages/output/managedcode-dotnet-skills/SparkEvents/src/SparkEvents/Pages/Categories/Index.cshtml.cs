using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Categories;

public class IndexModel(ICategoryService categoryService) : PageModel
{
    public IReadOnlyList<EventCategory> Categories { get; set; } = [];

    public async Task OnGetAsync()
    {
        Categories = await categoryService.GetAllAsync();
    }
}
