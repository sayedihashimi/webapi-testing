namespace VetClinicApi.Models;

public class Vaccination
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public string VaccineName { get; set; } = string.Empty;
    public DateOnly DateAdministered { get; set; }
    public DateOnly ExpirationDate { get; set; }
    public string? BatchNumber { get; set; }
    public int AdministeredByVetId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    public bool IsExpired => ExpirationDate < DateOnly.FromDateTime(DateTime.UtcNow);
    public bool IsDueSoon => !IsExpired && ExpirationDate <= DateOnly.FromDateTime(DateTime.UtcNow).AddDays(30);

    public Pet Pet { get; set; } = null!;
    public Veterinarian AdministeredByVet { get; set; } = null!;
}
