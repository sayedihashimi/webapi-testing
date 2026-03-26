using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Leave;

public sealed class EmployeeModel(ILeaveService leaveService, IEmployeeService employeeService) : PageModel
{
    public string EmployeeName { get; set; } = string.Empty;
    public List<LeaveBalance> Balances { get; set; } = [];
    public List<LeaveRequest> Requests { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(int id, CancellationToken ct)
    {
        var emp = await employeeService.GetByIdAsync(id, ct);
        if (emp == null) return NotFound();

        EmployeeName = $"{emp.FirstName} {emp.LastName}";
        Balances = await leaveService.GetEmployeeBalancesAsync(id, null, ct);
        Requests = await leaveService.GetEmployeeRequestsAsync(id, ct);
        return Page();
    }
}
