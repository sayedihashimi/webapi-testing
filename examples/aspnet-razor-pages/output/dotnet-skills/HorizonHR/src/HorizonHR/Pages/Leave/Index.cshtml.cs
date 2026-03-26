using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using HorizonHR.Models;
using HorizonHR.Models.Enums;
using HorizonHR.Services.Interfaces;

namespace HorizonHR.Pages.Leave;

public class IndexModel : PageModel
{
    private readonly ILeaveService _leaveService;
    private readonly IEmployeeService _employeeService;
    private readonly IDepartmentService _departmentService;

    public IndexModel(ILeaveService leaveService, IEmployeeService employeeService, IDepartmentService departmentService)
    {
        _leaveService = leaveService;
        _employeeService = employeeService;
        _departmentService = departmentService;
    }

    [BindProperty(SupportsGet = true)]
    public LeaveRequestStatus? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? EmployeeId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? LeaveTypeId { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? StartDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public DateOnly? EndDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public int pageNumber { get; set; } = 1;

    [BindProperty(SupportsGet = true)]
    public int pageSize { get; set; } = 10;

    public List<LeaveRequest> LeaveRequests { get; set; } = new();
    public List<Employee> Employees { get; set; } = new();
    public List<LeaveType> LeaveTypes { get; set; } = new();
    public PaginationModel Pagination { get; set; } = new();

    public async Task OnGetAsync()
    {
        var (items, totalCount) = await _leaveService.GetLeaveRequestsAsync(
            Status, EmployeeId, LeaveTypeId, StartDate, EndDate, pageNumber, pageSize);

        LeaveRequests = items;

        var (employees, _) = await _employeeService.GetEmployeesAsync(null, null, null, null, 1, int.MaxValue);
        Employees = employees;

        LeaveTypes = await _leaveService.GetLeaveTypesAsync();

        Pagination = new PaginationModel
        {
            CurrentPage = pageNumber,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
            PageUrl = "/Leave/Index"
        };
    }
}
