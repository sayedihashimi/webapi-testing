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
        var vax = await _db.Vaccinations
            .Include(v => v.Pet)
            .Include(v => v.AdministeredByVet)
            .FirstOrDefaultAsync(v => v.Id == id);
        return vax?.ToResponseDto();
    }

    public async Task<(VaccinationResponseDto? Result, string? Error)> CreateAsync(VaccinationCreateDto dto)
    {
        var pet = await _db.Pets.FindAsync(dto.PetId);
        if (pet is null) return (null, "Pet not found.");

        var vet = await _db.Veterinarians.FindAsync(dto.AdministeredByVetId);
        if (vet is null) return (null, "Veterinarian not found.");

        if (dto.ExpirationDate <= dto.DateAdministered)
            return (null, "ExpirationDate must be after DateAdministered.");

        var vax = new Vaccination
        {
            PetId = dto.PetId,
            VaccineName = dto.VaccineName,
            DateAdministered = dto.DateAdministered,
            ExpirationDate = dto.ExpirationDate,
            BatchNumber = dto.BatchNumber,
            AdministeredByVetId = dto.AdministeredByVetId,
            Notes = dto.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _db.Vaccinations.Add(vax);
        await _db.SaveChangesAsync();

        await _db.Entry(vax).Reference(v => v.Pet).LoadAsync();
        await _db.Entry(vax).Reference(v => v.AdministeredByVet).LoadAsync();

        _logger.LogInformation("Vaccination recorded: {VaxId} {Vaccine} for pet {PetId}", vax.Id, vax.VaccineName, vax.PetId);
        return (vax.ToResponseDto(), null);
    }
}
