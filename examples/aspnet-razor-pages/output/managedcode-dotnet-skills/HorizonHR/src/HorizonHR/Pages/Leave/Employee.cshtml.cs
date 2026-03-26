using HorizonHR.Models;
using HorizonHR.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Leave;

public class EmployeeModel(ILeaveService leaveService, IEmployeeService employeeService) : PageModel
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public List<LeaveBalance> Balances { get; set; } = [];
    public List<LeaveRequest> Requests { get; set; } = [];

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var emp = await employeeService.GetByIdAsync(id);
        if (emp is null) return NotFound();

        EmployeeId = id;
        EmployeeName = emp.FullName;
        Balances = await leaveService.GetEmployeeBalancesAsync(id);
        Requests = await leaveService.GetEmployeeRequestsAsync(id);
        return Page();
    }
}
