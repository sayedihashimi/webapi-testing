using KeystoneProperties.Data;
using KeystoneProperties.Models;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Services;

public sealed class LeaseService(ApplicationDbContext db, ILogger<LeaseService> logger)
    : ILeaseService
{
    public async Task<PaginatedList<Lease>> GetAllAsync(
        LeaseStatus? status, int? propertyId, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = db.Leases.AsNoTracking()
            .Include(l => l.Tenant)
            .Include(l => l.Unit)
                .ThenInclude(u => u.Property)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(l => l.Status == status.Value);
        if (propertyId.HasValue)
            query = query.Where(l => l.Unit.PropertyId == propertyId.Value);

        query = query.OrderByDescending(l => l.StartDate);
        return await PaginatedList<Lease>.CreateAsync(query, pageNumber, pageSize, ct);
    }

    public async Task<Lease?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await db.Leases
            .Include(l => l.Tenant)
            .Include(l => l.Unit)
                .ThenInclude(u => u.Property)
            .Include(l => l.Payments.OrderByDescending(p => p.PaymentDate))
            .Include(l => l.Inspections.OrderByDescending(i => i.ScheduledDate))
            .Include(l => l.RenewalOfLease)
            .Include(l => l.Renewals)
            .FirstOrDefaultAsync(l => l.Id == id, ct);
    }

    public async Task<(Lease? Lease, string? Error)> CreateAsync(Lease lease, CancellationToken ct = default)
    {
        if (lease.EndDate <= lease.StartDate)
            return (null, "End date must be after start date.");

        var hasOverlap = await HasOverlappingLeaseAsync(lease.UnitId, lease.StartDate, lease.EndDate, null, ct);
        if (hasOverlap)
            return (null, "An active or pending lease already exists for this unit during the specified date range.");

        db.Leases.Add(lease);
        await db.SaveChangesAsync(ct);

        if (lease.Status == LeaseStatus.Active)
        {
            await UpdateUnitStatusForActiveLeaseAsync(lease.UnitId, ct);
        }

        logger.LogInformation("Lease created: ID {LeaseId} for Unit {UnitId}, Tenant {TenantId}", lease.Id, lease.UnitId, lease.TenantId);
        return (lease, null);
    }

    public async Task<string?> UpdateAsync(Lease lease, CancellationToken ct = default)
    {
        db.Leases.Update(lease);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Lease updated: ID {LeaseId}", lease.Id);
        return null;
    }

    public async Task<(bool Success, string? Error)> TerminateAsync(
        int id, DateOnly terminationDate, string reason, DepositStatus depositStatus,
        decimal? depositReturnAmount, CancellationToken ct = default)
    {
        var lease = await db.Leases
            .Include(l => l.Unit)
            .FirstOrDefaultAsync(l => l.Id == id, ct);

        if (lease is null)
            return (false, "Lease not found.");

        if (lease.Status != LeaseStatus.Active && lease.Status != LeaseStatus.Pending)
            return (false, "Only active or pending leases can be terminated.");

        lease.Status = LeaseStatus.Terminated;
        lease.TerminationDate = terminationDate;
        lease.TerminationReason = reason;
        lease.DepositStatus = depositStatus;

        if (depositStatus is DepositStatus.Returned or DepositStatus.PartiallyReturned && depositReturnAmount.HasValue)
        {
            var depositReturn = new Payment
            {
                LeaseId = lease.Id,
                Amount = depositReturnAmount.Value,
                PaymentDate = DateOnly.FromDateTime(DateTime.Today),
                DueDate = DateOnly.FromDateTime(DateTime.Today),
                PaymentMethod = PaymentMethod.Check,
                PaymentType = PaymentType.DepositReturn,
                Status = PaymentStatus.Completed,
                Notes = $"Deposit return - {depositStatus}"
            };
            db.Payments.Add(depositReturn);
        }

        // Update unit status if no other active leases
        var otherActiveLeases = await db.Leases
            .AnyAsync(l => l.UnitId == lease.UnitId && l.Id != id && l.Status == LeaseStatus.Active, ct);
        if (!otherActiveLeases)
        {
            lease.Unit.Status = UnitStatus.Available;
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Lease terminated: ID {LeaseId}, Reason: {Reason}", id, reason);
        return (true, null);
    }

    public async Task<(Lease? Lease, string? Error)> RenewAsync(
        int originalLeaseId, DateOnly newEndDate, decimal newMonthlyRent, CancellationToken ct = default)
    {
        var original = await db.Leases
            .Include(l => l.Unit)
            .FirstOrDefaultAsync(l => l.Id == originalLeaseId, ct);

        if (original is null)
            return (null, "Original lease not found.");

        if (original.Status != LeaseStatus.Active)
            return (null, "Only active leases can be renewed.");

        var newStartDate = original.EndDate.AddDays(1);
        if (newEndDate <= newStartDate)
            return (null, "New end date must be after the new start date.");

        var hasOverlap = await HasOverlappingLeaseAsync(original.UnitId, newStartDate, newEndDate, originalLeaseId, ct);
        if (hasOverlap)
            return (null, "An overlapping lease exists for this unit.");

        original.Status = LeaseStatus.Renewed;

        var renewal = new Lease
        {
            UnitId = original.UnitId,
            TenantId = original.TenantId,
            StartDate = newStartDate,
            EndDate = newEndDate,
            MonthlyRentAmount = newMonthlyRent,
            DepositAmount = original.DepositAmount,
            DepositStatus = DepositStatus.Held,
            Status = LeaseStatus.Active,
            RenewalOfLeaseId = originalLeaseId
        };

        db.Leases.Add(renewal);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Lease renewed: Original {OriginalId} → New {NewId}", originalLeaseId, renewal.Id);
        return (renewal, null);
    }

    public async Task<List<Lease>> GetActiveLeasesByUnitAsync(int unitId, CancellationToken ct = default)
    {
        return await db.Leases.AsNoTracking()
            .Where(l => l.UnitId == unitId && (l.Status == LeaseStatus.Active || l.Status == LeaseStatus.Pending))
            .ToListAsync(ct);
    }

    public async Task<List<Lease>> GetActiveLeasesAsync(CancellationToken ct = default)
    {
        return await db.Leases.AsNoTracking()
            .Include(l => l.Tenant)
            .Include(l => l.Unit).ThenInclude(u => u.Property)
            .Where(l => l.Status == LeaseStatus.Active)
            .OrderBy(l => l.EndDate)
            .ToListAsync(ct);
    }

    private async Task<bool> HasOverlappingLeaseAsync(int unitId, DateOnly startDate, DateOnly endDate, int? excludeLeaseId, CancellationToken ct)
    {
        var query = db.Leases.Where(l =>
            l.UnitId == unitId &&
            (l.Status == LeaseStatus.Active || l.Status == LeaseStatus.Pending) &&
            l.StartDate <= endDate && l.EndDate >= startDate);

        if (excludeLeaseId.HasValue)
            query = query.Where(l => l.Id != excludeLeaseId.Value);

        return await query.AnyAsync(ct);
    }

    private async Task UpdateUnitStatusForActiveLeaseAsync(int unitId, CancellationToken ct)
    {
        var unit = await db.Units.FindAsync([unitId], ct);
        if (unit is not null)
        {
            unit.Status = UnitStatus.Occupied;
            await db.SaveChangesAsync(ct);
        }
    }
}
