using KeystoneProperties.Data;
using KeystoneProperties.Models;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Services;

public class PaymentService(ApplicationDbContext context, ILogger<PaymentService> logger) : IPaymentService
{
    public async Task<PaginatedList<Payment>> GetPaymentsAsync(
        PaymentType? paymentType, PaymentStatus? status, DateOnly? startDate, DateOnly? endDate,
        int? propertyId, int pageNumber, int pageSize)
    {
        var query = context.Payments
            .Include(p => p.Lease)
                .ThenInclude(l => l.Tenant)
            .Include(p => p.Lease)
                .ThenInclude(l => l.Unit)
                    .ThenInclude(u => u.Property)
            .AsNoTracking()
            .AsQueryable();

        if (paymentType.HasValue)
        {
            query = query.Where(p => p.PaymentType == paymentType.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(p => p.Status == status.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(p => p.PaymentDate >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(p => p.PaymentDate <= endDate.Value);
        }

        if (propertyId.HasValue)
        {
            query = query.Where(p => p.Lease.Unit.PropertyId == propertyId.Value);
        }

        query = query.OrderByDescending(p => p.PaymentDate);

        return await PaginatedList<Payment>.CreateAsync(query, pageNumber, pageSize);
    }

    public async Task<Payment?> GetPaymentByIdAsync(int id)
    {
        return await context.Payments
            .Include(p => p.Lease)
                .ThenInclude(l => l.Tenant)
            .Include(p => p.Lease)
                .ThenInclude(l => l.Unit)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Payment?> GetPaymentWithDetailsAsync(int id)
    {
        return await context.Payments
            .Include(p => p.Lease)
                .ThenInclude(l => l.Tenant)
            .Include(p => p.Lease)
                .ThenInclude(l => l.Unit)
                    .ThenInclude(u => u.Property)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<(bool Success, string? ErrorMessage, Payment? LateFeePayment)> RecordPaymentAsync(Payment payment)
    {
        payment.CreatedAt = DateTime.UtcNow;
        context.Payments.Add(payment);

        Payment? lateFeePayment = null;

        // Late fee calculation for rent payments
        if (payment.PaymentType == PaymentType.Rent && payment.PaymentDate > payment.DueDate.AddDays(5))
        {
            var daysLate = payment.PaymentDate.DayNumber - payment.DueDate.DayNumber;
            var lateFee = Math.Min(50m + 5m * (daysLate - 5), 200m);

            lateFeePayment = new Payment
            {
                LeaseId = payment.LeaseId,
                Amount = lateFee,
                PaymentDate = payment.PaymentDate,
                DueDate = payment.DueDate,
                PaymentMethod = payment.PaymentMethod,
                PaymentType = PaymentType.LateFee,
                Status = PaymentStatus.Completed,
                Notes = $"Late fee: {daysLate} days late. Base $50 + $5/day after 5 days grace period.",
                CreatedAt = DateTime.UtcNow
            };
            context.Payments.Add(lateFeePayment);
        }

        await context.SaveChangesAsync();

        logger.LogInformation("Recorded payment {PaymentId} of {Amount:C} for lease {LeaseId}", payment.Id, payment.Amount, payment.LeaseId);

        if (lateFeePayment is not null)
        {
            logger.LogInformation("Late fee {LateFeeId} of {Amount:C} assessed for lease {LeaseId}", lateFeePayment.Id, lateFeePayment.Amount, payment.LeaseId);
        }

        return (true, null, lateFeePayment);
    }

    public async Task<List<LeasePaymentInfo>> GetOverduePaymentsAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var currentMonthDueDate = new DateOnly(today.Year, today.Month, 1);

        var activeLeases = await context.Leases
            .Include(l => l.Unit)
                .ThenInclude(u => u.Property)
            .Include(l => l.Tenant)
            .Include(l => l.Payments)
            .Where(l => l.Status == LeaseStatus.Active)
            .AsNoTracking()
            .ToListAsync();

        var overdueList = new List<LeasePaymentInfo>();

        foreach (var lease in activeLeases)
        {
            // Check if current month's rent has been paid
            var hasCompletedRentPayment = lease.Payments
                .Any(p => p.PaymentType == PaymentType.Rent &&
                          p.Status == PaymentStatus.Completed &&
                          p.DueDate == currentMonthDueDate);

            if (!hasCompletedRentPayment && currentMonthDueDate < today)
            {
                overdueList.Add(new LeasePaymentInfo
                {
                    Lease = lease,
                    DueDate = currentMonthDueDate,
                    DaysOverdue = today.DayNumber - currentMonthDueDate.DayNumber,
                    AmountDue = lease.MonthlyRentAmount
                });
            }
        }

        return overdueList;
    }

    public async Task<decimal> GetRentCollectedThisMonthAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var startOfMonth = new DateOnly(today.Year, today.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        return await context.Payments
            .Where(p => p.PaymentType == PaymentType.Rent &&
                        p.Status == PaymentStatus.Completed &&
                        p.PaymentDate >= startOfMonth &&
                        p.PaymentDate <= endOfMonth)
            .AsNoTracking()
            .SumAsync(p => p.Amount);
    }

    public async Task<int> GetOverduePaymentCountAsync()
    {
        var overduePayments = await GetOverduePaymentsAsync();
        return overduePayments.Count;
    }
}
