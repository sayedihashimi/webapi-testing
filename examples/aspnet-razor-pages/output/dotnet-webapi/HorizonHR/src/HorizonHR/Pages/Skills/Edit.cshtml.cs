using System.ComponentModel.DataAnnotations;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Skills;

public sealed class EditModel(ISkillService skillService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public sealed class InputModel
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var skill = await skillService.GetByIdAsync(id, ct);
        if (skill == null) return NotFound();

        Input = new InputModel
        {
            Name = skill.Name,
            Category = skill.Category,
            Description = skill.Description
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken ct)
    {
        if (!ModelState.IsValid) return Page();

        try
        {
            await skillService.UpdateAsync(id, Input.Name, Input.Category, Input.Description, ct);
            TempData["SuccessMessage"] = "Skill updated.";
            return RedirectToPage("Index");
        }
        catch (KeyNotFoundException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return Page();
        }
    }
}
