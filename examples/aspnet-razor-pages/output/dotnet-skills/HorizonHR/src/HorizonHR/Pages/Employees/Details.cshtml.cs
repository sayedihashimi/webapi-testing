using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HorizonHR.Models;
using HorizonHR.Services.Interfaces;

namespace HorizonHR.Pages.Employees;

public class DetailsModel : PageModel
{
    private readonly IEmployeeService _employeeService;
    private readonly ILeaveService _leaveService;
    private readonly ISkillService _skillService;

    public DetailsModel(
        IEmployeeService employeeService,
        ILeaveService leaveService,
        ISkillService skillService)
    {
        _employeeService = employeeService;
        _leaveService = leaveService;
        _skillService = skillService;
    }

    public Employee Employee { get; set; } = null!;

    public IEnumerable<LeaveBalance> LeaveBalances { get; set; } = [];

    public IEnumerable<EmployeeSkill> EmployeeSkills { get; set; } = [];

    public IEnumerable<Employee> DirectReports { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(id);

        if (employee is null)
        {
            return NotFound();
        }

        Employee = employee;
        LeaveBalances = await _leaveService.GetEmployeeLeaveBalancesAsync(id, DateTime.Now.Year);
        EmployeeSkills = await _skillService.GetEmployeeSkillsAsync(id);
        DirectReports = await _employeeService.GetDirectReportsAsync(id);

        return Page();
    }
}
