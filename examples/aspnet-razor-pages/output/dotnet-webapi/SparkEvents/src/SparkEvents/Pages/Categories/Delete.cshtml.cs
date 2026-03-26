using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Categories;

public sealed class DeleteModel(IEventCategoryService categoryService) : PageModel
{
    public EventCategory Category { get; set; } = default!;
    public bool HasEvents { get; set; }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var category = await categoryService.GetByIdAsync(id, ct);
        if (category is null)
        {
            return NotFound();
        }

        Category = category;
        HasEvents = await categoryService.HasEventsAsync(id, ct);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken ct)
    {
        if (await categoryService.HasEventsAsync(id, ct))
        {
            TempData["ErrorMessage"] = "Cannot delete this category because it has associated events.";
            return RedirectToPage("Delete", new { id });
        }

        var deleted = await categoryService.DeleteAsync(id, ct);
        if (!deleted)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "Category deleted successfully.";
        return RedirectToPage("Index");
    }
}
