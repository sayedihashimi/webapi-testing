using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages;

public class IndexModel(
    IEmployeeService employeeService,
    IDepartmentService departmentService,
    ILeaveService leaveService,
    IReviewService reviewService) : PageModel
{
    public int TotalEmployees { get; set; }
    public int DepartmentCount { get; set; }
    public int PendingLeaveRequests { get; set; }
    public int UpcomingReviews { get; set; }
    public List<Employee> RecentHires { get; set; } = [];
    public List<Employee> OnLeaveToday { get; set; } = [];
    public List<Department> Departments { get; set; } = [];

    public async Task OnGetAsync()
    {
        TotalEmployees = await employeeService.GetTotalCountAsync();
        var deptPage = await departmentService.GetAllAsync(1, 100);
        DepartmentCount = deptPage.TotalCount;
        Departments = deptPage.Items;
        PendingLeaveRequests = await leaveService.GetPendingCountAsync();
        UpcomingReviews = await reviewService.GetUpcomingReviewCountAsync();
        RecentHires = await employeeService.GetRecentHiresAsync(30);
        OnLeaveToday = await employeeService.GetOnLeaveAsync();
    }
}
