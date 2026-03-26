using KeystoneProperties.Data;
using KeystoneProperties.Models;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(
        ApplicationDbContext context,
        IPaymentService paymentService,
        ILogger<DashboardService> logger)
    {
        _context = context;
        _paymentService = paymentService;
        _logger = logger;
    }

    public async Task<DashboardViewModel> GetDashboardAsync()
    {
        var totalProperties = await _context.Properties.CountAsync();
        var totalUnits = await _context.Units.CountAsync();
        var occupiedUnits = await _context.Units.CountAsync(u => u.Status == UnitStatus.Occupied);

        var occupancyRate = totalUnits > 0
            ? Math.Round((decimal)occupiedUnits / totalUnits * 100, 1)
            : 0m;

        var rentCollected = await _paymentService.GetTotalCollectedThisMonthAsync();
        var overdueCount = await _paymentService.GetOverdueCountAsync();

        var openMaintenanceCount = await _context.MaintenanceRequests
            .CountAsync(m => m.Status != MaintenanceStatus.Completed
                          && m.Status != MaintenanceStatus.Cancelled);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var cutoff = today.AddDays(30);

        var expiringLeases = await _context.Leases
            .Include(l => l.Unit).ThenInclude(u => u.Property)
            .Include(l => l.Tenant)
            .AsNoTracking()
            .Where(l => l.Status == LeaseStatus.Active
                     && l.EndDate >= today
                     && l.EndDate <= cutoff)
            .OrderBy(l => l.EndDate)
            .ToListAsync();

        return new DashboardViewModel
        {
            TotalProperties = totalProperties,
            TotalUnits = totalUnits,
            OccupancyRate = occupancyRate,
            RentCollectedThisMonth = rentCollected,
            OverduePaymentsCount = overdueCount,
            OpenMaintenanceCount = openMaintenanceCount,
            ExpiringLeases = expiringLeases
        };
    }
}
