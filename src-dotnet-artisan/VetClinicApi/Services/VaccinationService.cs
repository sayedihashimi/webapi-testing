using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class VaccinationService(VetClinicDbContext db, ILogger<VaccinationService> logger)
{
    public async Task<VaccinationResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var vax = await db.Vaccinations
            .AsNoTracking()
            .Include(v => v.Pet)
            .Include(v => v.AdministeredByVet)
            .FirstOrDefaultAsync(v => v.Id == id, ct);

        return vax is null ? null : PetService.MapToVaccinationResponse(vax);
    }

    public async Task<VaccinationResponse> CreateAsync(CreateVaccinationRequest request, CancellationToken ct)
    {
        if (!await db.Pets.AnyAsync(p => p.Id == request.PetId, ct))
        {
            throw new KeyNotFoundException($"Pet with ID {request.PetId} not found.");
        }

        if (!await db.Veterinarians.AnyAsync(v => v.Id == request.AdministeredByVetId, ct))
        {
            throw new KeyNotFoundException($"Veterinarian with ID {request.AdministeredByVetId} not found.");
        }

        if (request.ExpirationDate <= request.DateAdministered)
        {
            throw new ArgumentException("Expiration date must be after date administered.");
        }

        var vax = new Vaccination
        {
            PetId = request.PetId,
            VaccineName = request.VaccineName,
            DateAdministered = request.DateAdministered,
            ExpirationDate = request.ExpirationDate,
            BatchNumber = request.BatchNumber,
            AdministeredByVetId = request.AdministeredByVetId,
            Notes = request.Notes
        };

        db.Vaccinations.Add(vax);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Created vaccination {VaccinationId} for pet {PetId}", vax.Id, request.PetId);

        var created = await db.Vaccinations
            .AsNoTracking()
            .Include(v => v.Pet)
            .Include(v => v.AdministeredByVet)
            .FirstAsync(v => v.Id == vax.Id, ct);

        return PetService.MapToVaccinationResponse(created);
    }
}
