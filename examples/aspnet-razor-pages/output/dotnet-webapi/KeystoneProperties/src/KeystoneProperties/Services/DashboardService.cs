using KeystoneProperties.Data;
using KeystoneProperties.Models;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Services;

public sealed class DashboardService(
    ApplicationDbContext db,
    IPaymentService paymentService,
    IMaintenanceService maintenanceService)
    : IDashboardService
{
    public async Task<DashboardViewModel> GetDashboardAsync(CancellationToken ct = default)
    {
        var totalProperties = await db.Properties.AsNoTracking().CountAsync(p => p.IsActive, ct);
        var totalUnits = await db.Units.AsNoTracking().CountAsync(ct);
        var occupiedUnits = await db.Units.AsNoTracking().CountAsync(u => u.Status == UnitStatus.Occupied, ct);
        var occupancyRate = totalUnits > 0 ? (decimal)occupiedUnits / totalUnits * 100 : 0;

        var rentCollected = await paymentService.GetRentCollectedThisMonthAsync(ct);
        var overdueCount = await paymentService.GetOverduePaymentsCountAsync(ct);
        var openMaintenance = await maintenanceService.GetOpenRequestCountAsync(ct);

        var today = DateOnly.FromDateTime(DateTime.Today);
        var thirtyDaysOut = today.AddDays(30);

        var upcomingExpirations = await db.Leases.AsNoTracking()
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
            .ToListAsync(ct);

        return new DashboardViewModel
        {
            TotalProperties = totalProperties,
            TotalUnits = totalUnits,
            OccupancyRate = Math.Round(occupancyRate, 1),
            RentCollectedThisMonth = rentCollected,
            OverduePaymentsCount = overdueCount,
            OpenMaintenanceRequests = openMaintenance,
            UpcomingLeaseExpirations = upcomingExpirations
        };
    }
}
