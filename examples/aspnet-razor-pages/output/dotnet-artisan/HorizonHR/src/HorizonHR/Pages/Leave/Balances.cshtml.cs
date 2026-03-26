using HorizonHR.Models;
using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Leave;

public class BalancesModel(ILeaveService leaveService, IDepartmentService departmentService) : PageModel
{
    public List<LeaveBalance> Balances { get; set; } = [];
    [BindProperty(SupportsGet = true)] public int? DepartmentId { get; set; }
    public List<SelectListItem> Departments { get; set; } = [];

    public async Task OnGetAsync()
    {
        var depts = await departmentService.GetAllActiveAsync();
        Departments = depts.Select(d => new SelectListItem(d.Name, d.Id.ToString())).ToList();
        Balances = await leaveService.GetBalancesAsync(DepartmentId);
    }
}
