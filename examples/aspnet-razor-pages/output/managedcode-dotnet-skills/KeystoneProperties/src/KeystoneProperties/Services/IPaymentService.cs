using KeystoneProperties.Models;

namespace KeystoneProperties.Services;

public interface IPaymentService
{
    Task<PaginatedList<Payment>> GetPaymentsAsync(PaymentType? paymentType, PaymentStatus? status, DateOnly? startDate, DateOnly? endDate, int? propertyId, int pageNumber, int pageSize);
    Task<Payment?> GetPaymentByIdAsync(int id);
    Task<Payment?> GetPaymentWithDetailsAsync(int id);
    Task<(bool Success, string? ErrorMessage, Payment? LateFeePayment)> RecordPaymentAsync(Payment payment);
    Task<List<LeasePaymentInfo>> GetOverduePaymentsAsync();
    Task<decimal> GetRentCollectedThisMonthAsync();
    Task<int> GetOverduePaymentCountAsync();
}
