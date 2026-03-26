using HorizonHR.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HorizonHR.Pages;

public sealed class IndexModel(IDashboardService dashboardService) : PageModel
{
    public DashboardStats Stats { get; set; } = null!;

    public async Task OnGetAsync(CancellationToken ct)
    {
        Stats = await dashboardService.GetStatsAsync(ct);
    }
}
