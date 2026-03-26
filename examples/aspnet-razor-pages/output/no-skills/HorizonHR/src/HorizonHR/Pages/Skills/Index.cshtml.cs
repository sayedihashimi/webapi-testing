using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

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
        var skills = await _skillService.GetAllAsync();
        SkillsByCategory = skills.GroupBy(s => s.Category)
            .ToDictionary(g => g.Key, g => g.ToList());
    }
}
