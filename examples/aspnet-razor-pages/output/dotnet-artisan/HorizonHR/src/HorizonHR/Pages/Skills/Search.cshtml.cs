using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Skills;

public class SearchModel(ISkillService skillService) : PageModel
{
    [BindProperty(SupportsGet = true)] public int? SkillId { get; set; }
    [BindProperty(SupportsGet = true)] public ProficiencyLevel? MinProficiency { get; set; }
    public List<SelectListItem> AllSkills { get; set; } = [];
    public List<Employee> Results { get; set; } = [];
    public bool HasSearched { get; set; }

    public async Task OnGetAsync()
    {
        var skills = await skillService.GetAllAsync();
        AllSkills = skills.Select(s => new SelectListItem(s.Name, s.Id.ToString())).ToList();

        if (SkillId.HasValue)
        {
            HasSearched = true;
            Results = await skillService.SearchBySkillAsync(SkillId.Value, MinProficiency);
        }
    }
}
