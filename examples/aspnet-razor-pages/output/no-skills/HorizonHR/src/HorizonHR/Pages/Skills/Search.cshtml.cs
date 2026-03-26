using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Skills;

public class SearchModel : PageModel
{
    private readonly ISkillService _skillService;

    public SearchModel(ISkillService skillService)
    {
        _skillService = skillService;
    }

    public int? SkillId { get; set; }
    public ProficiencyLevel? MinLevel { get; set; }
    public List<Employee> Results { get; set; } = new();
    public List<SelectListItem> SkillOptions { get; set; } = new();

    public async Task OnGetAsync(int? skillId, ProficiencyLevel? minLevel)
    {
        SkillId = skillId;
        MinLevel = minLevel;

        var skills = await _skillService.GetAllAsync();
        SkillOptions = skills.Select(s => new SelectListItem($"{s.Name} ({s.Category})", s.Id.ToString())).ToList();

        if (skillId.HasValue)
        {
            Results = await _skillService.SearchBySkillAsync(skillId.Value, minLevel);
        }
    }
}
