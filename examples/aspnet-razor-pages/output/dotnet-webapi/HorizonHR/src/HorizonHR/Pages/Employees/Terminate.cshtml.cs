using System.ComponentModel.DataAnnotations;
using HorizonHR.Data;
using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HorizonHR.Pages.Employees;

public sealed class TerminateModel(IEmployeeService employeeService, HorizonDbContext db) : PageModel
{
    public Employee Employee { get; set; } = null!;
    public int PendingLeaveCount { get; set; }
    public int ManagedDepartmentCount { get; set; }
    public int DirectReportCount { get; set; }

    [BindProperty, Required]
    public DateOnly TerminationDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var emp = await employeeService.GetByIdAsync(id, ct);
        if (emp == null || emp.Status == EmployeeStatus.Terminated) return NotFound();

        Employee = emp;
        PendingLeaveCount = await db.LeaveRequests.CountAsync(lr => lr.EmployeeId == id && lr.Status == LeaveRequestStatus.Submitted, ct);
        ManagedDepartmentCount = await db.Departments.CountAsync(d => d.ManagerId == id, ct);
        DirectReportCount = await db.Employees.CountAsync(e => e.ManagerId == id, ct);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int id, CancellationToken ct)
    {
        try
        {
            await employeeService.TerminateAsync(id, TerminationDate, ct);
            TempData["SuccessMessage"] = "Employee has been terminated.";
            return RedirectToPage("Details", new { id });
        }
        catch (Exception ex) when (ex is InvalidOperationException or KeyNotFoundException)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToPage("Details", new { id });
        }
    }
}
