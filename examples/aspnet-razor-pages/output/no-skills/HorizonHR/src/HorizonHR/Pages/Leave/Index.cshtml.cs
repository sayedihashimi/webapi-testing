using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Leave;

public class IndexModel : PageModel
{
    private readonly ILeaveService _leaveService;
    private readonly IEmployeeService _employeeService;

    public IndexModel(ILeaveService leaveService, IEmployeeService employeeService)
    {
        _leaveService = leaveService;
        _employeeService = employeeService;
    }

    public List<LeaveRequest> LeaveRequests { get; set; } = new();
    public LeaveRequestStatus? StatusFilter { get; set; }
    public int? EmployeeFilter { get; set; }
    public int? LeaveTypeFilter { get; set; }
    public int PageNumber { get; set; } = 1;
    public int TotalPages { get; set; }
    public List<SelectListItem> EmployeeOptions { get; set; } = new();
    public List<SelectListItem> LeaveTypeOptions { get; set; } = new();

    public async Task OnGetAsync(LeaveRequestStatus? status, int? employeeId, int? leaveTypeId, int pageNumber = 1)
    {
        StatusFilter = status;
        EmployeeFilter = employeeId;
        LeaveTypeFilter = leaveTypeId;
        PageNumber = pageNumber;

        var employees = await _employeeService.GetAllActiveAsync();
        EmployeeOptions = employees.Select(e => new SelectListItem(e.FullName, e.Id.ToString())).ToList();

        var leaveTypes = await _leaveService.GetLeaveTypesAsync();
        LeaveTypeOptions = leaveTypes.Select(lt => new SelectListItem(lt.Name, lt.Id.ToString())).ToList();

        var (items, totalCount) = await _leaveService.GetPagedRequestsAsync(pageNumber, 10, status, employeeId, leaveTypeId);
        LeaveRequests = items;
        TotalPages = (int)Math.Ceiling(totalCount / 10.0);
    }
}
