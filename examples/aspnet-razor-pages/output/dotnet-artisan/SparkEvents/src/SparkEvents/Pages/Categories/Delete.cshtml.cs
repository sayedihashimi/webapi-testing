using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Pages.Categories;

public sealed class DeleteModel(SparkEventsDbContext db) : PageModel
{
    public EventCategory Category { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var category = await db.EventCategories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category is null)
        {
            return NotFound();
        }

        Category = category;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        var category = await db.EventCategories
            .Include(c => c.Events)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category is null)
        {
            return NotFound();
        }

        if (category.Events.Count > 0)
        {
            TempData["ErrorMessage"] = $"Cannot delete \"{category.Name}\" because it has {category.Events.Count} event(s) associated with it.";
            return RedirectToPage("Index");
        }

        db.EventCategories.Remove(category);
        await db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Category \"{category.Name}\" deleted successfully.";
        return RedirectToPage("Index");
    }
}
