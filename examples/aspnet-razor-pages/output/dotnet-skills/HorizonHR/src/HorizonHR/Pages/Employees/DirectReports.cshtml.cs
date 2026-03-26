using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HorizonHR.Models;
using HorizonHR.Services.Interfaces;

namespace HorizonHR.Pages.Employees;

public class DirectReportsModel : PageModel
{
    private readonly IEmployeeService _employeeService;

    public DirectReportsModel(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    public Employee Employee { get; set; } = null!;

    public IEnumerable<Employee> DirectReports { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(id);

        if (employee is null)
        {
            return NotFound();
        }

        Employee = employee;
        DirectReports = await _employeeService.GetDirectReportsAsync(id);

        return Page();
    }
}
