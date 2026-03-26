using System.ComponentModel.DataAnnotations;
using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Employees;

public class SkillsModel(ISkillService skillService, IEmployeeService employeeService) : PageModel
{
    public Employee Employee { get; set; } = null!;
    public List<EmployeeSkill> EmployeeSkills { get; set; } = [];
    public List<SelectListItem> AvailableSkills { get; set; } = [];

    [BindProperty] public InputModel Input { get; set; } = new();

    public class InputModel
    {
        public int EmployeeId { get; set; }
        [Required] public int SkillId { get; set; }
        [Required] public ProficiencyLevel ProficiencyLevel { get; set; }
        [Range(0, 50)] public int? YearsOfExperience { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var emp = await employeeService.GetByIdAsync(id);
        if (emp is null) { return NotFound(); }
        Employee = emp;
        Input.EmployeeId = id;
        EmployeeSkills = await skillService.GetEmployeeSkillsAsync(id);
        await LoadAvailableSkills(id);
        return Page();
    }

    public async Task<IActionResult> OnPostAddAsync()
    {
        var emp = await employeeService.GetByIdAsync(Input.EmployeeId);
        if (emp is null) { return NotFound(); }
        Employee = emp;

        if (!ModelState.IsValid)
        {
            EmployeeSkills = await skillService.GetEmployeeSkillsAsync(Input.EmployeeId);
            await LoadAvailableSkills(Input.EmployeeId);
            return Page();
        }

        var es = new EmployeeSkill
        {
            EmployeeId = Input.EmployeeId,
            SkillId = Input.SkillId,
            ProficiencyLevel = Input.ProficiencyLevel,
            YearsOfExperience = Input.YearsOfExperience,
            LastAssessedDate = DateOnly.FromDateTime(DateTime.Today)
        };
        await skillService.AddEmployeeSkillAsync(es);
        TempData["Success"] = "Skill added.";
        return RedirectToPage(new { id = Input.EmployeeId });
    }

    public async Task<IActionResult> OnPostRemoveAsync(int employeeSkillId, int id)
    {
        await skillService.RemoveEmployeeSkillAsync(employeeSkillId);
        TempData["Success"] = "Skill removed.";
        return RedirectToPage(new { id });
    }

    private async Task LoadAvailableSkills(int employeeId)
    {
        var allSkills = await skillService.GetAllAsync();
        var existingSkillIds = (await skillService.GetEmployeeSkillsAsync(employeeId)).Select(es => es.SkillId).ToHashSet();
        AvailableSkills = allSkills
            .Where(s => !existingSkillIds.Contains(s.Id))
            .Select(s => new SelectListItem(s.Name, s.Id.ToString()))
            .ToList();
    }
}
