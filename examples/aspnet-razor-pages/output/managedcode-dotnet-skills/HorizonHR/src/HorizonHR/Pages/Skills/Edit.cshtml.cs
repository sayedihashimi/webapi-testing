using System.ComponentModel.DataAnnotations;
using HorizonHR.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Skills;

public class EditModel(ISkillService skillService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        public int Id { get; set; }
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        [Required, MaxLength(100)]
        public string Category { get; set; } = string.Empty;
        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var skill = await skillService.GetByIdAsync(id);
        if (skill is null) return NotFound();

        Input = new InputModel
        {
            Id = skill.Id,
            Name = skill.Name,
            Category = skill.Category,
            Description = skill.Description
        };
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var skill = await skillService.GetByIdAsync(Input.Id);
        if (skill is null) return NotFound();

        skill.Name = Input.Name;
        skill.Category = Input.Category;
        skill.Description = Input.Description;

        await skillService.UpdateAsync(skill);
        TempData["SuccessMessage"] = $"Skill '{skill.Name}' updated.";
        return RedirectToPage("Index");
    }
}
