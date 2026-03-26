using KeystoneProperties.Data;
using KeystoneProperties.Models;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Services;

public class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PaymentService> _logger;

    public PaymentService(ApplicationDbContext context, ILogger<PaymentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaginatedList<Payment>> GetPaymentsAsync(
        PaymentType? type, PaymentStatus? status, DateOnly? fromDate, DateOnly? toDate,
        int? propertyId, int pageNumber, int pageSize)
    {
        var query = _context.Payments
            .Include(p => p.Lease).ThenInclude(l => l.Unit).ThenInclude(u => u.Property)
            .Include(p => p.Lease).ThenInclude(l => l.Tenant)
            .AsNoTracking()
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

        query = query.OrderByDescending(p => p.PaymentDate);

        return await PaginatedList<Payment>.CreateAsync(query, pageNumber, pageSize);
    }

    public async Task<Payment?> GetByIdAsync(int id)
    {
        return await _context.Payments
            .Include(p => p.Lease)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Payment?> GetWithDetailsAsync(int id)
    {
        return await _context.Payments
            .Include(p => p.Lease).ThenInclude(l => l.Unit).ThenInclude(u => u.Property)
            .Include(p => p.Lease).ThenInclude(l => l.Tenant)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<(bool Success, string? Error, Payment? LateFeePayment)> RecordPaymentAsync(Payment payment)
    {
        var lease = await _context.Leases.FindAsync(payment.LeaseId);
        if (lease == null)
            return (false, "Lease not found.", null);

        _context.Payments.Add(payment);

        Payment? lateFeePayment = null;

        // Late fee calculation for rent payments
        if (payment.PaymentType == PaymentType.Rent)
        {
            int daysLate = payment.PaymentDate.DayNumber - payment.DueDate.DayNumber;
            if (daysLate > 5)
            {
                decimal lateFee = Math.Min(50m + (daysLate - 5) * 5m, 200m);

                lateFeePayment = new Payment
                {
                    LeaseId = payment.LeaseId,
                    Amount = lateFee,
                    PaymentDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    DueDate = payment.DueDate,
                    PaymentMethod = payment.PaymentMethod,
                    PaymentType = PaymentType.LateFee,
                    Status = PaymentStatus.Pending,
                    Notes = $"Late fee: payment received {daysLate} days after due date.",
                    CreatedAt = DateTime.UtcNow
                };
                _context.Payments.Add(lateFeePayment);

                _logger.LogInformation(
                    "Late fee of {LateFee:C} assessed on lease {LeaseId} ({DaysLate} days late)",
                    lateFee, payment.LeaseId, daysLate);
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Payment {PaymentId} recorded for lease {LeaseId}", payment.Id, payment.LeaseId);
        return (true, null, lateFeePayment);
    }

    public async Task<List<OverduePaymentInfo>> GetOverduePaymentsAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var activeLeases = await _context.Leases
            .Include(l => l.Unit).ThenInclude(u => u.Property)
            .Include(l => l.Tenant)
            .Include(l => l.Payments)
            .AsNoTracking()
            .Where(l => l.Status == LeaseStatus.Active)
            .ToListAsync();

        var overdueList = new List<OverduePaymentInfo>();

        foreach (var lease in activeLeases)
        {
            // Generate all monthly due dates from StartDate up to today
            var dueDate = new DateOnly(lease.StartDate.Year, lease.StartDate.Month, lease.StartDate.Day);

            while (dueDate <= today)
            {
                // Check if a Completed Rent payment exists for this due date
                bool isPaid = lease.Payments.Any(p =>
                    p.PaymentType == PaymentType.Rent
                    && p.Status == PaymentStatus.Completed
                    && p.DueDate == dueDate);

                if (!isPaid && dueDate < today)
                {
                    int daysOverdue = today.DayNumber - dueDate.DayNumber;
                    overdueList.Add(new OverduePaymentInfo
                    {
                        Lease = lease,
                        DueDate = dueDate,
                        AmountDue = lease.MonthlyRentAmount,
                        DaysOverdue = daysOverdue
                    });
                }

                dueDate = dueDate.AddMonths(1);
            }
        }

        return overdueList.OrderByDescending(o => o.DaysOverdue).ToList();
    }

    public async Task<decimal> GetTotalCollectedThisMonthAsync()
    {
        var now = DateTime.UtcNow;
        var firstOfMonth = new DateOnly(now.Year, now.Month, 1);
        var lastOfMonth = firstOfMonth.AddMonths(1).AddDays(-1);

        return await _context.Payments
            .AsNoTracking()
            .Where(p => p.Status == PaymentStatus.Completed
                     && p.PaymentDate >= firstOfMonth
                     && p.PaymentDate <= lastOfMonth)
            .SumAsync(p => p.Amount);
    }

    public async Task<int> GetOverdueCountAsync()
    {
        var overduePayments = await GetOverduePaymentsAsync();
        return overduePayments.Count;
    }
}
