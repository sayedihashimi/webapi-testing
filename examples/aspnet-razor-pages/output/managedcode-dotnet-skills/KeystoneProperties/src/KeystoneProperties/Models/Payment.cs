using System.ComponentModel.DataAnnotations;

namespace KeystoneProperties.Models;

public class Payment
{
    public int Id { get; set; }

    [Required]
    public int LeaseId { get; set; }

    [Required]
    public decimal Amount { get; set; }

    [Required]
    public DateOnly PaymentDate { get; set; }

    [Required]
    public DateOnly DueDate { get; set; }

    [Required]
    public PaymentMethod PaymentMethod { get; set; }

    [Required]
    public PaymentType PaymentType { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    [MaxLength(100)]
    public string? ReferenceNumber { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Lease Lease { get; set; } = null!;
}
