using KeystoneProperties.Models;

namespace KeystoneProperties.Services;

public interface IPaymentService
{
    Task<PaginatedList<Payment>> GetPaymentsAsync(PaymentType? type, PaymentStatus? status, DateOnly? fromDate, DateOnly? toDate, int? propertyId, int pageNumber, int pageSize);
    Task<Payment?> GetByIdAsync(int id);
    Task<Payment?> GetWithDetailsAsync(int id);
    Task<(bool Success, string? Error, Payment? LateFeePayment)> RecordPaymentAsync(Payment payment);
    Task<List<OverduePaymentInfo>> GetOverduePaymentsAsync();
    Task<decimal> GetTotalCollectedThisMonthAsync();
    Task<int> GetOverdueCountAsync();
}
