using KeystoneProperties.Data;
using KeystoneProperties.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Services;

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;
    private readonly IPaymentService _paymentService;

    public DashboardService(ApplicationDbContext context, IPaymentService paymentService)
    {
        _context = context;
        _paymentService = paymentService;
    }

    public async Task<DashboardViewModel> GetDashboardDataAsync()
    {
        var totalProperties = await _context.Properties.CountAsync(p => p.IsActive);
        var totalUnits = await _context.Units.CountAsync();
        var occupiedUnits = await _context.Units.CountAsync(u => u.Status == UnitStatus.Occupied);
        var occupancyRate = totalUnits > 0 ? (double)occupiedUnits / totalUnits * 100 : 0;

        var rentCollected = await _paymentService.GetRentCollectedThisMonthAsync();
        var overdueCount = await _paymentService.GetOverdueCountAsync();
        var openMaintenance = await _context.MaintenanceRequests
            .CountAsync(m => m.Status != MaintenanceStatus.Completed && m.Status != MaintenanceStatus.Cancelled);

        var today = DateOnly.FromDateTime(DateTime.Today);
        var thirtyDaysOut = today.AddDays(30);
        var expiringLeases = await _context.Leases
            .Include(l => l.Tenant)
            .Include(l => l.Unit).ThenInclude(u => u.Property)
            .Where(l => l.Status == LeaseStatus.Active && l.EndDate >= today && l.EndDate <= thirtyDaysOut)
            .OrderBy(l => l.EndDate)
            .Select(l => new LeaseExpirationItem
            {
                LeaseId = l.Id,
                TenantName = l.Tenant.FirstName + " " + l.Tenant.LastName,
                UnitInfo = l.Unit.Property.Name + " - Unit " + l.Unit.UnitNumber,
                EndDate = l.EndDate,
                DaysUntilExpiration = l.EndDate.DayNumber - today.DayNumber
            })
            .ToListAsync();

        return new DashboardViewModel
        {
            TotalProperties = totalProperties,
            TotalUnits = totalUnits,
            OccupancyRate = Math.Round(occupancyRate, 1),
            RentCollectedThisMonth = rentCollected,
            OverduePaymentsCount = overdueCount,
            OpenMaintenanceCount = openMaintenance,
            UpcomingExpirations = expiringLeases
        };
    }
}
