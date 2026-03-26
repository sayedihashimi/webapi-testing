using System.ComponentModel.DataAnnotations;
using KeystoneProperties.Models.Enums;

namespace KeystoneProperties.Models;

public class Payment
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Lease")]
    public int LeaseId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be positive")]
    [DataType(DataType.Currency)]
    public decimal Amount { get; set; }

    [Required]
    [Display(Name = "Payment Date")]
    public DateOnly PaymentDate { get; set; }

    [Required]
    [Display(Name = "Due Date")]
    public DateOnly DueDate { get; set; }

    [Required]
    [Display(Name = "Payment Method")]
    public PaymentMethod PaymentMethod { get; set; }

    [Required]
    [Display(Name = "Payment Type")]
    public PaymentType PaymentType { get; set; }

    [Required]
    public PaymentStatus Status { get; set; } = PaymentStatus.Completed;

    [MaxLength(100)]
    [Display(Name = "Reference Number")]
    public string? ReferenceNumber { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Lease Lease { get; set; } = null!;
}
