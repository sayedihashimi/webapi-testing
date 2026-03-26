namespace KeystoneProperties.Services;

public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardDataAsync();
}
