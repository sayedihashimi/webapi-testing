using HorizonHR.Models;
using HorizonHR.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Skills;

public class IndexModel(ISkillService skillService) : PageModel
{
    public List<Skill> Skills { get; set; } = [];

    public async Task OnGetAsync()
    {
        Skills = await skillService.GetAllAsync();
    }
}
