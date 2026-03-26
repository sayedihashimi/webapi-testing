using System.ComponentModel.DataAnnotations;

namespace KeystoneProperties.Models;

public sealed class Payment
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Lease")]
    public int LeaseId { get; set; }

    [Required, Range(0.01, double.MaxValue, ErrorMessage = "Amount must be positive")]
    [DataType(DataType.Currency)]
    public decimal Amount { get; set; }

    [Required]
    [Display(Name = "Payment Date")]
    public DateOnly PaymentDate { get; set; }

    [Required]
    [Display(Name = "Due Date")]
    public DateOnly DueDate { get; set; }

    [Display(Name = "Payment Method")]
    public PaymentMethod PaymentMethod { get; set; }

    [Display(Name = "Payment Type")]
    public PaymentType PaymentType { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    [MaxLength(100)]
    [Display(Name = "Reference Number")]
    public string? ReferenceNumber { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public Lease Lease { get; set; } = null!;
}
