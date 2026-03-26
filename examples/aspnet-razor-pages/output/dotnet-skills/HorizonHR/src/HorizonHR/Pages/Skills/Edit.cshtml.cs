using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HorizonHR.Models;
using HorizonHR.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace HorizonHR.Pages.Skills;

public class EditModel : PageModel
{
    private readonly ISkillService _skillService;

    public EditModel(ISkillService skillService)
    {
        _skillService = skillService;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        public int Id { get; set; }

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

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var skill = await _skillService.GetSkillByIdAsync(id);
        if (skill == null)
        {
            return NotFound();
        }

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
        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            var skill = new Skill
            {
                Id = Input.Id,
                Name = Input.Name,
                Category = Input.Category,
                Description = Input.Description
            };

            await _skillService.UpdateSkillAsync(skill);
            TempData["SuccessMessage"] = "Skill updated successfully.";
            return RedirectToPage("Index");
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return Page();
        }
    }
}
