using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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

    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public List<LeaveBalance> Balances { get; set; } = new();
    public List<LeaveRequest> Requests { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var employee = await _employeeService.GetByIdAsync(id);
        if (employee == null) return NotFound();

        EmployeeId = id;
        EmployeeName = employee.FullName;
        Balances = await _leaveService.GetEmployeeBalancesAsync(id);
        Requests = await _leaveService.GetEmployeeRequestsAsync(id);
        return Page();
    }
}
