using KeystoneProperties.Models;

namespace KeystoneProperties.Services;

public interface IPaymentService
{
    Task<PaginatedList<Payment>> GetPaymentsAsync(PaymentType? type, PaymentStatus? status, DateOnly? fromDate, DateOnly? toDate, int? propertyId, int pageNumber, int pageSize);
    Task<Payment?> GetByIdAsync(int id);
    Task<(bool Success, string? Error)> RecordPaymentAsync(Payment payment);
    Task<decimal> GetRentCollectedThisMonthAsync();
    Task<List<Lease>> GetOverduePaymentsAsync();
    Task<int> GetOverdueCountAsync();
}
