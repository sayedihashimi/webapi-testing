using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class VaccinationService(VetClinicDbContext db, ILogger<VaccinationService> logger)
    : IVaccinationService
{
    public async Task<VaccinationResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var vacc = await db.Vaccinations.AsNoTracking()
            .Include(v => v.Pet)
            .Include(v => v.AdministeredByVet)
            .FirstOrDefaultAsync(v => v.Id == id, ct);

        if (vacc is null) return null;

        return new VaccinationResponse(
            vacc.Id, vacc.PetId, vacc.Pet.Name, vacc.VaccineName,
            vacc.DateAdministered, vacc.ExpirationDate, vacc.BatchNumber,
            vacc.AdministeredByVetId,
            vacc.AdministeredByVet.FirstName + " " + vacc.AdministeredByVet.LastName,
            vacc.Notes,
            vacc.ExpirationDate < today,
            vacc.ExpirationDate >= today && vacc.ExpirationDate <= today.AddDays(30),
            vacc.CreatedAt);
    }

    public async Task<VaccinationResponse> CreateAsync(CreateVaccinationRequest request, CancellationToken ct)
    {
        if (!await db.Pets.AnyAsync(p => p.Id == request.PetId && p.IsActive, ct))
            throw new KeyNotFoundException($"Active pet with ID {request.PetId} not found.");

        if (!await db.Veterinarians.AnyAsync(v => v.Id == request.AdministeredByVetId, ct))
            throw new KeyNotFoundException($"Veterinarian with ID {request.AdministeredByVetId} not found.");

        if (request.ExpirationDate <= request.DateAdministered)
            throw new ArgumentException("Expiration date must be after the date administered.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var vacc = new Vaccination
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

        db.Vaccinations.Add(vacc);
        await db.SaveChangesAsync(ct);

        var pet = await db.Pets.FindAsync([vacc.PetId], ct);
        var vet = await db.Veterinarians.FindAsync([vacc.AdministeredByVetId], ct);
        logger.LogInformation("Created vaccination {VaccId} for pet {PetId}", vacc.Id, vacc.PetId);

        return new VaccinationResponse(
            vacc.Id, vacc.PetId, pet!.Name, vacc.VaccineName,
            vacc.DateAdministered, vacc.ExpirationDate, vacc.BatchNumber,
            vacc.AdministeredByVetId, vet!.FirstName + " " + vet.LastName,
            vacc.Notes,
            vacc.ExpirationDate < today,
            vacc.ExpirationDate >= today && vacc.ExpirationDate <= today.AddDays(30),
            vacc.CreatedAt);
    }
}
