using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Data;
using SparkEvents.Models;

namespace SparkEvents.Pages.Categories;

public sealed class CreateModel(SparkEventsDbContext db) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var category = new EventCategory
        {
            Name = Input.Name,
            Description = Input.Description,
            ColorHex = Input.ColorHex
        };

        db.EventCategories.Add(category);
        await db.SaveChangesAsync();

        TempData["SuccessMessage"] = $"Category \"{category.Name}\" created successfully.";
        return RedirectToPage("Index");
    }

    public sealed class InputModel
    {
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
