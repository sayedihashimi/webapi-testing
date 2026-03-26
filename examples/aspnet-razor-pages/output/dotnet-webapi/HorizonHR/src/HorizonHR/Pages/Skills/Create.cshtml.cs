using System.ComponentModel.DataAnnotations;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Skills;

public sealed class CreateModel(ISkillService skillService) : PageModel
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

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        if (!ModelState.IsValid) return Page();

        await skillService.CreateAsync(Input.Name, Input.Category, Input.Description, ct);
        TempData["SuccessMessage"] = "Skill created successfully.";
        return RedirectToPage("Index");
    }
}
