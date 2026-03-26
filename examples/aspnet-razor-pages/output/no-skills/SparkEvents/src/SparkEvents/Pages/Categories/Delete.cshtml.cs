using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Categories;

public class DeleteModel : PageModel
{
    private readonly ICategoryService _categoryService;

    public DeleteModel(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public EventCategory Category { get; set; } = null!;
    public bool HasEvents { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        if (category == null) return NotFound();

        Category = category;
        HasEvents = await _categoryService.HasEventsAsync(id);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var result = await _categoryService.DeleteCategoryAsync(id);
        if (!result)
        {
            TempData["ErrorMessage"] = "Cannot delete this category. It may have associated events.";
            return RedirectToPage("Index");
        }

        TempData["SuccessMessage"] = "Category deleted successfully.";
        return RedirectToPage("Index");
    }
}
