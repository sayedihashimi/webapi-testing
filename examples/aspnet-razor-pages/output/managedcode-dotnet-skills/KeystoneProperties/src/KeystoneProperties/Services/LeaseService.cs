using KeystoneProperties.Data;
using KeystoneProperties.Models;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Services;

public class LeaseService(ApplicationDbContext context, ILogger<LeaseService> logger) : ILeaseService
{
    public async Task<PaginatedList<Lease>> GetLeasesAsync(
        LeaseStatus? status, int? propertyId, int pageNumber, int pageSize)
    {
        var query = context.Leases
            .Include(l => l.Unit)
                .ThenInclude(u => u.Property)
            .Include(l => l.Tenant)
            .AsNoTracking()
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(l => l.Status == status.Value);
        }

        if (propertyId.HasValue)
        {
            query = query.Where(l => l.Unit.PropertyId == propertyId.Value);
        }

        query = query.OrderByDescending(l => l.StartDate);

        return await PaginatedList<Lease>.CreateAsync(query, pageNumber, pageSize);
    }

    public async Task<Lease?> GetLeaseByIdAsync(int id)
    {
        return await context.Leases
            .Include(l => l.Unit)
            .Include(l => l.Tenant)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<Lease?> GetLeaseWithDetailsAsync(int id)
    {
        var lease = await context.Leases
            .Include(l => l.Unit)
                .ThenInclude(u => u.Property)
            .Include(l => l.Tenant)
            .Include(l => l.Payments.OrderByDescending(p => p.PaymentDate))
            .Include(l => l.Inspections.OrderByDescending(i => i.ScheduledDate))
            .Include(l => l.RenewalOfLease)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lease is not null)
        {
            // Load renewal chain: find any lease that renewed from this one
            var renewedLease = await context.Leases
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.RenewalOfLeaseId == id);

            // Attach as navigation isn't direct, but we loaded it for reference
        }

        return lease;
    }

    public async Task<(bool Success, string? ErrorMessage)> CreateLeaseAsync(Lease lease)
    {
        if (lease.EndDate <= lease.StartDate)
        {
            return (false, "End date must be after the start date.");
        }

        if (await HasOverlappingLeaseAsync(lease.UnitId, lease.StartDate, lease.EndDate))
        {
            return (false, "There is an overlapping active or pending lease for this unit during the specified dates.");
        }

        lease.CreatedAt = DateTime.UtcNow;
        lease.UpdatedAt = DateTime.UtcNow;
        context.Leases.Add(lease);

        if (lease.Status == LeaseStatus.Active)
        {
            var unit = await context.Units.FindAsync(lease.UnitId);
            if (unit is not null)
            {
                unit.Status = UnitStatus.Occupied;
                unit.UpdatedAt = DateTime.UtcNow;
            }
        }

        await context.SaveChangesAsync();

        logger.LogInformation("Created lease {LeaseId} for unit {UnitId}, tenant {TenantId}", lease.Id, lease.UnitId, lease.TenantId);
        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> UpdateLeaseAsync(Lease lease)
    {
        if (lease.EndDate <= lease.StartDate)
        {
            return (false, "End date must be after the start date.");
        }

        if (await HasOverlappingLeaseAsync(lease.UnitId, lease.StartDate, lease.EndDate, lease.Id))
        {
            return (false, "There is an overlapping active or pending lease for this unit during the specified dates.");
        }

        lease.UpdatedAt = DateTime.UtcNow;
        context.Leases.Update(lease);
        await context.SaveChangesAsync();

        logger.LogInformation("Updated lease {LeaseId}", lease.Id);
        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> TerminateLeaseAsync(
        int id, string reason, DepositStatus depositStatus, decimal? depositReturnAmount)
    {
        var lease = await context.Leases
            .Include(l => l.Unit)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lease is null)
        {
            return (false, "Lease not found.");
        }

        lease.Status = LeaseStatus.Terminated;
        lease.TerminationDate = DateOnly.FromDateTime(DateTime.UtcNow);
        lease.TerminationReason = reason;
        lease.DepositStatus = depositStatus;
        lease.UpdatedAt = DateTime.UtcNow;

        // Create deposit return payment if applicable
        if (depositStatus is DepositStatus.Returned or DepositStatus.PartiallyReturned && depositReturnAmount.HasValue)
        {
            var depositReturnPayment = new Payment
            {
                LeaseId = lease.Id,
                Amount = -depositReturnAmount.Value,
                PaymentDate = DateOnly.FromDateTime(DateTime.UtcNow),
                DueDate = DateOnly.FromDateTime(DateTime.UtcNow),
                PaymentMethod = PaymentMethod.BankTransfer,
                PaymentType = PaymentType.DepositReturn,
                Status = PaymentStatus.Completed,
                Notes = $"Deposit return for terminated lease. Reason: {reason}",
                CreatedAt = DateTime.UtcNow
            };
            context.Payments.Add(depositReturnPayment);
        }

        // Set unit to Available if no other active lease exists
        var hasOtherActiveLease = await context.Leases
            .AnyAsync(l => l.UnitId == lease.UnitId && l.Id != lease.Id && l.Status == LeaseStatus.Active);

        if (!hasOtherActiveLease)
        {
            lease.Unit.Status = UnitStatus.Available;
            lease.Unit.UpdatedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();

        logger.LogInformation("Terminated lease {LeaseId}. Reason: {Reason}", lease.Id, reason);
        return (true, null);
    }

    public async Task<(bool Success, string? ErrorMessage)> RenewLeaseAsync(
        int originalLeaseId, DateOnly newEndDate, decimal newMonthlyRent)
    {
        var originalLease = await context.Leases
            .Include(l => l.Unit)
            .FirstOrDefaultAsync(l => l.Id == originalLeaseId);

        if (originalLease is null)
        {
            return (false, "Original lease not found.");
        }

        var newStartDate = originalLease.EndDate.AddDays(1);

        if (newEndDate <= newStartDate)
        {
            return (false, "New end date must be after the renewal start date.");
        }

        if (await HasOverlappingLeaseAsync(originalLease.UnitId, newStartDate, newEndDate, originalLeaseId))
        {
            return (false, "There is an overlapping lease for this unit during the renewal period.");
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var newStatus = newStartDate <= today ? LeaseStatus.Active : LeaseStatus.Pending;

        var renewalLease = new Lease
        {
            UnitId = originalLease.UnitId,
            TenantId = originalLease.TenantId,
            StartDate = newStartDate,
            EndDate = newEndDate,
            MonthlyRentAmount = newMonthlyRent,
            DepositAmount = originalLease.DepositAmount,
            DepositStatus = DepositStatus.Held,
            Status = newStatus,
            RenewalOfLeaseId = originalLeaseId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        originalLease.Status = LeaseStatus.Renewed;
        originalLease.UpdatedAt = DateTime.UtcNow;

        context.Leases.Add(renewalLease);

        if (newStatus == LeaseStatus.Active)
        {
            originalLease.Unit.Status = UnitStatus.Occupied;
            originalLease.Unit.UpdatedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync();

        logger.LogInformation("Renewed lease {OriginalLeaseId} with new lease {NewLeaseId}", originalLeaseId, renewalLease.Id);
        return (true, null);
    }

    public async Task<List<Lease>> GetExpiringLeasesAsync(int days)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var cutoff = today.AddDays(days);

        return await context.Leases
            .Include(l => l.Unit)
                .ThenInclude(u => u.Property)
            .Include(l => l.Tenant)
            .Where(l => l.Status == LeaseStatus.Active && l.EndDate >= today && l.EndDate <= cutoff)
            .AsNoTracking()
            .OrderBy(l => l.EndDate)
            .ToListAsync();
    }

    public async Task<Lease?> GetActiveLeaseForUnitAsync(int unitId)
    {
        return await context.Leases
            .Include(l => l.Tenant)
            .Where(l => l.UnitId == unitId && l.Status == LeaseStatus.Active)
            .FirstOrDefaultAsync();
    }

    public async Task<bool> HasOverlappingLeaseAsync(
        int unitId, DateOnly startDate, DateOnly endDate, int? excludeLeaseId = null)
    {
        var query = context.Leases
            .Where(l => l.UnitId == unitId &&
                        (l.Status == LeaseStatus.Active || l.Status == LeaseStatus.Pending) &&
                        l.StartDate <= endDate &&
                        l.EndDate >= startDate);

        if (excludeLeaseId.HasValue)
        {
            query = query.Where(l => l.Id != excludeLeaseId.Value);
        }

        return await query.AnyAsync();
    }
}
