using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Skills;

public class SearchModel(ISkillService skillService) : PageModel
{
    public int? SkillId { get; set; }
    public ProficiencyLevel? MinLevel { get; set; }
    public List<EmployeeSkill> Results { get; set; } = [];
    public List<SelectListItem> SkillOptions { get; set; } = [];

    public async Task OnGetAsync(int? skillId = null, ProficiencyLevel? minLevel = null)
    {
        SkillId = skillId;
        MinLevel = minLevel;

        var skills = await skillService.GetAllAsync();
        SkillOptions = skills.Select(s => new SelectListItem(s.Name, s.Id.ToString())).ToList();

        if (skillId.HasValue)
        {
            Results = await skillService.SearchBySkillAsync(skillId.Value, minLevel);
        }
    }
}
