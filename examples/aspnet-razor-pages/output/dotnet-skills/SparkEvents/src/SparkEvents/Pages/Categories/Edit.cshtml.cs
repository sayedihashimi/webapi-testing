using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Services;

namespace SparkEvents.Pages.Categories;

public class EditModel : PageModel
{
    private readonly ICategoryService _categoryService;
    public EditModel(ICategoryService categoryService) => _categoryService = categoryService;

    [BindProperty]
    public InputModel Input { get; set; } = new();
    public int Id { get; set; }

    public class InputModel
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        [MaxLength(500)]
        public string? Description { get; set; }
        [MaxLength(7)]
        public string? ColorHex { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var category = await _categoryService.GetCategoryByIdAsync(id);
        if (category == null) return NotFound();
        Id = id;
        Input = new InputModel { Name = category.Name, Description = category.Description, ColorHex = category.ColorHex };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (!ModelState.IsValid) { Id = id; return Page(); }
        var category = await _categoryService.GetCategoryByIdAsync(id);
        if (category == null) return NotFound();
        category.Name = Input.Name;
        category.Description = Input.Description;
        category.ColorHex = Input.ColorHex;
        await _categoryService.UpdateCategoryAsync(category);
        TempData["SuccessMessage"] = "Category updated successfully.";
        return RedirectToPage("Index");
    }
}
