using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class VaccinationService(VetClinicDbContext db, ILogger<VaccinationService> logger) : IVaccinationService
{
    public async Task<VaccinationResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var vaccination = await db.Vaccinations.AsNoTracking()
            .Include(v => v.AdministeredByVet)
            .FirstOrDefaultAsync(v => v.Id == id, ct);

        return vaccination is null ? null : MapToResponse(vaccination);
    }

    public async Task<VaccinationResponse> CreateAsync(CreateVaccinationRequest request, CancellationToken ct)
    {
        var petExists = await db.Pets.AsNoTracking().AnyAsync(p => p.Id == request.PetId && p.IsActive, ct);
        if (!petExists)
            throw new KeyNotFoundException($"Active pet with ID {request.PetId} not found.");

        var vetExists = await db.Veterinarians.AsNoTracking().AnyAsync(v => v.Id == request.AdministeredByVetId, ct);
        if (!vetExists)
            throw new KeyNotFoundException($"Veterinarian with ID {request.AdministeredByVetId} not found.");

        if (request.ExpirationDate <= request.DateAdministered)
            throw new ArgumentException("Expiration date must be after the date administered.");

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
            .Include(v => v.AdministeredByVet)
            .FirstAsync(v => v.Id == vaccination.Id, ct);

        logger.LogInformation("Created vaccination {VaccinationId} for pet {PetId}", vaccination.Id, vaccination.PetId);

        return MapToResponse(created);
    }

    private static VaccinationResponse MapToResponse(Vaccination v)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return new()
        {
            Id = v.Id,
            PetId = v.PetId,
            VaccineName = v.VaccineName,
            DateAdministered = v.DateAdministered,
            ExpirationDate = v.ExpirationDate,
            BatchNumber = v.BatchNumber,
            AdministeredByVetId = v.AdministeredByVetId,
            Notes = v.Notes,
            IsExpired = v.ExpirationDate < today,
            IsDueSoon = v.ExpirationDate >= today && v.ExpirationDate <= today.AddDays(30),
            CreatedAt = v.CreatedAt,
            AdministeredByVet = v.AdministeredByVet != null ? new VeterinarianSummaryResponse
            {
                Id = v.AdministeredByVet.Id,
                FirstName = v.AdministeredByVet.FirstName,
                LastName = v.AdministeredByVet.LastName,
                Specialization = v.AdministeredByVet.Specialization
            } : null
        };
    }
}
