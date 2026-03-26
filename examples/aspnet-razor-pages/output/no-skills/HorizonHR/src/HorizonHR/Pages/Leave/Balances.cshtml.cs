using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Leave;

public class BalancesModel : PageModel
{
    private readonly ILeaveService _leaveService;
    private readonly IDepartmentService _departmentService;

    public BalancesModel(ILeaveService leaveService, IDepartmentService departmentService)
    {
        _leaveService = leaveService;
        _departmentService = departmentService;
    }

    public List<LeaveBalance> Balances { get; set; } = new();
    public int Year { get; set; } = DateTime.UtcNow.Year;
    public List<SelectListItem> DepartmentOptions { get; set; } = new();

    public async Task OnGetAsync(int? departmentId)
    {
        var departments = await _departmentService.GetAllAsync();
        DepartmentOptions = departments.Select(d => new SelectListItem(d.Name, d.Id.ToString())).ToList();
        Balances = await _leaveService.GetBalancesAsync(departmentId);
    }
}
