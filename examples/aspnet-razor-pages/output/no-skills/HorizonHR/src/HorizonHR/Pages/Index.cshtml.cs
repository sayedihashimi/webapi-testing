using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages;

public class IndexModel : PageModel
{
    private readonly IEmployeeService _employeeService;
    private readonly IDepartmentService _departmentService;
    private readonly ILeaveService _leaveService;
    private readonly IReviewService _reviewService;

    public IndexModel(IEmployeeService employeeService, IDepartmentService departmentService,
        ILeaveService leaveService, IReviewService reviewService)
    {
        _employeeService = employeeService;
        _departmentService = departmentService;
        _leaveService = leaveService;
        _reviewService = reviewService;
    }

    public int TotalEmployees { get; set; }
    public int DepartmentCount { get; set; }
    public int PendingLeaveRequests { get; set; }
    public int UpcomingReviews { get; set; }
    public List<Employee> RecentHires { get; set; } = new();
    public List<Employee> OnLeaveEmployees { get; set; } = new();
    public Dictionary<string, int> DepartmentHeadcounts { get; set; } = new();

    public async Task OnGetAsync()
    {
        TotalEmployees = await _employeeService.GetTotalCountAsync();
        var departments = await _departmentService.GetAllAsync();
        DepartmentCount = departments.Count;
        PendingLeaveRequests = await _leaveService.GetPendingCountAsync();
        UpcomingReviews = await _reviewService.GetUpcomingCountAsync();
        RecentHires = await _employeeService.GetRecentHiresAsync(30);
        OnLeaveEmployees = await _employeeService.GetOnLeaveAsync();

        DepartmentHeadcounts = departments
            .Where(d => d.IsActive)
            .ToDictionary(d => d.Name, d => d.Employees.Count(e => e.Status != Models.Enums.EmployeeStatus.Terminated));
    }
}
