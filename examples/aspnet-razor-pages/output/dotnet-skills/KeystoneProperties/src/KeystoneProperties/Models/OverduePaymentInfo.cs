namespace KeystoneProperties.Models;

public class OverduePaymentInfo
{
    public Lease Lease { get; set; } = null!;
    public DateOnly DueDate { get; set; }
    public decimal AmountDue { get; set; }
    public int DaysOverdue { get; set; }
}
