using KeystoneProperties.Data;
using KeystoneProperties.Models;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Services;

public sealed class PaymentService(ApplicationDbContext db, ILogger<PaymentService> logger)
    : IPaymentService
{
    private const decimal BaseLateFee = 50.00m;
    private const decimal DailyLateFee = 5.00m;
    private const decimal MaxLateFee = 200.00m;
    private const int GracePeriodDays = 5;

    public async Task<PaginatedList<Payment>> GetAllAsync(
        PaymentType? type, PaymentStatus? status, DateOnly? fromDate, DateOnly? toDate,
        int? propertyId, int pageNumber, int pageSize, CancellationToken ct = default)
    {
        var query = db.Payments.AsNoTracking()
            .Include(p => p.Lease)
                .ThenInclude(l => l.Tenant)
            .Include(p => p.Lease)
                .ThenInclude(l => l.Unit)
                    .ThenInclude(u => u.Property)
            .AsQueryable();

        if (type.HasValue)
            query = query.Where(p => p.PaymentType == type.Value);
        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);
        if (fromDate.HasValue)
            query = query.Where(p => p.PaymentDate >= fromDate.Value);
        if (toDate.HasValue)
            query = query.Where(p => p.PaymentDate <= toDate.Value);
        if (propertyId.HasValue)
            query = query.Where(p => p.Lease.Unit.PropertyId == propertyId.Value);

        query = query.OrderByDescending(p => p.PaymentDate).ThenByDescending(p => p.Id);
        return await PaginatedList<Payment>.CreateAsync(query, pageNumber, pageSize, ct);
    }

    public async Task<Payment?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await db.Payments
            .Include(p => p.Lease)
                .ThenInclude(l => l.Tenant)
            .Include(p => p.Lease)
                .ThenInclude(l => l.Unit)
                    .ThenInclude(u => u.Property)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<(Payment? Payment, Payment? LateFee, string? Error)> CreateAsync(
        Payment payment, CancellationToken ct = default)
    {
        var lease = await db.Leases.FindAsync([payment.LeaseId], ct);
        if (lease is null)
            return (null, null, "Lease not found.");

        db.Payments.Add(payment);

        Payment? lateFee = null;

        // Calculate late fee for rent payments
        if (payment.PaymentType == PaymentType.Rent && payment.Status == PaymentStatus.Completed)
        {
            var daysLate = payment.PaymentDate.DayNumber - payment.DueDate.DayNumber;
            if (daysLate > GracePeriodDays)
            {
                var additionalDays = daysLate - GracePeriodDays;
                var feeAmount = Math.Min(BaseLateFee + (DailyLateFee * additionalDays), MaxLateFee);

                lateFee = new Payment
                {
                    LeaseId = payment.LeaseId,
                    Amount = feeAmount,
                    PaymentDate = payment.PaymentDate,
                    DueDate = payment.DueDate,
                    PaymentMethod = payment.PaymentMethod,
                    PaymentType = PaymentType.LateFee,
                    Status = PaymentStatus.Completed,
                    Notes = $"Late fee: {daysLate} days late (${BaseLateFee} + ${DailyLateFee}×{additionalDays} additional days)"
                };
                db.Payments.Add(lateFee);
            }
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Payment recorded: {PaymentType} of {Amount:C} for Lease {LeaseId}", payment.PaymentType, payment.Amount, payment.LeaseId);
        return (payment, lateFee, null);
    }

    public async Task<List<Lease>> GetOverdueLeasePaymentsAsync(CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var activeLeases = await db.Leases.AsNoTracking()
            .Include(l => l.Tenant)
            .Include(l => l.Unit).ThenInclude(u => u.Property)
            .Include(l => l.Payments)
            .Where(l => l.Status == LeaseStatus.Active)
            .ToListAsync(ct);

        return activeLeases.Where(l =>
        {
            // Check each month from start to today
            var current = l.StartDate;
            while (current <= today && current <= l.EndDate)
            {
                var dueDate = new DateOnly(current.Year, current.Month, 1);
                if (dueDate <= today)
                {
                    var hasPaidRent = l.Payments.Any(p =>
                        p.PaymentType == PaymentType.Rent &&
                        p.Status == PaymentStatus.Completed &&
                        p.DueDate.Year == dueDate.Year &&
                        p.DueDate.Month == dueDate.Month);

                    if (!hasPaidRent)
                        return true;
                }
                current = current.AddMonths(1);
            }
            return false;
        }).ToList();
    }

    public async Task<decimal> GetRentCollectedThisMonthAsync(CancellationToken ct = default)
    {
        var firstOfMonth = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);
        var lastOfMonth = firstOfMonth.AddMonths(1).AddDays(-1);

        return await db.Payments.AsNoTracking()
            .Where(p => p.PaymentType == PaymentType.Rent &&
                        p.Status == PaymentStatus.Completed &&
                        p.PaymentDate >= firstOfMonth &&
                        p.PaymentDate <= lastOfMonth)
            .SumAsync(p => p.Amount, ct);
    }

    public async Task<int> GetOverduePaymentsCountAsync(CancellationToken ct = default)
    {
        var overdue = await GetOverdueLeasePaymentsAsync(ct);
        return overdue.Count;
    }
}
