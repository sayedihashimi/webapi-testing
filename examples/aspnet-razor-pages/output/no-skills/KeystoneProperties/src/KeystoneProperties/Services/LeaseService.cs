using KeystoneProperties.Data;
using KeystoneProperties.Models;
using KeystoneProperties.Models.Enums;
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

    public async Task<(List<Lease> Items, int TotalCount)> GetLeasesAsync(
        LeaseStatus? status, int? propertyId, int page, int pageSize)
    {
        var query = _context.Leases
            .Include(l => l.Unit).ThenInclude(u => u.Property)
            .Include(l => l.Tenant)
            .AsQueryable();

        if (status.HasValue) query = query.Where(l => l.Status == status.Value);
        if (propertyId.HasValue) query = query.Where(l => l.Unit.PropertyId == propertyId.Value);

        var totalCount = await query.CountAsync();
        var items = await query.OrderByDescending(l => l.StartDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Lease?> GetByIdAsync(int id) =>
        await _context.Leases
            .Include(l => l.Unit).ThenInclude(u => u.Property)
            .Include(l => l.Tenant)
            .FirstOrDefaultAsync(l => l.Id == id);

    public async Task<Lease?> GetWithDetailsAsync(int id) =>
        await _context.Leases
            .Include(l => l.Unit).ThenInclude(u => u.Property)
            .Include(l => l.Tenant)
            .Include(l => l.Payments.OrderByDescending(p => p.PaymentDate))
            .Include(l => l.Inspections)
            .Include(l => l.RenewalOfLease)
            .FirstOrDefaultAsync(l => l.Id == id);

    public async Task<(bool Success, string? Error)> CreateAsync(Lease lease)
    {
        if (lease.EndDate <= lease.StartDate)
            return (false, "End date must be after start date.");

        if (await HasOverlappingLeaseAsync(lease.UnitId, lease.StartDate, lease.EndDate))
            return (false, "This unit already has an active or pending lease that overlaps with the specified dates.");

        lease.CreatedAt = DateTime.UtcNow;
        lease.UpdatedAt = DateTime.UtcNow;
        _context.Leases.Add(lease);

        // If lease is active, set unit to occupied
        if (lease.Status == LeaseStatus.Active)
        {
            var unit = await _context.Units.FindAsync(lease.UnitId);
            if (unit != null) unit.Status = UnitStatus.Occupied;
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Lease created: ID {Id} for Unit {UnitId}, Tenant {TenantId}", lease.Id, lease.UnitId, lease.TenantId);
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> UpdateAsync(Lease lease)
    {
        if (lease.EndDate <= lease.StartDate)
            return (false, "End date must be after start date.");

        if (await HasOverlappingLeaseAsync(lease.UnitId, lease.StartDate, lease.EndDate, lease.Id))
            return (false, "This unit already has an active or pending lease that overlaps with the specified dates.");

        lease.UpdatedAt = DateTime.UtcNow;
        _context.Leases.Update(lease);
        await _context.SaveChangesAsync();
        return (true, null);
    }

    public async Task<(bool Success, string? Error)> TerminateAsync(
        int id, string reason, DateOnly terminationDate, DepositStatus depositStatus, decimal? depositReturnAmount)
    {
        var lease = await _context.Leases.Include(l => l.Unit).FirstOrDefaultAsync(l => l.Id == id);
        if (lease == null) return (false, "Lease not found.");

        if (lease.Status != LeaseStatus.Active && lease.Status != LeaseStatus.Pending)
            return (false, "Only active or pending leases can be terminated.");

        lease.Status = LeaseStatus.Terminated;
        lease.TerminationDate = terminationDate;
        lease.TerminationReason = reason;
        lease.DepositStatus = depositStatus;
        lease.UpdatedAt = DateTime.UtcNow;

        // Create deposit return payment if applicable
        if (depositStatus == DepositStatus.Returned || depositStatus == DepositStatus.PartiallyReturned)
        {
            var returnAmount = depositReturnAmount ?? lease.DepositAmount;
            var depositPayment = new Payment
            {
                LeaseId = lease.Id,
                Amount = returnAmount,
                PaymentDate = terminationDate,
                DueDate = terminationDate,
                PaymentMethod = PaymentMethod.Check,
                PaymentType = PaymentType.DepositReturn,
                Status = PaymentStatus.Completed,
                Notes = $"Deposit {(depositStatus == DepositStatus.Returned ? "fully" : "partially")} returned upon lease termination",
                CreatedAt = DateTime.UtcNow
            };
            _context.Payments.Add(depositPayment);
        }

        // Update unit status if no other active leases
        var hasOtherActive = await _context.Leases
            .AnyAsync(l => l.UnitId == lease.UnitId && l.Id != lease.Id && l.Status == LeaseStatus.Active);
        if (!hasOtherActive)
        {
            lease.Unit.Status = UnitStatus.Available;
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Lease terminated: ID {Id}, Reason: {Reason}", id, reason);
        return (true, null);
    }

    public async Task<(bool Success, string? Error, Lease? NewLease)> RenewAsync(
        int id, DateOnly newEndDate, decimal newRentAmount)
    {
        var originalLease = await _context.Leases
            .Include(l => l.Unit)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (originalLease == null) return (false, "Lease not found.", null);
        if (originalLease.Status != LeaseStatus.Active)
            return (false, "Only active leases can be renewed.", null);

        var newStartDate = originalLease.EndDate.AddDays(1);
        if (newEndDate <= newStartDate)
            return (false, "New end date must be after the renewal start date.", null);

        // Create new lease
        var newLease = new Lease
        {
            UnitId = originalLease.UnitId,
            TenantId = originalLease.TenantId,
            StartDate = newStartDate,
            EndDate = newEndDate,
            MonthlyRentAmount = newRentAmount,
            DepositAmount = originalLease.DepositAmount,
            DepositStatus = DepositStatus.Held,
            Status = LeaseStatus.Active,
            RenewalOfLeaseId = originalLease.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Leases.Add(newLease);

        // Update original lease status
        originalLease.Status = LeaseStatus.Renewed;
        originalLease.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Lease renewed: Original ID {OrigId} → New ID {NewId}", id, newLease.Id);
        return (true, null, newLease);
    }

    public async Task<bool> HasOverlappingLeaseAsync(int unitId, DateOnly startDate, DateOnly endDate, int? excludeLeaseId = null)
    {
        var query = _context.Leases
            .Where(l => l.UnitId == unitId &&
                        (l.Status == LeaseStatus.Active || l.Status == LeaseStatus.Pending) &&
                        l.StartDate < endDate && l.EndDate > startDate);

        if (excludeLeaseId.HasValue)
            query = query.Where(l => l.Id != excludeLeaseId.Value);

        return await query.AnyAsync();
    }

    public async Task<List<Lease>> GetExpiringLeasesAsync(int days)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var futureDate = today.AddDays(days);
        return await _context.Leases
            .Include(l => l.Unit).ThenInclude(u => u.Property)
            .Include(l => l.Tenant)
            .Where(l => l.Status == LeaseStatus.Active && l.EndDate >= today && l.EndDate <= futureDate)
            .OrderBy(l => l.EndDate)
            .ToListAsync();
    }

    public async Task<Lease?> GetActiveLeaseForUnitAsync(int unitId) =>
        await _context.Leases
            .Include(l => l.Tenant)
            .FirstOrDefaultAsync(l => l.UnitId == unitId && l.Status == LeaseStatus.Active);
}
