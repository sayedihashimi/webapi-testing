using System.ComponentModel.DataAnnotations;
using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Employees;

public class TerminateModel : PageModel
{
    private readonly IEmployeeService _employeeService;

    public TerminateModel(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    public Employee? Employee { get; set; }

    [BindProperty, Required]
    public DateOnly TerminationDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Employee = await _employeeService.GetByIdAsync(id);
        if (Employee == null) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        Employee = await _employeeService.GetByIdAsync(id);
        if (Employee == null) return NotFound();

        if (!ModelState.IsValid) return Page();

        try
        {
            await _employeeService.TerminateAsync(id, TerminationDate);
            TempData["Success"] = $"Employee {Employee.FullName} has been terminated.";
            return RedirectToPage("Details", new { id });
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return Page();
        }
    }
}
