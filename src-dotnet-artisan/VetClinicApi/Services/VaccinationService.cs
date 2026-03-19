using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class VaccinationService(VetClinicDbContext db, ILogger<VaccinationService> logger) : IVaccinationService
{
    public async Task<VaccinationDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var vaccination = await db.Vaccinations
            .Include(v => v.AdministeredByVet)
            .FirstOrDefaultAsync(v => v.Id == id, ct);

        if (vaccination is null)
        {
            return null;
        }

        return MapToDto(vaccination);
    }

    public async Task<VaccinationDto> CreateAsync(CreateVaccinationDto dto, CancellationToken ct = default)
    {
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
        await db.SaveChangesAsync(ct);

        await db.Entry(vaccination).Reference(v => v.AdministeredByVet).LoadAsync(ct);

        logger.LogInformation("Created vaccination {VaccinationId} for pet {PetId}", vaccination.Id, vaccination.PetId);

        return MapToDto(vaccination);
    }

    private static VaccinationDto MapToDto(Vaccination v) =>
        new(v.Id, v.PetId, v.VaccineName, v.DateAdministered, v.ExpirationDate,
            v.BatchNumber, v.AdministeredByVetId,
            $"{v.AdministeredByVet.FirstName} {v.AdministeredByVet.LastName}",
            v.Notes, v.CreatedAt, v.IsExpired, v.IsDueSoon);
}
