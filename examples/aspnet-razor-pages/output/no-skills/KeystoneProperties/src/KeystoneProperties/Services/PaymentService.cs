using KeystoneProperties.Data;
using KeystoneProperties.Models;
using KeystoneProperties.Models.Enums;
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

    public async Task<(List<Payment> Items, int TotalCount)> GetPaymentsAsync(
        PaymentType? type, PaymentStatus? status, DateOnly? fromDate, DateOnly? toDate,
        int? propertyId, int page, int pageSize, string? sortBy, bool descending)
    {
        var query = _context.Payments
            .Include(p => p.Lease).ThenInclude(l => l.Tenant)
            .Include(p => p.Lease).ThenInclude(l => l.Unit).ThenInclude(u => u.Property)
            .AsQueryable();

        if (type.HasValue) query = query.Where(p => p.PaymentType == type.Value);
        if (status.HasValue) query = query.Where(p => p.Status == status.Value);
        if (fromDate.HasValue) query = query.Where(p => p.PaymentDate >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(p => p.PaymentDate <= toDate.Value);
        if (propertyId.HasValue) query = query.Where(p => p.Lease.Unit.PropertyId == propertyId.Value);

        var totalCount = await query.CountAsync();

        query = (sortBy?.ToLower()) switch
        {
            "amount" => descending ? query.OrderByDescending(p => p.Amount) : query.OrderBy(p => p.Amount),
            "duedate" => descending ? query.OrderByDescending(p => p.DueDate) : query.OrderBy(p => p.DueDate),
            _ => descending ? query.OrderByDescending(p => p.PaymentDate) : query.OrderBy(p => p.PaymentDate),
        };

        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, totalCount);
    }

    public async Task<Payment?> GetByIdAsync(int id) =>
        await _context.Payments
            .Include(p => p.Lease).ThenInclude(l => l.Tenant)
            .Include(p => p.Lease).ThenInclude(l => l.Unit).ThenInclude(u => u.Property)
            .FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Payment?> GetWithDetailsAsync(int id) => await GetByIdAsync(id);

    public async Task<(bool Success, string? Error, Payment? LateFeePayment)> RecordPaymentAsync(Payment payment)
    {
        payment.CreatedAt = DateTime.UtcNow;
        _context.Payments.Add(payment);

        Payment? lateFeePayment = null;

        // Calculate late fee for rent payments
        if (payment.PaymentType == PaymentType.Rent && payment.Status == PaymentStatus.Completed)
        {
            var lateFee = CalculateLateFee(payment.PaymentDate, payment.DueDate);
            if (lateFee > 0)
            {
                lateFeePayment = new Payment
                {
                    LeaseId = payment.LeaseId,
                    Amount = lateFee,
                    PaymentDate = payment.PaymentDate,
                    DueDate = payment.DueDate,
                    PaymentMethod = payment.PaymentMethod,
                    PaymentType = PaymentType.LateFee,
                    Status = PaymentStatus.Completed,
                    Notes = $"Late fee for rent payment due {payment.DueDate:d}",
                    CreatedAt = DateTime.UtcNow
                };
                _context.Payments.Add(lateFeePayment);
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Payment recorded: ID {Id}, Amount {Amount}, Type {Type}", payment.Id, payment.Amount, payment.PaymentType);
        return (true, null, lateFeePayment);
    }

    public async Task<List<(Lease Lease, DateOnly DueDate)>> GetOverduePaymentsAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var activeLeases = await _context.Leases
            .Include(l => l.Tenant)
            .Include(l => l.Unit).ThenInclude(u => u.Property)
            .Include(l => l.Payments)
            .Where(l => l.Status == LeaseStatus.Active)
            .ToListAsync();

        var overdue = new List<(Lease, DateOnly)>();
        foreach (var lease in activeLeases)
        {
            // Check each month from start to today
            var checkDate = lease.StartDate;
            while (checkDate <= today && checkDate <= lease.EndDate)
            {
                var dueDate = new DateOnly(checkDate.Year, checkDate.Month, 1);
                if (dueDate <= today)
                {
                    var hasPaid = lease.Payments.Any(p =>
                        p.PaymentType == PaymentType.Rent &&
                        p.Status == PaymentStatus.Completed &&
                        p.DueDate.Year == dueDate.Year &&
                        p.DueDate.Month == dueDate.Month);

                    if (!hasPaid)
                        overdue.Add((lease, dueDate));
                }
                checkDate = checkDate.AddMonths(1);
            }
        }
        return overdue.OrderBy(x => x.Item2).ToList();
    }

    public async Task<decimal> GetRentCollectedThisMonthAsync()
    {
        var now = DateTime.Now;
        var startOfMonth = new DateOnly(now.Year, now.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        return await _context.Payments
            .Where(p => p.PaymentType == PaymentType.Rent &&
                        p.Status == PaymentStatus.Completed &&
                        p.PaymentDate >= startOfMonth &&
                        p.PaymentDate <= endOfMonth)
            .SumAsync(p => p.Amount);
    }

    public async Task<int> GetOverdueCountAsync()
    {
        var overdueList = await GetOverduePaymentsAsync();
        return overdueList.Count;
    }

    public decimal CalculateLateFee(DateOnly paymentDate, DateOnly dueDate)
    {
        var daysLate = paymentDate.DayNumber - dueDate.DayNumber;
        if (daysLate <= 5) return 0;

        var additionalDays = daysLate - 5;
        var fee = 50.00m + (additionalDays * 5.00m);
        return Math.Min(fee, 200.00m);
    }
}
