using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Leave;

public sealed class BalancesModel(ILeaveService leaveService, IDepartmentService departmentService) : PageModel
{
    public List<LeaveBalance> Balances { get; set; } = [];
    public List<Department> Departments { get; set; } = [];
    public int? DepartmentId { get; set; }
    public int Year { get; set; } = DateTime.UtcNow.Year;

    public async Task OnGetAsync(int? departmentId, CancellationToken ct)
    {
        DepartmentId = departmentId;
        Departments = await departmentService.GetAllSimpleAsync(ct);
        Balances = await leaveService.GetBalancesAsync(departmentId, Year, ct);
    }
}
