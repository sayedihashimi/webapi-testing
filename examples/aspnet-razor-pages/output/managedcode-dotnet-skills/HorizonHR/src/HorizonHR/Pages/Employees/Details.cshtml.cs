using HorizonHR.Models;
using HorizonHR.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Employees;

public class DetailsModel(IEmployeeService employeeService) : PageModel
{
    public Employee? Employee { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Employee = await employeeService.GetByIdAsync(id);
        if (Employee is null) return NotFound();
        return Page();
    }
}
