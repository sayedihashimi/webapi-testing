using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Categories;

public class DeleteModel : PageModel
{
    private readonly ICategoryService _categoryService;
    public DeleteModel(ICategoryService categoryService) => _categoryService = categoryService;

    public EventCategory? Category { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Category = await _categoryService.GetCategoryByIdAsync(id);
        if (Category == null) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var (success, error) = await _categoryService.DeleteCategoryAsync(id);
        if (!success)
        {
            TempData["ErrorMessage"] = error;
            return RedirectToPage("Index");
        }
        TempData["SuccessMessage"] = "Category deleted successfully.";
        return RedirectToPage("Index");
    }
}
