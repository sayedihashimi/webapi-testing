using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Skills;

public sealed class IndexModel(ISkillService skillService) : PageModel
{
    public Dictionary<string, List<Skill>> SkillsByCategory { get; set; } = [];

    public async Task OnGetAsync(CancellationToken ct)
    {
        var skills = await skillService.GetAllAsync(ct);
        SkillsByCategory = skills.GroupBy(s => s.Category).ToDictionary(g => g.Key, g => g.ToList());
    }
}
