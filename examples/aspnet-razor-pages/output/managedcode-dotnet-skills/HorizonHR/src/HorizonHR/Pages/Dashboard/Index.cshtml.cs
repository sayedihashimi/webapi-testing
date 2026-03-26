using HorizonHR.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages.Dashboard;

public class IndexModel(IDashboardService dashboardService) : PageModel
{
    public DashboardViewModel Data { get; set; } = new();

    public async Task OnGetAsync()
    {
        Data = await dashboardService.GetDashboardDataAsync();
    }
}
