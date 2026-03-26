namespace HorizonHR.Models;

public class DashboardViewModel
{
    public int TotalEmployees { get; set; }
    public int TotalDepartments { get; set; }
    public int PendingLeaveRequests { get; set; }
    public int UpcomingReviews { get; set; }
    public List<Employee> RecentHires { get; set; } = [];
    public List<Employee> EmployeesOnLeave { get; set; } = [];
    public List<DepartmentHeadcount> HeadcountByDepartment { get; set; } = [];
}

public class DepartmentHeadcount
{
    public string DepartmentName { get; set; } = string.Empty;
    public int Count { get; set; }
}
