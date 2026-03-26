using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Employees;

public class SkillsModel(ISkillService skillService, IEmployeeService employeeService) : PageModel
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public List<EmployeeSkill> EmployeeSkills { get; set; } = [];
    public List<SelectListItem> AvailableSkills { get; set; } = [];

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        public int SkillId { get; set; }
        public ProficiencyLevel ProficiencyLevel { get; set; }
        public int? YearsOfExperience { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var emp = await employeeService.GetByIdAsync(id);
        if (emp is null) return NotFound();

        EmployeeId = id;
        EmployeeName = emp.FullName;
        await LoadDataAsync(id);
        return Page();
    }

    public async Task<IActionResult> OnPostAddAsync(int id)
    {
        try
        {
            var es = new EmployeeSkill
            {
                EmployeeId = id,
                SkillId = Input.SkillId,
                ProficiencyLevel = Input.ProficiencyLevel,
                YearsOfExperience = Input.YearsOfExperience,
                LastAssessedDate = DateOnly.FromDateTime(DateTime.UtcNow)
            };
            await skillService.AddEmployeeSkillAsync(es);
            TempData["SuccessMessage"] = "Skill added.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostRemoveAsync(int id, int skillId)
    {
        await skillService.RemoveEmployeeSkillAsync(id, skillId);
        TempData["SuccessMessage"] = "Skill removed.";
        return RedirectToPage(new { id });
    }

    private async Task LoadDataAsync(int employeeId)
    {
        EmployeeSkills = await skillService.GetEmployeeSkillsAsync(employeeId);
        var allSkills = await skillService.GetAllAsync();
        var existingIds = EmployeeSkills.Select(es => es.SkillId).ToHashSet();
        AvailableSkills = allSkills
            .Where(s => !existingIds.Contains(s.Id))
            .Select(s => new SelectListItem(s.Name, s.Id.ToString()))
            .ToList();
    }
}
