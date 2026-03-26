using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Leave;

public class IndexModel(ILeaveService leaveService, IEmployeeService employeeService) : PageModel
{
    public PaginatedList<LeaveRequest> Requests { get; set; } = null!;
    [BindProperty(SupportsGet = true)] public LeaveRequestStatus? Status { get; set; }
    [BindProperty(SupportsGet = true)] public int? EmployeeId { get; set; }
    [BindProperty(SupportsGet = true)] public int? LeaveTypeId { get; set; }
    [BindProperty(SupportsGet = true)] public int PageNumber { get; set; } = 1;
    public List<SelectListItem> Employees { get; set; } = [];
    public List<SelectListItem> LeaveTypes { get; set; } = [];

    public async Task OnGetAsync()
    {
        Requests = await leaveService.GetAllRequestsAsync(PageNumber, 10, Status, EmployeeId, LeaveTypeId);
        var emps = await employeeService.GetAllActiveAsync();
        Employees = emps.Select(e => new SelectListItem(e.FullName, e.Id.ToString())).ToList();
        var types = await leaveService.GetLeaveTypesAsync();
        LeaveTypes = types.Select(t => new SelectListItem(t.Name, t.Id.ToString())).ToList();
    }
}
