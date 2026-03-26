using Microsoft.AspNetCore.Mvc.RazorPages;
using HorizonHR.Models;
using HorizonHR.Services.Interfaces;

namespace HorizonHR.Pages.Skills;

public class IndexModel : PageModel
{
    private readonly ISkillService _skillService;

    public IndexModel(ISkillService skillService)
    {
        _skillService = skillService;
    }

    public Dictionary<string, List<Skill>> SkillsByCategory { get; set; } = new();

    public async Task OnGetAsync()
    {
        var skills = await _skillService.GetSkillsAsync();
        SkillsByCategory = skills
            .GroupBy(s => s.Category)
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.ToList());
    }
}
