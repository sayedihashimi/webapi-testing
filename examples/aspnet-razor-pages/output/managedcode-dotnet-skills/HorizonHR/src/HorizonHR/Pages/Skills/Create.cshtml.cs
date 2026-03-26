using System.ComponentModel.DataAnnotations;
using HorizonHR.Models;
using HorizonHR.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Skills;

public class CreateModel(ISkillService skillService) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        var skill = new Skill
        {
            Name = Input.Name,
            Category = Input.Category,
            Description = Input.Description
        };

        await skillService.CreateAsync(skill);
        TempData["SuccessMessage"] = $"Skill '{skill.Name}' created.";
        return RedirectToPage("Index");
    }
}
