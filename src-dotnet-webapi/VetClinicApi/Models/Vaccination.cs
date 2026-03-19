namespace VetClinicApi.Models;

public sealed class Vaccination
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public required string VaccineName { get; set; }
    public DateOnly DateAdministered { get; set; }
    public DateOnly ExpirationDate { get; set; }
    public string? BatchNumber { get; set; }
    public int AdministeredByVetId { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }

    public Pet Pet { get; set; } = null!;
    public Veterinarian AdministeredByVet { get; set; } = null!;
}
