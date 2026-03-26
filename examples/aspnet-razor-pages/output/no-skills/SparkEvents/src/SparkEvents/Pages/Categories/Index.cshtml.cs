using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Categories;

public class IndexModel : PageModel
{
    private readonly ICategoryService _categoryService;

    public IndexModel(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public List<EventCategory> Categories { get; set; } = new();

    public async Task OnGetAsync()
    {
        Categories = await _categoryService.GetAllCategoriesAsync();
    }
}
