using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Employees;

public class DirectReportsModel(IEmployeeService employeeService) : PageModel
{
    public Employee Manager { get; set; } = null!;
    public List<Employee> Reports { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var mgr = await employeeService.GetByIdAsync(id);
        if (mgr is null) { return NotFound(); }
        Manager = mgr;
        Reports = await employeeService.GetDirectReportsAsync(id);
        return Page();
    }
}
