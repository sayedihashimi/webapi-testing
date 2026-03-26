using System.ComponentModel.DataAnnotations;
using HorizonHR.Models;
using HorizonHR.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Employees;

public class TerminateModel(IEmployeeService employeeService) : PageModel
{
    public Employee? Employee { get; set; }

    [BindProperty, Required]
    public DateOnly TerminationDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public async Task<IActionResult> OnGetAsync(int id)
    {
        Employee = await employeeService.GetByIdAsync(id);
        if (Employee is null) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        Employee = await employeeService.GetByIdAsync(id);
        if (Employee is null) return NotFound();

        if (!ModelState.IsValid) return Page();

        try
        {
            await employeeService.TerminateAsync(id, TerminationDate);
            TempData["SuccessMessage"] = $"Employee '{Employee.FullName}' has been terminated.";
            return RedirectToPage("Details", new { id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return Page();
        }
    }
}
