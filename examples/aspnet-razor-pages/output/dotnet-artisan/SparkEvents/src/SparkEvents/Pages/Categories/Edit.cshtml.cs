using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Data;

namespace SparkEvents.Pages.Categories;

public sealed class EditModel(SparkEventsDbContext db) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var category = await db.EventCategories.FindAsync(id);
        if (category is null)
        {
            return NotFound();
        }

        Input = new InputModel
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ColorHex = category.ColorHex
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var category = await db.EventCategories.FindAsync(Input.Id);
        if (category is null)
        {
            return NotFound();
        }

        category.Name = Input.Name;
        category.Description = Input.Description;
        category.ColorHex = Input.ColorHex;

        await db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Category \"{category.Name}\" updated successfully.";
        return RedirectToPage("Index");
    }

    public sealed class InputModel
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(7)]
        [Display(Name = "Color (hex)")]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex code (e.g. #FF5733).")]
        public string? ColorHex { get; set; }
    }
}
