using Microsoft.AspNetCore.Mvc.RazorPages;
using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services.Interfaces;

namespace HorizonHR.Pages.Skills;

public class SearchModel : PageModel
{
    private readonly ISkillService _skillService;

    public SearchModel(ISkillService skillService)
    {
        _skillService = skillService;
    }

    public List<Skill> Skills { get; set; } = new();
    public List<Employee> Results { get; set; } = new();
    public bool SearchPerformed { get; set; }

    [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)]
    public int? SkillId { get; set; }

    [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)]
    public ProficiencyLevel? MinLevel { get; set; }

    public async Task OnGetAsync()
    {
        Skills = await _skillService.GetSkillsAsync();

        if (SkillId.HasValue)
        {
            Results = await _skillService.SearchEmployeesBySkillAsync(SkillId.Value, MinLevel);
            SearchPerformed = true;
        }
    }
}
