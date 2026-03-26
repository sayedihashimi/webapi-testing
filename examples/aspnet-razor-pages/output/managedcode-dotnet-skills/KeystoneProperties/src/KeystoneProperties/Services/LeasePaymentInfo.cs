using KeystoneProperties.Models;

namespace KeystoneProperties.Services;

public class LeasePaymentInfo
{
    public Lease Lease { get; set; } = null!;
    public DateOnly DueDate { get; set; }
    public int DaysOverdue { get; set; }
    public decimal AmountDue { get; set; }
}
