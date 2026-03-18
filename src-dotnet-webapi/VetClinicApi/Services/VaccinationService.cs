using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public interface IVaccinationService
{
    Task<VaccinationResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<VaccinationResponse> CreateAsync(CreateVaccinationRequest request, CancellationToken ct);
}

public class VaccinationService(VetClinicDbContext db, ILogger<VaccinationService> logger) : IVaccinationService
{
    public async Task<VaccinationResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await db.Vaccinations.AsNoTracking()
            .Include(v => v.Pet)
            .Include(v => v.AdministeredBy)
            .Where(v => v.Id == id)
            .Select(v => new VaccinationResponse(v.Id, v.PetId, v.Pet.Name, v.VaccineName,
                v.DateAdministered, v.ExpirationDate, v.BatchNumber, v.AdministeredByVetId,
                $"{v.AdministeredBy.FirstName} {v.AdministeredBy.LastName}",
                v.Notes, v.ExpirationDate < today, v.ExpirationDate >= today && v.ExpirationDate <= today.AddDays(30),
                v.CreatedAt))
            .FirstOrDefaultAsync(ct);
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

        var pet = await db.Pets.FindAsync([vaccination.PetId], ct);
        var vet = await db.Veterinarians.FindAsync([vaccination.AdministeredByVetId], ct);
        logger.LogInformation("Created vaccination {VaccinationId} for pet {PetId}", vaccination.Id, vaccination.PetId);

        return new VaccinationResponse(vaccination.Id, vaccination.PetId, pet!.Name, vaccination.VaccineName,
            vaccination.DateAdministered, vaccination.ExpirationDate, vaccination.BatchNumber,
            vaccination.AdministeredByVetId, $"{vet!.FirstName} {vet.LastName}",
            vaccination.Notes, vaccination.ExpirationDate < today,
            vaccination.ExpirationDate >= today && vaccination.ExpirationDate <= today.AddDays(30),
            vaccination.CreatedAt);
    }
}
