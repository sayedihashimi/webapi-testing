using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HorizonHR.Models;
using HorizonHR.Services.Interfaces;

namespace HorizonHR.Pages.Employees;

public class TerminateModel : PageModel
{
    private readonly IEmployeeService _employeeService;

    public TerminateModel(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    public Employee Employee { get; set; } = null!;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Termination Date")]
        public DateOnly TerminationDate { get; set; }
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(id);

        if (employee is null)
        {
            return NotFound();
        }

        Employee = employee;
        Input.Id = id;
        Input.TerminationDate = DateOnly.FromDateTime(DateTime.Today);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(Input.Id);
            if (employee is null)
            {
                return NotFound();
            }
            Employee = employee;
            return Page();
        }

        try
        {
            await _employeeService.TerminateEmployeeAsync(Input.Id, Input.TerminationDate);
            TempData["SuccessMessage"] = "Employee has been terminated successfully.";
            return RedirectToPage("Details", new { id = Input.Id });
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            var employee = await _employeeService.GetEmployeeByIdAsync(Input.Id);
            if (employee is null)
            {
                return NotFound();
            }
            Employee = employee;
            return Page();
        }
    }
}
