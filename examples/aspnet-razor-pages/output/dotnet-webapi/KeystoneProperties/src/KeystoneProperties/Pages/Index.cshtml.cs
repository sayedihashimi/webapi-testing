using KeystoneProperties.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace KeystoneProperties.Pages;

public sealed class IndexModel(IDashboardService dashboardService) : PageModel
{
    public DashboardViewModel Dashboard { get; set; } = null!;

    public async Task OnGetAsync(CancellationToken ct)
    {
        Dashboard = await dashboardService.GetDashboardAsync(ct);
    }
}
