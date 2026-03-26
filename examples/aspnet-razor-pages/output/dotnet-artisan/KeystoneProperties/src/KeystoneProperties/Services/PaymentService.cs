using KeystoneProperties.Data;
using KeystoneProperties.Models;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Services;

public sealed class PaymentService(ApplicationDbContext db, ILogger<PaymentService> logger) : IPaymentService
{
    public async Task<PaginatedList<Payment>> GetPaymentsAsync(PaymentType? type, PaymentStatus? status, DateOnly? fromDate, DateOnly? toDate, int? propertyId, int pageNumber, int pageSize)
    {
        var query = db.Payments
            .Include(p => p.Lease).ThenInclude(l => l.Tenant)
            .Include(p => p.Lease).ThenInclude(l => l.Unit).ThenInclude(u => u.Property)
            .AsNoTracking()
            .AsQueryable();

        if (type.HasValue)
        {
            query = query.Where(p => p.PaymentType == type.Value);
        }
        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }
        if (fromDate.HasValue)
        {
            query = query.Where(p => p.PaymentDate >= fromDate.Value);
        }
        if (toDate.HasValue)
        {
            query = query.Where(p => p.PaymentDate <= toDate.Value);
        }
        if (propertyId.HasValue)
        {
            query = query.Where(p => p.Lease.Unit.PropertyId == propertyId.Value);
        }

        query = query.OrderByDescending(p => p.PaymentDate).ThenByDescending(p => p.Id);

        var totalCount = await query.CountAsync();
        var items = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PaginatedList<Payment>(items, totalCount, pageNumber, pageSize);
    }

    public async Task<Payment?> GetByIdAsync(int id)
    {
        return await db.Payments
            .Include(p => p.Lease).ThenInclude(l => l.Tenant)
            .Include(p => p.Lease).ThenInclude(l => l.Unit).ThenInclude(u => u.Property)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<(bool Success, string? Error)> RecordPaymentAsync(Payment payment)
    {
        db.Payments.Add(payment);
        await db.SaveChangesAsync();

        // Calculate late fee if rent payment is late
        if (payment.PaymentType == PaymentType.Rent && payment.Status == PaymentStatus.Completed)
        {
            var daysLate = payment.PaymentDate.DayNumber - payment.DueDate.DayNumber;
            if (daysLate > 5)
            {
                int additionalDays = daysLate - 5;
                decimal lateFee = Math.Min(50m + additionalDays * 5m, 200m);

                var lateFeePayment = new Payment
                {
                    LeaseId = payment.LeaseId,
                    Amount = lateFee,
                    PaymentDate = payment.PaymentDate,
                    DueDate = payment.DueDate,
                    PaymentMethod = payment.PaymentMethod,
                    PaymentType = PaymentType.LateFee,
                    Status = PaymentStatus.Completed,
                    Notes = $"Late fee: {daysLate} days late (${50} + {additionalDays} × $5, capped at $200)"
                };
                db.Payments.Add(lateFeePayment);
                await db.SaveChangesAsync();

                logger.LogInformation("Late fee of {LateFee:C} generated for lease {LeaseId}", lateFee, payment.LeaseId);
            }
        }

        logger.LogInformation("Payment recorded: {PaymentType} of {Amount:C} for Lease {LeaseId}", payment.PaymentType, payment.Amount, payment.LeaseId);
        return (true, null);
    }

    public async Task<decimal> GetRentCollectedThisMonthAsync()
    {
        var firstOfMonth = new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var lastOfMonth = firstOfMonth.AddMonths(1).AddDays(-1);

        return await db.Payments
            .Where(p => p.PaymentType == PaymentType.Rent &&
                        p.Status == PaymentStatus.Completed &&
                        p.PaymentDate >= firstOfMonth &&
                        p.PaymentDate <= lastOfMonth)
            .SumAsync(p => p.Amount);
    }

    public async Task<List<Lease>> GetOverduePaymentsAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var activeLeases = await db.Leases
            .Include(l => l.Tenant)
            .Include(l => l.Unit).ThenInclude(u => u.Property)
            .Include(l => l.Payments)
            .Where(l => l.Status == LeaseStatus.Active)
            .AsNoTracking()
            .ToListAsync();

        // Find leases with overdue rent
        return activeLeases.Where(l =>
        {
            var currentMonth = new DateOnly(today.Year, today.Month, 1);
            var dueDate = new DateOnly(today.Year, today.Month, l.StartDate.Day > 28 ? 28 : l.StartDate.Day);
            if (dueDate > today)
            {
                return false;
            }

            var hasPaymentThisMonth = l.Payments.Any(p =>
                p.PaymentType == PaymentType.Rent &&
                p.Status == PaymentStatus.Completed &&
                p.DueDate.Month == today.Month && p.DueDate.Year == today.Year);

            return !hasPaymentThisMonth;
        }).ToList();
    }

    public async Task<int> GetOverdueCountAsync()
    {
        var overdue = await GetOverduePaymentsAsync();
        return overdue.Count;
    }
}
