using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SparkEvents.Services;

namespace SparkEvents.Pages.Categories;

public sealed class EditModel(IEventCategoryService categoryService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public sealed class InputModel
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(7)]
        [Display(Name = "Color Hex")]
        public string? ColorHex { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var category = await categoryService.GetByIdAsync(id, ct);
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

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var category = await categoryService.GetByIdAsync(Input.Id, ct);
        if (category is null)
        {
            return NotFound();
        }

        if (await categoryService.NameExistsAsync(Input.Name, Input.Id, ct))
        {
            ModelState.AddModelError("Input.Name", "A category with this name already exists.");
            return Page();
        }

        category.Name = Input.Name;
        category.Description = Input.Description;
        category.ColorHex = Input.ColorHex;

        await categoryService.UpdateAsync(category, ct);

        TempData["SuccessMessage"] = "Category updated successfully.";
        return RedirectToPage("Index");
    }
}
