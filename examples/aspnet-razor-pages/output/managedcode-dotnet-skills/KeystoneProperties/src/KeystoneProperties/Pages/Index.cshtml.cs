using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages;

public class IndexModel(IDashboardService dashboardService) : PageModel
{
    public DashboardViewModel Dashboard { get; set; } = default!;

    public async Task OnGetAsync()
    {
        Dashboard = await dashboardService.GetDashboardDataAsync();
    }
}
