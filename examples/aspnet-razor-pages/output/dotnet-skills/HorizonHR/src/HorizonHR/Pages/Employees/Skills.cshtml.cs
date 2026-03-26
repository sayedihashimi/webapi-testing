using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

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

    public Employee Employee { get; set; } = null!;
    public List<EmployeeSkill> EmployeeSkills { get; set; } = new();
    public List<Skill> AvailableSkills { get; set; } = new();

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [Display(Name = "Skill")]
        public int SkillId { get; set; }

        [Required]
        [Display(Name = "Proficiency Level")]
        public ProficiencyLevel ProficiencyLevel { get; set; }

        [Range(0, 50)]
        [Display(Name = "Years of Experience")]
        public int? YearsOfExperience { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(id);
        if (employee == null)
        {
            return NotFound();
        }

        Employee = employee;
        await LoadDataAsync(id);
        return Page();
    }

    public async Task<IActionResult> OnPostAddAsync(int id)
    {
        if (!ModelState.IsValid)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null) return NotFound();
            Employee = employee;
            await LoadDataAsync(id);
            return Page();
        }

        try
        {
            var employeeSkill = new EmployeeSkill
            {
                EmployeeId = id,
                SkillId = Input.SkillId,
                ProficiencyLevel = Input.ProficiencyLevel,
                YearsOfExperience = Input.YearsOfExperience
            };

            await _skillService.AddEmployeeSkillAsync(employeeSkill);
            TempData["SuccessMessage"] = "Skill added successfully.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToPage("/Employees/Skills", new { id });
    }

    public async Task<IActionResult> OnPostRemoveAsync(int id, int employeeSkillId)
    {
        try
        {
            await _skillService.RemoveEmployeeSkillAsync(employeeSkillId);
            TempData["SuccessMessage"] = "Skill removed successfully.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToPage("/Employees/Skills", new { id });
    }

    private async Task LoadDataAsync(int employeeId)
    {
        EmployeeSkills = await _skillService.GetEmployeeSkillsAsync(employeeId);
        var allSkills = await _skillService.GetSkillsAsync();
        var existingSkillIds = EmployeeSkills.Select(es => es.SkillId).ToHashSet();
        AvailableSkills = allSkills.Where(s => !existingSkillIds.Contains(s.Id)).ToList();
    }
}
