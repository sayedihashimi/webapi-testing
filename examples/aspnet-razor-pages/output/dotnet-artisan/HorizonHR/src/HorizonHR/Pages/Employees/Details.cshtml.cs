using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Employees;

public class DetailsModel(IEmployeeService employeeService) : PageModel
{
    public Employee Employee { get; set; } = null!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var emp = await employeeService.GetByIdAsync(id);
        if (emp is null) { return NotFound(); }
        Employee = emp;
        return Page();
    }
}
