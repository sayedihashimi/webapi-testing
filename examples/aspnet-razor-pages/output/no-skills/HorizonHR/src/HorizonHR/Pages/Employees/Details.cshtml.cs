using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Employees;

public class DetailsModel : PageModel
{
    private readonly IEmployeeService _employeeService;

    public DetailsModel(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    public Employee? Employee { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Employee = await _employeeService.GetByIdAsync(id);
        if (Employee == null) return NotFound();
        return Page();
    }
}
