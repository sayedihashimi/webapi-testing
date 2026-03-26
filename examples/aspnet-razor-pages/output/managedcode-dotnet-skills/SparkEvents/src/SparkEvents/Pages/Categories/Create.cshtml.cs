using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Categories;

public class CreateModel(ICategoryService categoryService) : PageModel
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

        if (await categoryService.NameExistsAsync(Input.Name))
        {
            ModelState.AddModelError("Input.Name", "A category with this name already exists.");
            return Page();
        }

        var category = new EventCategory
        {
            Name = Input.Name,
            Description = Input.Description,
            ColorHex = Input.ColorHex
        };

        await categoryService.CreateAsync(category);

        TempData["SuccessMessage"] = "Category created successfully.";
        return RedirectToPage("Index");
    }

    public class InputModel
    {
        [Required]
        [MaxLength(100)]
        [Display(Name = "Name")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [MaxLength(7)]
        [Display(Name = "Color (Hex)")]
        [RegularExpression(@"^#[0-9A-Fa-f]{6}$", ErrorMessage = "Color must be a valid hex code (e.g. #FF5733).")]
        public string? ColorHex { get; set; }
    }
}
