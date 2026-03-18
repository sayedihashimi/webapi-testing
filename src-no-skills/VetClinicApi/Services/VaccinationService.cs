using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class VaccinationService : IVaccinationService
{
    private readonly VetClinicDbContext _db;
    private readonly ILogger<VaccinationService> _logger;

    public VaccinationService(VetClinicDbContext db, ILogger<VaccinationService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<VaccinationResponseDto?> GetByIdAsync(int id)
    {
        var v = await _db.Vaccinations
            .Include(v => v.Pet).Include(v => v.AdministeredByVet)
            .FirstOrDefaultAsync(v => v.Id == id);
        return v == null ? null : MapToResponse(v);
    }

    public async Task<VaccinationResponseDto> CreateAsync(CreateVaccinationDto dto)
    {
        if (!await _db.Pets.AnyAsync(p => p.Id == dto.PetId))
            throw new KeyNotFoundException($"Pet with ID {dto.PetId} not found.");
        if (!await _db.Veterinarians.AnyAsync(v => v.Id == dto.AdministeredByVetId))
            throw new KeyNotFoundException($"Veterinarian with ID {dto.AdministeredByVetId} not found.");

        if (dto.ExpirationDate <= dto.DateAdministered)
            throw new ArgumentException("Expiration date must be after the date administered.");

        var vaccination = new Vaccination
        {
            PetId = dto.PetId, VaccineName = dto.VaccineName,
            DateAdministered = dto.DateAdministered, ExpirationDate = dto.ExpirationDate,
            BatchNumber = dto.BatchNumber, AdministeredByVetId = dto.AdministeredByVetId,
            Notes = dto.Notes, CreatedAt = DateTime.UtcNow
        };

        _db.Vaccinations.Add(vaccination);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Vaccination recorded: {VaccinationId} for Pet {PetId}", vaccination.Id, vaccination.PetId);

        return (await GetByIdAsync(vaccination.Id))!;
    }

    public static VaccinationResponseDto MapToResponse(Vaccination v) => new()
    {
        Id = v.Id, PetId = v.PetId, VaccineName = v.VaccineName,
        DateAdministered = v.DateAdministered, ExpirationDate = v.ExpirationDate,
        BatchNumber = v.BatchNumber, AdministeredByVetId = v.AdministeredByVetId,
        Notes = v.Notes, IsExpired = v.IsExpired, IsDueSoon = v.IsDueSoon, CreatedAt = v.CreatedAt,
        Pet = v.Pet != null ? new PetSummaryDto { Id = v.Pet.Id, Name = v.Pet.Name, Species = v.Pet.Species, Breed = v.Pet.Breed, IsActive = v.Pet.IsActive } : null,
        AdministeredByVet = v.AdministeredByVet != null ? new VeterinarianSummaryDto { Id = v.AdministeredByVet.Id, FirstName = v.AdministeredByVet.FirstName, LastName = v.AdministeredByVet.LastName, Specialization = v.AdministeredByVet.Specialization } : null
    };
}
