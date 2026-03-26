using KeystoneProperties.Data;
using KeystoneProperties.Models;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Services;

public sealed class LeaseService(ApplicationDbContext db, ILogger<LeaseService> logger) : ILeaseService
{
    public async Task<PaginatedList<Lease>> GetLeasesAsync(LeaseStatus? status, int? propertyId, int pageNumber, int pageSize)
    {
        var query = db.Leases
            .Include(l => l.Tenant)
            .Include(l => l.Unit).ThenInclude(u => u.Property)
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

        var totalCount = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PaginatedList<Lease>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<Lease?> GetByIdAsync(int id)
    {
        return await db.Leases
            .Include(l => l.Tenant)
            .Include(l => l.Unit).ThenInclude(u => u.Property)
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<Lease?> GetWithDetailsAsync(int id)
    {
        return await db.Leases
            .Include(l => l.Tenant)
            .Include(l => l.Unit).ThenInclude(u => u.Property)
            .Include(l => l.Payments.OrderByDescending(p => p.PaymentDate))
            .Include(l => l.Inspections)
            .Include(l => l.RenewalOfLease)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<(bool Success, string? Error)> CreateAsync(Lease lease)
    {
        // Validate no overlapping leases
        var overlapping = await db.Leases.AnyAsync(l =>
            l.UnitId == lease.UnitId &&
            (l.Status == LeaseStatus.Active || l.Status == LeaseStatus.Pending) &&
            l.StartDate < lease.EndDate && l.EndDate > lease.StartDate);

        if (overlapping)
        {
            return (false, "This unit already has an active or pending lease that overlaps with the proposed dates.");
        }

        if (lease.EndDate <= lease.StartDate)
        {
            return (false, "End date must be after start date.");
        }

        db.Leases.Add(lease);
        await db.SaveChangesAsync();

        // Update unit status if lease is active
        if (lease.Status == LeaseStatus.Active)
        {
            var unit = await db.Units.FindAsync(lease.UnitId);
            if (unit is not null)
            {
                unit.Status = UnitStatus.Occupied;
                await db.SaveChangesAsync();
            }
        }

        logger.LogInformation("Lease created: ID {LeaseId} for Unit {UnitId}, Tenant {TenantId}", lease.Id, lease.UnitId, lease.TenantId);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(Lease lease)
    {
        db.Leases.Update(lease);
        await db.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> TerminateAsync(int id, DateOnly terminationDate, string reason, DepositStatus depositStatus, decimal? depositReturnAmount)
    {
        var lease = await db.Leases.Include(l => l.Unit).FirstOrDefaultAsync(l => l.Id == id);
        if (lease is null)
        {
            return (false, "Lease not found.");
        }

        if (lease.Status is not (LeaseStatus.Active or LeaseStatus.Pending))
        {
            return (false, "Only active or pending leases can be terminated.");
        }

        lease.Status = LeaseStatus.Terminated;
        lease.TerminationDate = terminationDate;
        lease.TerminationReason = reason;
        lease.DepositStatus = depositStatus;

        // Create deposit return payment if applicable
        if (depositReturnAmount.HasValue && depositReturnAmount.Value > 0 && depositStatus is DepositStatus.Returned or DepositStatus.PartiallyReturned)
        {
            var depositReturn = new Payment
            {
                LeaseId = lease.Id,
                Amount = -depositReturnAmount.Value,
                PaymentDate = DateOnly.FromDateTime(DateTime.UtcNow),
                DueDate = DateOnly.FromDateTime(DateTime.UtcNow),
                PaymentMethod = PaymentMethod.Check,
                PaymentType = PaymentType.DepositReturn,
                Status = PaymentStatus.Completed,
                Notes = $"Deposit return - {depositStatus}"
            };
            db.Payments.Add(depositReturn);
        }

        // Update unit status if no other active leases
        var hasOtherActiveLeases = await db.Leases.AnyAsync(l => l.UnitId == lease.UnitId && l.Id != lease.Id && l.Status == LeaseStatus.Active);
        if (!hasOtherActiveLeases)
        {
            lease.Unit.Status = UnitStatus.Available;
        }

        await db.SaveChangesAsync();
        logger.LogInformation("Lease terminated: ID {LeaseId}, Reason: {Reason}", lease.Id, reason);
        return (true, null);
    }

    public async Task<(bool Success, string? Error, Lease? NewLease)> RenewAsync(int id, DateOnly newEndDate, decimal newMonthlyRent)
    {
        var lease = await db.Leases.FirstOrDefaultAsync(l => l.Id == id);
        if (lease is null)
        {
            return (false, "Lease not found.", null);
        }

        if (lease.Status != LeaseStatus.Active)
        {
            return (false, "Only active leases can be renewed.", null);
        }

        var newStartDate = lease.EndDate.AddDays(1);
        if (newEndDate <= newStartDate)
        {
            return (false, "New end date must be after the new start date.", null);
        }

        // Mark original as renewed
        lease.Status = LeaseStatus.Renewed;

        // Create new lease
        var newLease = new Lease
        {
            UnitId = lease.UnitId,
            TenantId = lease.TenantId,
            StartDate = newStartDate,
            EndDate = newEndDate,
            MonthlyRentAmount = newMonthlyRent,
            DepositAmount = lease.DepositAmount,
            DepositStatus = DepositStatus.Held,
            Status = LeaseStatus.Active,
            RenewalOfLeaseId = lease.Id
        };

        db.Leases.Add(newLease);
        await db.SaveChangesAsync();

        logger.LogInformation("Lease renewed: Original ID {OriginalId}, New ID {NewId}", lease.Id, newLease.Id);
        return (true, null, newLease);
    }

    public async Task<List<Lease>> GetExpiringLeasesAsync(int days)
    {
        var cutoff = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(days));
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return await db.Leases
            .Include(l => l.Tenant)
            .Include(l => l.Unit).ThenInclude(u => u.Property)
            .Where(l => l.Status == LeaseStatus.Active && l.EndDate >= today && l.EndDate <= cutoff)
            .AsNoTracking()
            .OrderBy(l => l.EndDate)
            .ToListAsync();
    }

    public async Task<List<Lease>> GetActiveLeasesAsync()
    {
        return await db.Leases
            .Include(l => l.Tenant)
            .Include(l => l.Unit).ThenInclude(u => u.Property)
            .Where(l => l.Status == LeaseStatus.Active)
            .AsNoTracking()
            .OrderBy(l => l.Unit.Property.Name).ThenBy(l => l.Unit.UnitNumber)
            .ToListAsync();
    }
}
