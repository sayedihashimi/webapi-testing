using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs.Vaccination;
using VetClinicApi.Models;
using VetClinicApi.Services.Interfaces;

namespace VetClinicApi.Services.Implementations;

public class VaccinationService : IVaccinationService
{
    private readonly VetClinicDbContext _db;
    private readonly ILogger<VaccinationService> _logger;

    public VaccinationService(VetClinicDbContext db, ILogger<VaccinationService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<VaccinationDto> GetByIdAsync(int id)
    {
        var vaccination = await _db.Vaccinations
            .Include(v => v.Pet).Include(v => v.AdministeredByVet)
            .FirstOrDefaultAsync(v => v.Id == id)
            ?? throw new KeyNotFoundException($"Vaccination with ID {id} not found");

        return MapToDto(vaccination);
    }

    public async Task<VaccinationDto> CreateAsync(CreateVaccinationDto dto)
    {
        if (!await _db.Pets.AnyAsync(p => p.Id == dto.PetId && p.IsActive))
            throw new KeyNotFoundException($"Active pet with ID {dto.PetId} not found");

        if (!await _db.Veterinarians.AnyAsync(v => v.Id == dto.AdministeredByVetId))
            throw new KeyNotFoundException($"Veterinarian with ID {dto.AdministeredByVetId} not found");

        if (dto.ExpirationDate <= dto.DateAdministered)
            throw new ArgumentException("Expiration date must be after the date administered");

        var vaccination = new Vaccination
        {
            PetId = dto.PetId,
            VaccineName = dto.VaccineName,
            DateAdministered = dto.DateAdministered,
            ExpirationDate = dto.ExpirationDate,
            BatchNumber = dto.BatchNumber,
            AdministeredByVetId = dto.AdministeredByVetId,
            Notes = dto.Notes
        };

        _db.Vaccinations.Add(vaccination);
        await _db.SaveChangesAsync();

        await _db.Entry(vaccination).Reference(v => v.Pet).LoadAsync();
        await _db.Entry(vaccination).Reference(v => v.AdministeredByVet).LoadAsync();
        _logger.LogInformation("Created vaccination {VaccId} for pet {PetId}", vaccination.Id, dto.PetId);
        return MapToDto(vaccination);
    }

    private static VaccinationDto MapToDto(Vaccination v) => new()
    {
        Id = v.Id, PetId = v.PetId, PetName = v.Pet.Name,
        VaccineName = v.VaccineName, DateAdministered = v.DateAdministered,
        ExpirationDate = v.ExpirationDate, BatchNumber = v.BatchNumber,
        AdministeredByVetId = v.AdministeredByVetId,
        AdministeredByVetName = v.AdministeredByVet.FirstName + " " + v.AdministeredByVet.LastName,
        Notes = v.Notes, IsExpired = v.IsExpired, IsDueSoon = v.IsDueSoon, CreatedAt = v.CreatedAt
    };
}
