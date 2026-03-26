using System.ComponentModel.DataAnnotations;
using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Employees;

public class SkillsModel : PageModel
{
    private readonly ISkillService _skillService;
    private readonly IEmployeeService _employeeService;

    public SkillsModel(ISkillService skillService, IEmployeeService employeeService)
    {
        _skillService = skillService;
        _employeeService = employeeService;
    }

    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public List<EmployeeSkill> EmployeeSkills { get; set; } = new();
    public List<SelectListItem> SkillOptions { get; set; } = new();

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        public int SkillId { get; set; }

        [Required]
        public ProficiencyLevel ProficiencyLevel { get; set; }

        [Range(0, 50)]
        public int? YearsOfExperience { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var employee = await _employeeService.GetByIdAsync(id);
        if (employee == null) return NotFound();

        EmployeeId = id;
        EmployeeName = employee.FullName;
        await LoadDataAsync(id);
        return Page();
    }

    public async Task<IActionResult> OnPostAddAsync(int id)
    {
        EmployeeId = id;
        var employee = await _employeeService.GetByIdAsync(id);
        if (employee == null) return NotFound();
        EmployeeName = employee.FullName;

        if (!ModelState.IsValid)
        {
            await LoadDataAsync(id);
            return Page();
        }

        try
        {
            await _skillService.AddEmployeeSkillAsync(new EmployeeSkill
            {
                EmployeeId = id,
                SkillId = Input.SkillId,
                ProficiencyLevel = Input.ProficiencyLevel,
                YearsOfExperience = Input.YearsOfExperience,
                LastAssessedDate = DateOnly.FromDateTime(DateTime.Today)
            });
            TempData["Success"] = "Skill added successfully.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToPage(new { id });
    }

    public async Task<IActionResult> OnPostRemoveAsync(int id, int employeeSkillId)
    {
        await _skillService.RemoveEmployeeSkillAsync(employeeSkillId);
        TempData["Success"] = "Skill removed.";
        return RedirectToPage(new { id });
    }

    private async Task LoadDataAsync(int employeeId)
    {
        EmployeeSkills = await _skillService.GetEmployeeSkillsAsync(employeeId);
        var allSkills = await _skillService.GetAllAsync();
        var existingSkillIds = EmployeeSkills.Select(es => es.SkillId).ToHashSet();
        SkillOptions = allSkills
            .Where(s => !existingSkillIds.Contains(s.Id))
            .Select(s => new SelectListItem($"{s.Name} ({s.Category})", s.Id.ToString()))
            .ToList();
    }
}
