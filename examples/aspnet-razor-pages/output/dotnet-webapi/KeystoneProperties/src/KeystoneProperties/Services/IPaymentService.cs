using KeystoneProperties.Models;

namespace KeystoneProperties.Services;

public interface IPaymentService
{
    Task<PaginatedList<Payment>> GetAllAsync(PaymentType? type, PaymentStatus? status, DateOnly? fromDate, DateOnly? toDate, int? propertyId, int pageNumber, int pageSize, CancellationToken ct = default);
    Task<Payment?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<(Payment? Payment, Payment? LateFee, string? Error)> CreateAsync(Payment payment, CancellationToken ct = default);
    Task<List<Lease>> GetOverdueLeasePaymentsAsync(CancellationToken ct = default);
    Task<decimal> GetRentCollectedThisMonthAsync(CancellationToken ct = default);
    Task<int> GetOverduePaymentsCountAsync(CancellationToken ct = default);
}
