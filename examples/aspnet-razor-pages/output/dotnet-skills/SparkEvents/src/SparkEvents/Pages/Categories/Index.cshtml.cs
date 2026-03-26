using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Categories;

public class IndexModel : PageModel
{
    private readonly ICategoryService _categoryService;
    public IndexModel(ICategoryService categoryService) => _categoryService = categoryService;

    public PaginatedList<EventCategory> Categories { get; set; } = null!;

    public async Task OnGetAsync(int pageNumber = 1)
    {
        Categories = await _categoryService.GetCategoriesAsync(pageNumber);
    }
}
