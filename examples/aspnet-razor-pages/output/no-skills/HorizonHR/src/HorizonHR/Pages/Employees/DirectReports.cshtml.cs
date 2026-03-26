using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Employees;

public class DirectReportsModel : PageModel
{
    private readonly IEmployeeService _employeeService;

    public DirectReportsModel(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    public int ManagerId { get; set; }
    public string ManagerName { get; set; } = string.Empty;
    public List<Employee> DirectReports { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var manager = await _employeeService.GetByIdAsync(id);
        if (manager == null) return NotFound();

        ManagerId = id;
        ManagerName = manager.FullName;
        DirectReports = await _employeeService.GetDirectReportsAsync(id);
        return Page();
    }
}
