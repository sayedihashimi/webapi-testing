using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HorizonHR.Models;
using HorizonHR.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace HorizonHR.Pages.Skills;

public class CreateModel : PageModel
{
    private readonly ISkillService _skillService;

    public CreateModel(ISkillService skillService)
    {
        _skillService = skillService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [MaxLength(100)]
        [Display(Name = "Skill Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Display(Name = "Category")]
        public string Category { get; set; } = string.Empty;

        [MaxLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var skill = new Skill
            {
                Name = Input.Name,
                Category = Input.Category,
                Description = Input.Description
            };

            await _skillService.CreateSkillAsync(skill);
            TempData["SuccessMessage"] = "Skill created successfully.";
            return RedirectToPage("Index");
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return Page();
        }
    }
}
