using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Employees;

public sealed class SkillsModel(ISkillService skillService, IEmployeeService employeeService) : PageModel
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public List<EmployeeSkill> EmployeeSkills { get; set; } = [];
    public List<SelectListItem> AvailableSkills { get; set; } = [];

    [BindProperty]
    public AddSkillInput AddInput { get; set; } = new();

    public sealed class AddSkillInput
    {
        public int SkillId { get; set; }
        public ProficiencyLevel ProficiencyLevel { get; set; } = ProficiencyLevel.Beginner;
        public int? YearsOfExperience { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var emp = await employeeService.GetByIdAsync(id, ct);
        if (emp == null) return NotFound();

        EmployeeId = id;
        EmployeeName = $"{emp.FirstName} {emp.LastName}";
        EmployeeSkills = await skillService.GetEmployeeSkillsAsync(id, ct);
        await LoadAvailableSkillsAsync(ct);
        return Page();
    }

    public async Task<IActionResult> OnPostAddAsync(int id, CancellationToken ct)
    {
        try
        {
            await skillService.AddEmployeeSkillAsync(id, AddInput.SkillId, AddInput.ProficiencyLevel, AddInput.YearsOfExperience, ct);
            TempData["SuccessMessage"] = "Skill added successfully.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostRemoveAsync(int id, int employeeSkillId, CancellationToken ct)
    {
        try
        {
            await skillService.RemoveEmployeeSkillAsync(employeeSkillId, ct);
            TempData["SuccessMessage"] = "Skill removed.";
        }
        catch (KeyNotFoundException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        return RedirectToPage(new { id });
    }

    private async Task LoadAvailableSkillsAsync(CancellationToken ct)
    {
        var skills = await skillService.GetAllAsync(ct);
        AvailableSkills = skills.Select(s => new SelectListItem($"{s.Name} ({s.Category})", s.Id.ToString())).ToList();
    }
}
