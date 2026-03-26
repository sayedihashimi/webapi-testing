using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HorizonHR.Models;
using HorizonHR.Services.Interfaces;

namespace HorizonHR.Pages.Leave;

public class EmployeeModel : PageModel
{
    private readonly ILeaveService _leaveService;
    private readonly IEmployeeService _employeeService;

    public EmployeeModel(ILeaveService leaveService, IEmployeeService employeeService)
    {
        _leaveService = leaveService;
        _employeeService = employeeService;
    }

    public Employee Employee { get; set; } = null!;
    public List<LeaveBalance> LeaveBalances { get; set; } = new();
    public List<LeaveRequest> LeaveRequests { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var employee = await _employeeService.GetEmployeeByIdAsync(id);
        if (employee == null)
        {
            return NotFound();
        }

        Employee = employee;

        var currentYear = DateTime.UtcNow.Year;
        LeaveBalances = await _leaveService.GetEmployeeLeaveBalancesAsync(id, currentYear);

        var (requests, _) = await _leaveService.GetLeaveRequestsAsync(
            null, id, null, null, null, 1, int.MaxValue);
        LeaveRequests = requests;

        return Page();
    }
}
