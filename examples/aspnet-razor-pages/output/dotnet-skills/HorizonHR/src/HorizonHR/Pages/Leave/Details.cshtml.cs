using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services.Interfaces;

namespace HorizonHR.Pages.Leave;

public class DetailsModel : PageModel
{
    private readonly ILeaveService _leaveService;

    public DetailsModel(ILeaveService leaveService)
    {
        _leaveService = leaveService;
    }

    public LeaveRequest LeaveRequest { get; set; } = null!;
    public List<LeaveBalance> LeaveBalances { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var leaveRequest = await _leaveService.GetLeaveRequestByIdAsync(id);
        if (leaveRequest == null)
        {
            return NotFound();
        }

        LeaveRequest = leaveRequest;

        LeaveBalances = await _leaveService.GetEmployeeLeaveBalancesAsync(
            leaveRequest.EmployeeId, leaveRequest.StartDate.Year);

        return Page();
    }
}
