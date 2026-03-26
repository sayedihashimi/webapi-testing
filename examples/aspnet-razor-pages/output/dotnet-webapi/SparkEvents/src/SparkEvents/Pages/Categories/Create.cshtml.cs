using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Models;
using SparkEvents.Services;

namespace SparkEvents.Pages.Categories;

public sealed class CreateModel(IEventCategoryService categoryService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public sealed class InputModel
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(7)]
        [Display(Name = "Color Hex")]
        public string? ColorHex { get; set; }
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (await categoryService.NameExistsAsync(Input.Name, ct: ct))
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

        await categoryService.CreateAsync(category, ct);

        TempData["SuccessMessage"] = "Category created successfully.";
        return RedirectToPage("Index");
    }
}
