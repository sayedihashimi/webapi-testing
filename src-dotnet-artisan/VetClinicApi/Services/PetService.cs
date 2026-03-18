using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class PetService(VetClinicDbContext db, ILogger<PetService> logger)
{
    private static readonly string[] ValidSpecies = ["Dog", "Cat", "Bird", "Rabbit"];

    public async Task<PagedResponse<PetResponse>> GetAllAsync(
        string? search, string? species, bool includeInactive, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Pets.AsNoTracking().AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(p => p.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(species))
        {
            query = query.Where(p => p.Species == species);
        }

        var totalCount = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PetResponse(
                p.Id, p.Name, p.Species, p.Breed, p.DateOfBirth,
                p.Weight, p.Color, p.MicrochipNumber, p.IsActive,
                p.OwnerId, p.CreatedAt, p.UpdatedAt))
            .ToListAsync(ct);

        return new PagedResponse<PetResponse>(
            items, page, pageSize, totalCount, totalPages,
            page < totalPages, page > 1);
    }

    public async Task<PetDetailResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await db.Pets
            .AsNoTracking()
            .Include(p => p.Owner)
            .Where(p => p.Id == id)
            .Select(p => new PetDetailResponse(
                p.Id, p.Name, p.Species, p.Breed, p.DateOfBirth,
                p.Weight, p.Color, p.MicrochipNumber, p.IsActive,
                p.OwnerId, p.Owner.FirstName + " " + p.Owner.LastName,
                p.CreatedAt, p.UpdatedAt))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PetResponse> CreateAsync(CreatePetRequest request, CancellationToken ct)
    {
        ValidateSpecies(request.Species);

        if (!await db.Owners.AnyAsync(o => o.Id == request.OwnerId, ct))
        {
            throw new KeyNotFoundException($"Owner with ID {request.OwnerId} not found.");
        }

        if (request.MicrochipNumber is not null &&
            await db.Pets.AnyAsync(p => p.MicrochipNumber == request.MicrochipNumber, ct))
        {
            throw new InvalidOperationException($"A pet with microchip number '{request.MicrochipNumber}' already exists.");
        }

        var pet = new Pet
        {
            Name = request.Name,
            Species = request.Species,
            Breed = request.Breed,
            DateOfBirth = request.DateOfBirth,
            Weight = request.Weight,
            Color = request.Color,
            MicrochipNumber = request.MicrochipNumber,
            OwnerId = request.OwnerId
        };

        db.Pets.Add(pet);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Created pet {PetId}: {PetName} ({Species})", pet.Id, pet.Name, pet.Species);
        return MapToResponse(pet);
    }

    public async Task<PetResponse?> UpdateAsync(int id, UpdatePetRequest request, CancellationToken ct)
    {
        ValidateSpecies(request.Species);

        var pet = await db.Pets.FindAsync([id], ct);
        if (pet is null)
        {
            return null;
        }

        if (!await db.Owners.AnyAsync(o => o.Id == request.OwnerId, ct))
        {
            throw new KeyNotFoundException($"Owner with ID {request.OwnerId} not found.");
        }

        if (request.MicrochipNumber is not null &&
            await db.Pets.AnyAsync(p => p.MicrochipNumber == request.MicrochipNumber && p.Id != id, ct))
        {
            throw new InvalidOperationException($"A pet with microchip number '{request.MicrochipNumber}' already exists.");
        }

        pet.Name = request.Name;
        pet.Species = request.Species;
        pet.Breed = request.Breed;
        pet.DateOfBirth = request.DateOfBirth;
        pet.Weight = request.Weight;
        pet.Color = request.Color;
        pet.MicrochipNumber = request.MicrochipNumber;
        pet.OwnerId = request.OwnerId;
        pet.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Updated pet {PetId}", pet.Id);
        return MapToResponse(pet);
    }

    public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
    {
        var pet = await db.Pets.FindAsync([id], ct);
        if (pet is null)
        {
            return false;
        }

        pet.IsActive = false;
        pet.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Soft-deleted pet {PetId}", id);
        return true;
    }

    public async Task<List<MedicalRecordResponse>> GetMedicalRecordsAsync(int petId, CancellationToken ct)
    {
        if (!await db.Pets.AnyAsync(p => p.Id == petId, ct))
        {
            throw new KeyNotFoundException($"Pet with ID {petId} not found.");
        }

        return await db.MedicalRecords
            .AsNoTracking()
            .Include(m => m.Veterinarian)
            .Include(m => m.Prescriptions)
            .Where(m => m.PetId == petId)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => MapToMedicalRecordResponse(m))
            .ToListAsync(ct);
    }

    public async Task<List<VaccinationResponse>> GetVaccinationsAsync(int petId, CancellationToken ct)
    {
        if (!await db.Pets.AnyAsync(p => p.Id == petId, ct))
        {
            throw new KeyNotFoundException($"Pet with ID {petId} not found.");
        }

        return await db.Vaccinations
            .AsNoTracking()
            .Include(v => v.Pet)
            .Include(v => v.AdministeredByVet)
            .Where(v => v.PetId == petId)
            .OrderByDescending(v => v.DateAdministered)
            .ToListAsync(ct)
            .ContinueWith(t => t.Result.Select(MapToVaccinationResponse).ToList(), ct);
    }

    public async Task<List<VaccinationResponse>> GetUpcomingVaccinationsAsync(int petId, CancellationToken ct)
    {
        if (!await db.Pets.AnyAsync(p => p.Id == petId, ct))
        {
            throw new KeyNotFoundException($"Pet with ID {petId} not found.");
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var dueSoonDate = today.AddDays(30);

        var vaccinations = await db.Vaccinations
            .AsNoTracking()
            .Include(v => v.Pet)
            .Include(v => v.AdministeredByVet)
            .Where(v => v.PetId == petId && v.ExpirationDate <= dueSoonDate)
            .OrderBy(v => v.ExpirationDate)
            .ToListAsync(ct);

        return vaccinations.Select(MapToVaccinationResponse).ToList();
    }

    public async Task<List<PrescriptionResponse>> GetActivePrescriptionsAsync(int petId, CancellationToken ct)
    {
        if (!await db.Pets.AnyAsync(p => p.Id == petId, ct))
        {
            throw new KeyNotFoundException($"Pet with ID {petId} not found.");
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var prescriptions = await db.Prescriptions
            .AsNoTracking()
            .Include(p => p.MedicalRecord)
            .Where(p => p.MedicalRecord.PetId == petId)
            .ToListAsync(ct);

        return prescriptions
            .Where(p => p.IsActive)
            .Select(p => new PrescriptionResponse(
                p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage,
                p.DurationDays, p.StartDate, p.EndDate, p.IsActive,
                p.Instructions, p.CreatedAt))
            .ToList();
    }

    private static void ValidateSpecies(string species)
    {
        if (!ValidSpecies.Contains(species))
        {
            throw new ArgumentException($"Invalid species '{species}'. Must be one of: {string.Join(", ", ValidSpecies)}");
        }
    }

    private static PetResponse MapToResponse(Pet p) =>
        new(p.Id, p.Name, p.Species, p.Breed, p.DateOfBirth,
            p.Weight, p.Color, p.MicrochipNumber, p.IsActive,
            p.OwnerId, p.CreatedAt, p.UpdatedAt);

    internal static MedicalRecordResponse MapToMedicalRecordResponse(MedicalRecord m) =>
        new(m.Id, m.AppointmentId, m.PetId, m.Pet?.Name ?? "",
            m.VeterinarianId, m.Veterinarian is not null ? m.Veterinarian.FirstName + " " + m.Veterinarian.LastName : "",
            m.Diagnosis, m.Treatment, m.Notes, m.FollowUpDate, m.CreatedAt,
            m.Prescriptions?.Select(p => new PrescriptionResponse(
                p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage,
                p.DurationDays, p.StartDate, p.EndDate, p.IsActive,
                p.Instructions, p.CreatedAt)).ToList());

    internal static VaccinationResponse MapToVaccinationResponse(Vaccination v) =>
        new(v.Id, v.PetId, v.Pet?.Name ?? "", v.VaccineName,
            v.DateAdministered, v.ExpirationDate, v.BatchNumber,
            v.AdministeredByVetId,
            v.AdministeredByVet is not null ? v.AdministeredByVet.FirstName + " " + v.AdministeredByVet.LastName : "",
            v.Notes, v.IsExpired, v.IsDueSoon, v.CreatedAt);
}
