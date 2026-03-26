using KeystoneProperties.Data;
using KeystoneProperties.Models;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Services;

public class DashboardService(ApplicationDbContext context) : IDashboardService
{
    public async Task<DashboardViewModel> GetDashboardDataAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var startOfMonth = new DateOnly(today.Year, today.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        var totalProperties = await context.Properties.AsNoTracking().CountAsync();
        var totalUnits = await context.Units.AsNoTracking().CountAsync();
        var occupiedUnits = await context.Units.AsNoTracking().CountAsync(u => u.Status == UnitStatus.Occupied);

        var occupancyRate = totalUnits > 0 ? (decimal)occupiedUnits / totalUnits * 100 : 0;

        var rentCollected = await context.Payments
            .Where(p => p.PaymentType == PaymentType.Rent &&
                        p.Status == PaymentStatus.Completed &&
                        p.PaymentDate >= startOfMonth &&
                        p.PaymentDate <= endOfMonth)
            .AsNoTracking()
            .SumAsync(p => p.Amount);

        var currentMonthDueDate = new DateOnly(today.Year, today.Month, 1);
        var overdueCount = await context.Leases
            .Where(l => l.Status == LeaseStatus.Active && currentMonthDueDate < today)
            .Where(l => !l.Payments.Any(p =>
                p.PaymentType == PaymentType.Rent &&
                p.Status == PaymentStatus.Completed &&
                p.DueDate == currentMonthDueDate))
            .AsNoTracking()
            .CountAsync();

        var openMaintenanceRequests = await context.MaintenanceRequests
            .CountAsync(mr => mr.Status != MaintenanceStatus.Completed && mr.Status != MaintenanceStatus.Cancelled);

        var expiringLeases = await context.Leases
            .Include(l => l.Unit)
                .ThenInclude(u => u.Property)
            .Include(l => l.Tenant)
            .Where(l => l.Status == LeaseStatus.Active && l.EndDate >= today && l.EndDate <= today.AddDays(30))
            .AsNoTracking()
            .OrderBy(l => l.EndDate)
            .ToListAsync();

        return new DashboardViewModel
        {
            TotalProperties = totalProperties,
            TotalUnits = totalUnits,
            OccupiedUnits = occupiedUnits,
            OccupancyRate = occupancyRate,
            RentCollectedThisMonth = rentCollected,
            OverduePaymentsCount = overdueCount,
            OpenMaintenanceRequests = openMaintenanceRequests,
            ExpiringLeases = expiringLeases
        };
    }
}
