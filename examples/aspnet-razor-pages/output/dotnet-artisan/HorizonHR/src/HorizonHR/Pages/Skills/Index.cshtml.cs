using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Skills;

public class IndexModel(ISkillService skillService) : PageModel
{
    public Dictionary<string, List<Skill>> SkillsByCategory { get; set; } = [];

    public async Task OnGetAsync()
    {
        SkillsByCategory = await skillService.GetGroupedByCategoryAsync();
    }
}
