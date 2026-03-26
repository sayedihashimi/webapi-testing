using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Employees;

public sealed class DirectReportsModel(IEmployeeService employeeService) : PageModel
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public List<Employee> DirectReports { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var emp = await employeeService.GetByIdAsync(id, ct);
        if (emp == null) return NotFound();

        EmployeeId = id;
        EmployeeName = $"{emp.FirstName} {emp.LastName}";
        DirectReports = await employeeService.GetDirectReportsAsync(id, ct);
        return Page();
    }
}
