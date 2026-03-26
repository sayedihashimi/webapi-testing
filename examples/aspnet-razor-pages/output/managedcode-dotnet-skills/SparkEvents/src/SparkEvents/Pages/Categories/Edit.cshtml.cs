using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Services;

namespace SparkEvents.Pages.Categories;

public class EditModel(ICategoryService categoryService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var category = await categoryService.GetByIdAsync(id);
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

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var category = await categoryService.GetByIdAsync(id);
        if (category is null)
        {
            return NotFound();
        }

        if (await categoryService.NameExistsAsync(Input.Name, id))
        {
            ModelState.AddModelError("Input.Name", "A category with this name already exists.");
            return Page();
        }

        category.Name = Input.Name;
        category.Description = Input.Description;
        category.ColorHex = Input.ColorHex;

        await categoryService.UpdateAsync(category);

        TempData["SuccessMessage"] = "Category updated successfully.";
        return RedirectToPage("Index");
    }

    public class InputModel
    {
        public int Id { get; set; }

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
