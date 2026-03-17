using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class VaccinationService(VetClinicDbContext db, ILogger<VaccinationService> logger) : IVaccinationService
{
    public async Task<VaccinationDto?> GetByIdAsync(int id)
    {
        return await db.Vaccinations
            .Include(v => v.Pet).Include(v => v.AdministeredByVet)
            .Where(v => v.Id == id)
            .Select(v => new VaccinationDto(v.Id, v.PetId, v.Pet.Name, v.VaccineName, v.DateAdministered, v.ExpirationDate,
                v.BatchNumber, v.AdministeredByVetId,
                v.AdministeredByVet.FirstName + " " + v.AdministeredByVet.LastName,
                v.Notes, v.IsExpired, v.IsDueSoon, v.CreatedAt))
            .FirstOrDefaultAsync();
    }

    public async Task<VaccinationDto> CreateAsync(CreateVaccinationDto dto)
    {
        if (dto.ExpirationDate <= dto.DateAdministered)
            throw new ArgumentException("Expiration date must be after date administered.");

        if (!await db.Pets.AnyAsync(p => p.Id == dto.PetId && p.IsActive))
            throw new InvalidOperationException("Active pet not found.");

        if (!await db.Veterinarians.AnyAsync(v => v.Id == dto.AdministeredByVetId))
            throw new InvalidOperationException("Veterinarian not found.");

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

        db.Vaccinations.Add(vaccination);
        await db.SaveChangesAsync();

        await db.Entry(vaccination).Reference(v => v.Pet).LoadAsync();
        await db.Entry(vaccination).Reference(v => v.AdministeredByVet).LoadAsync();

        logger.LogInformation("Created vaccination {VaccinationId} for pet {PetId}", vaccination.Id, dto.PetId);

        return new VaccinationDto(
            vaccination.Id, vaccination.PetId, vaccination.Pet.Name, vaccination.VaccineName,
            vaccination.DateAdministered, vaccination.ExpirationDate, vaccination.BatchNumber,
            vaccination.AdministeredByVetId, vaccination.AdministeredByVet.FirstName + " " + vaccination.AdministeredByVet.LastName,
            vaccination.Notes, vaccination.IsExpired, vaccination.IsDueSoon, vaccination.CreatedAt);
    }
}
