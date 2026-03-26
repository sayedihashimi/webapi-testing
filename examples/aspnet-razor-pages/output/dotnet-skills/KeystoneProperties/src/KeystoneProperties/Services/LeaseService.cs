using KeystoneProperties.Data;
using KeystoneProperties.Models;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Services;

public class LeaseService : ILeaseService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<LeaseService> _logger;

    public LeaseService(ApplicationDbContext context, ILogger<LeaseService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaginatedList<Lease>> GetLeasesAsync(
        LeaseStatus? status, int? propertyId, string? search, int pageNumber, int pageSize)
    {
        var query = _context.Leases
            .Include(l => l.Unit).ThenInclude(u => u.Property)
            .Include(l => l.Tenant)
            .AsNoTracking()
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(l => l.Status == status.Value);

        if (propertyId.HasValue)
            query = query.Where(l => l.Unit.PropertyId == propertyId.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = $"%{search.Trim()}%";
            query = query.Where(l =>
                EF.Functions.Like(l.Tenant.FirstName, term) ||
                EF.Functions.Like(l.Tenant.LastName, term));
        }

        query = query.OrderByDescending(l => l.StartDate);

        return await PaginatedList<Lease>.CreateAsync(query, pageNumber, pageSize);
    }

    public async Task<Lease?> GetByIdAsync(int id)
    {
        return await _context.Leases
            .Include(l => l.Unit)
            .Include(l => l.Tenant)
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<Lease?> GetWithDetailsAsync(int id)
    {
        return await _context.Leases
            .Include(l => l.Unit).ThenInclude(u => u.Property)
            .Include(l => l.Tenant)
            .Include(l => l.Payments)
            .Include(l => l.Inspections)
            .Include(l => l.RenewalOfLease)
            .AsSplitQuery()
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<(bool Success, string? Error)> CreateAsync(Lease lease)
    {
        if (await HasOverlappingLeaseAsync(lease.UnitId, lease.StartDate, lease.EndDate))
            return (false, "An active or pending lease already exists for this unit during the specified dates.");

        _context.Leases.Add(lease);

        if (lease.Status == LeaseStatus.Active)
        {
            var unit = await _context.Units.FindAsync(lease.UnitId);
            if (unit != null)
            {
                unit.Status = UnitStatus.Occupied;
                unit.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Lease {LeaseId} created for unit {UnitId}", lease.Id, lease.UnitId);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(Lease lease)
    {
        var existing = await _context.Leases.FindAsync(lease.Id);
        if (existing == null)
            return (false, "Lease not found.");

        if (await HasOverlappingLeaseAsync(lease.UnitId, lease.StartDate, lease.EndDate, lease.Id))
            return (false, "An active or pending lease already exists for this unit during the specified dates.");

        existing.UnitId = lease.UnitId;
        existing.TenantId = lease.TenantId;
        existing.StartDate = lease.StartDate;
        existing.EndDate = lease.EndDate;
        existing.MonthlyRentAmount = lease.MonthlyRentAmount;
        existing.DepositAmount = lease.DepositAmount;
        existing.DepositStatus = lease.DepositStatus;
        existing.Notes = lease.Notes;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Lease {LeaseId} updated", lease.Id);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> TerminateAsync(
        int id, string reason, DepositStatus depositStatus, decimal? depositReturnAmount)
    {
        var lease = await _context.Leases
            .Include(l => l.Unit)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lease == null)
            return (false, "Lease not found.");

        if (lease.Status != LeaseStatus.Active && lease.Status != LeaseStatus.Pending)
            return (false, "Only active or pending leases can be terminated.");

        lease.Status = LeaseStatus.Terminated;
        lease.TerminationDate = DateOnly.FromDateTime(DateTime.UtcNow);
        lease.TerminationReason = reason;
        lease.DepositStatus = depositStatus;
        lease.UpdatedAt = DateTime.UtcNow;

        if ((depositStatus == DepositStatus.Returned || depositStatus == DepositStatus.PartiallyReturned)
            && depositReturnAmount.HasValue && depositReturnAmount.Value > 0)
        {
            var depositReturn = new Payment
            {
                LeaseId = lease.Id,
                Amount = -depositReturnAmount.Value,
                PaymentDate = DateOnly.FromDateTime(DateTime.UtcNow),
                DueDate = DateOnly.FromDateTime(DateTime.UtcNow),
                PaymentMethod = PaymentMethod.BankTransfer,
                PaymentType = PaymentType.DepositReturn,
                Status = PaymentStatus.Completed,
                Notes = $"Deposit return upon lease termination. Reason: {reason}",
                CreatedAt = DateTime.UtcNow
            };
            _context.Payments.Add(depositReturn);
        }

        // Check if unit should become Available
        var hasOtherActiveLease = await _context.Leases
            .AnyAsync(l => l.UnitId == lease.UnitId
                        && l.Id != lease.Id
                        && l.Status == LeaseStatus.Active);

        if (!hasOtherActiveLease)
        {
            lease.Unit.Status = UnitStatus.Available;
            lease.Unit.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Lease {LeaseId} terminated. Reason: {Reason}", id, reason);
        return (true, null);
    }

    public async Task<(bool Success, string? Error, Lease? NewLease)> RenewAsync(
        int id, DateOnly newEndDate, decimal newRentAmount)
    {
        var lease = await _context.Leases.FindAsync(id);
        if (lease == null)
            return (false, "Lease not found.", null);

        if (lease.Status != LeaseStatus.Active)
            return (false, "Only active leases can be renewed.", null);

        var newStartDate = lease.EndDate.AddDays(1);

        if (newEndDate <= newStartDate)
            return (false, "New end date must be after the new start date.", null);

        if (await HasOverlappingLeaseAsync(lease.UnitId, newStartDate, newEndDate, id))
            return (false, "An overlapping lease exists for this unit during the renewal period.", null);

        lease.Status = LeaseStatus.Renewed;
        lease.UpdatedAt = DateTime.UtcNow;

        var newLease = new Lease
        {
            UnitId = lease.UnitId,
            TenantId = lease.TenantId,
            StartDate = newStartDate,
            EndDate = newEndDate,
            MonthlyRentAmount = newRentAmount,
            DepositAmount = lease.DepositAmount,
            DepositStatus = DepositStatus.Held,
            Status = LeaseStatus.Pending,
            RenewalOfLeaseId = lease.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Leases.Add(newLease);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Lease {LeaseId} renewed. New lease {NewLeaseId} created", id, newLease.Id);
        return (true, null, newLease);
    }

    public async Task<(bool Success, string? Error)> ActivateAsync(int id)
    {
        var lease = await _context.Leases
            .Include(l => l.Unit)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (lease == null)
            return (false, "Lease not found.");

        if (lease.Status != LeaseStatus.Pending)
            return (false, "Only pending leases can be activated.");

        lease.Status = LeaseStatus.Active;
        lease.UpdatedAt = DateTime.UtcNow;

        lease.Unit.Status = UnitStatus.Occupied;
        lease.Unit.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Lease {LeaseId} activated", id);
        return (true, null);
    }

    public async Task<List<Lease>> GetExpiringLeasesAsync(int days)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var cutoff = today.AddDays(days);

        return await _context.Leases
            .Include(l => l.Unit).ThenInclude(u => u.Property)
            .Include(l => l.Tenant)
            .AsNoTracking()
            .Where(l => l.Status == LeaseStatus.Active
                     && l.EndDate >= today
                     && l.EndDate <= cutoff)
            .OrderBy(l => l.EndDate)
            .ToListAsync();
    }

    public async Task<bool> HasOverlappingLeaseAsync(
        int unitId, DateOnly startDate, DateOnly endDate, int? excludeLeaseId = null)
    {
        var query = _context.Leases
            .Where(l => l.UnitId == unitId
                     && (l.Status == LeaseStatus.Active || l.Status == LeaseStatus.Pending)
                     && l.StartDate <= endDate
                     && l.EndDate >= startDate);

        if (excludeLeaseId.HasValue)
            query = query.Where(l => l.Id != excludeLeaseId.Value);

        return await query.AnyAsync();
    }
}
