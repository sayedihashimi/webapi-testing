using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Categories;

public class DeleteModel(ICategoryService categoryService) : PageModel
{
    public EventCategory? Category { get; set; }

    public bool HasEvents { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Category = await categoryService.GetByIdAsync(id);
        if (Category is null)
        {
            return NotFound();
        }

        HasEvents = await categoryService.HasEventsAsync(id);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var category = await categoryService.GetByIdAsync(id);
        if (category is null)
        {
            return NotFound();
        }

        var deleted = await categoryService.DeleteAsync(id);
        if (!deleted)
        {
            TempData["ErrorMessage"] = "Cannot delete a category that has associated events.";
            return RedirectToPage("Index");
        }

        TempData["SuccessMessage"] = "Category deleted successfully.";
        return RedirectToPage("Index");
    }
}
