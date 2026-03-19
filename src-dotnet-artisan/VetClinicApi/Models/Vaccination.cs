using System.ComponentModel.DataAnnotations;

namespace VetClinicApi.Models;

public sealed class Vaccination
{
    public int Id { get; set; }

    public int PetId { get; set; }
    public Pet Pet { get; set; } = null!;

    [Required, MaxLength(200)]
    public string VaccineName { get; set; } = string.Empty;

    public DateOnly DateAdministered { get; set; }

    public DateOnly ExpirationDate { get; set; }

    [MaxLength(50)]
    public string? BatchNumber { get; set; }

    public int AdministeredByVetId { get; set; }
    public Veterinarian AdministeredByVet { get; set; } = null!;

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsExpired => ExpirationDate < DateOnly.FromDateTime(DateTime.UtcNow);
    public bool IsDueSoon => !IsExpired && ExpirationDate <= DateOnly.FromDateTime(DateTime.UtcNow).AddDays(30);
}
