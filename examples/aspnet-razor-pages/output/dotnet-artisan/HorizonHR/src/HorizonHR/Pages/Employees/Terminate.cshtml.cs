using System.ComponentModel.DataAnnotations;
using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Employees;

public class TerminateModel(IEmployeeService employeeService) : PageModel
{
    public Employee Employee { get; set; } = null!;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        public int Id { get; set; }
        [Required]
        public DateOnly TerminationDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var emp = await employeeService.GetByIdAsync(id);
        if (emp is null || emp.Status == EmployeeStatus.Terminated) { return NotFound(); }
        Employee = emp;
        Input.Id = id;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var emp = await employeeService.GetByIdAsync(Input.Id);
        if (emp is null) { return NotFound(); }
        Employee = emp;

        if (!ModelState.IsValid) { return Page(); }

        await employeeService.TerminateAsync(Input.Id, Input.TerminationDate);
        TempData["Success"] = $"Employee '{emp.FullName}' has been terminated.";
        return RedirectToPage("Details", new { id = Input.Id });
    }
}
