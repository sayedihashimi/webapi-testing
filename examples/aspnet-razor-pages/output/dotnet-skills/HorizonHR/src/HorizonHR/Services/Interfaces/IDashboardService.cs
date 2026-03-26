using HorizonHR.Models;

namespace HorizonHR.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardViewModel> GetDashboardDataAsync();
}
