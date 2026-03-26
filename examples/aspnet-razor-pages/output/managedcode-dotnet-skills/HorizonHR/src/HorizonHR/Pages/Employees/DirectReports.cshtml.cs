using HorizonHR.Models;
using HorizonHR.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Employees;

public class DirectReportsModel(IEmployeeService employeeService) : PageModel
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public List<Employee> DirectReports { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var employee = await employeeService.GetByIdAsync(id);
        if (employee is null) return NotFound();

        EmployeeId = id;
        EmployeeName = employee.FullName;
        DirectReports = await employeeService.GetDirectReportsAsync(id);
        return Page();
    }
}
