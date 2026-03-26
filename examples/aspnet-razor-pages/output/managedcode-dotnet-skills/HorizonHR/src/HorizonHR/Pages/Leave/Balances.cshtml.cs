using HorizonHR.Models;
using HorizonHR.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HorizonHR.Pages.Leave;

public class BalancesModel(ILeaveService leaveService, IDepartmentService departmentService) : PageModel
{
    public List<LeaveBalance> Balances { get; set; } = [];
    public int Year { get; set; }
    public List<SelectListItem> DepartmentOptions { get; set; } = [];

    public async Task OnGetAsync(int? departmentId = null)
    {
        Year = DateTime.UtcNow.Year;
        Balances = await leaveService.GetBalancesAsync(departmentId, Year);

        var depts = await departmentService.GetAllAsync();
        DepartmentOptions = depts.Select(d => new SelectListItem(d.Name, d.Id.ToString())).ToList();
    }
}
