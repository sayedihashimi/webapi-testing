using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class VaccinationService(VetClinicDbContext db, ILogger<VaccinationService> logger) : IVaccinationService
{
    public async Task<VaccinationResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var v = await db.Vaccinations.AsNoTracking()
            .Include(v => v.Pet)
            .Include(v => v.AdministeredByVet)
            .FirstOrDefaultAsync(v => v.Id == id, ct);

        return v is null ? null : MapToResponse(v);
    }

    public async Task<VaccinationResponse> CreateAsync(CreateVaccinationRequest request, CancellationToken ct)
    {
        if (!await db.Pets.AnyAsync(p => p.Id == request.PetId && p.IsActive, ct))
            throw new KeyNotFoundException($"Active pet with ID {request.PetId} not found.");
        if (!await db.Veterinarians.AnyAsync(v => v.Id == request.AdministeredByVetId, ct))
            throw new KeyNotFoundException($"Veterinarian with ID {request.AdministeredByVetId} not found.");
        if (request.ExpirationDate <= request.DateAdministered)
            throw new ArgumentException("Expiration date must be after date administered.");

        var vaccination = new Vaccination
        {
            PetId = request.PetId,
            VaccineName = request.VaccineName,
            DateAdministered = request.DateAdministered,
            ExpirationDate = request.ExpirationDate,
            BatchNumber = request.BatchNumber,
            AdministeredByVetId = request.AdministeredByVetId,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        db.Vaccinations.Add(vaccination);
        await db.SaveChangesAsync(ct);

        var created = await db.Vaccinations.AsNoTracking()
            .Include(v => v.Pet).Include(v => v.AdministeredByVet)
            .FirstAsync(v => v.Id == vaccination.Id, ct);

        logger.LogInformation("Created vaccination {VaccinationId} for pet {PetId}", vaccination.Id, request.PetId);
        return MapToResponse(created);
    }

    private static VaccinationResponse MapToResponse(Vaccination v)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return new VaccinationResponse(
            v.Id, v.PetId, v.Pet.Name, v.VaccineName, v.DateAdministered, v.ExpirationDate,
            v.BatchNumber, v.AdministeredByVetId,
            $"{v.AdministeredByVet.FirstName} {v.AdministeredByVet.LastName}",
            v.Notes, v.ExpirationDate < today,
            v.ExpirationDate <= today.AddDays(30) && v.ExpirationDate >= today,
            v.CreatedAt);
    }
}
