using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Skills;

public sealed class SearchModel(ISkillService skillService) : PageModel
{
    public List<Skill> AllSkills { get; set; } = [];
    public int? SkillId { get; set; }
    public ProficiencyLevel? MinProficiency { get; set; }
    public List<Employee> Results { get; set; } = [];

    public async Task OnGetAsync(int? skillId, ProficiencyLevel? minProficiency, CancellationToken ct)
    {
        AllSkills = await skillService.GetAllAsync(ct);
        SkillId = skillId;
        MinProficiency = minProficiency;

        if (skillId.HasValue)
        {
            Results = await skillService.SearchBySkillAsync(skillId.Value, minProficiency, ct);
        }
    }
}
