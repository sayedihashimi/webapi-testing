using System.ComponentModel.DataAnnotations;

namespace KeystoneProperties.Models;

public sealed class Payment
{
    public int Id { get; set; }

    public int LeaseId { get; set; }
    public Lease Lease { get; set; } = null!;

    [Required, Range(0.01, double.MaxValue, ErrorMessage = "Amount must be positive.")]
    public decimal Amount { get; set; }

    [Required]
    public DateOnly PaymentDate { get; set; }

    [Required]
    public DateOnly DueDate { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public PaymentType PaymentType { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    [MaxLength(100)]
    public string? ReferenceNumber { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }
}
