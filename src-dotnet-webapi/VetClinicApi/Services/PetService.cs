using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public interface IPetService
{
    Task<PaginatedResponse<PetResponse>> GetAllAsync(string? search, string? species, bool includeInactive, int page, int pageSize, CancellationToken ct);
    Task<PetDetailResponse?> GetByIdAsync(int id, CancellationToken ct);
    Task<PetResponse> CreateAsync(CreatePetRequest request, CancellationToken ct);
    Task<PetResponse?> UpdateAsync(int id, UpdatePetRequest request, CancellationToken ct);
    Task<bool> DeleteAsync(int id, CancellationToken ct);
    Task<PaginatedResponse<MedicalRecordResponse>?> GetMedicalRecordsAsync(int id, int page, int pageSize, CancellationToken ct);
    Task<List<VaccinationResponse>?> GetVaccinationsAsync(int id, CancellationToken ct);
    Task<List<VaccinationResponse>?> GetUpcomingVaccinationsAsync(int id, CancellationToken ct);
    Task<List<PrescriptionResponse>?> GetActivePrescriptionsAsync(int id, CancellationToken ct);
}

public class PetService(VetClinicDbContext db, ILogger<PetService> logger) : IPetService
{
    public async Task<PaginatedResponse<PetResponse>> GetAllAsync(string? search, string? species, bool includeInactive, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Pets.AsNoTracking().AsQueryable();

        if (!includeInactive)
            query = query.Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(species))
            query = query.Where(p => p.Species == species);

        var totalCount = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PetResponse(p.Id, p.Name, p.Species, p.Breed, p.DateOfBirth, p.Weight, p.Color,
                p.MicrochipNumber, p.IsActive, p.OwnerId, p.CreatedAt, p.UpdatedAt))
            .ToListAsync(ct);

        return new PaginatedResponse<PetResponse>(items, page, pageSize, totalCount, totalPages, page < totalPages, page > 1);
    }

    public async Task<PetDetailResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await db.Pets.AsNoTracking()
            .Include(p => p.Owner)
            .Where(p => p.Id == id)
            .Select(p => new PetDetailResponse(p.Id, p.Name, p.Species, p.Breed, p.DateOfBirth, p.Weight, p.Color,
                p.MicrochipNumber, p.IsActive, p.OwnerId, $"{p.Owner.FirstName} {p.Owner.LastName}",
                p.CreatedAt, p.UpdatedAt))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PetResponse> CreateAsync(CreatePetRequest request, CancellationToken ct)
    {
        if (!await db.Owners.AnyAsync(o => o.Id == request.OwnerId, ct))
            throw new KeyNotFoundException($"Owner with ID {request.OwnerId} not found.");

        if (!string.IsNullOrWhiteSpace(request.MicrochipNumber) &&
            await db.Pets.AnyAsync(p => p.MicrochipNumber == request.MicrochipNumber, ct))
            throw new InvalidOperationException($"A pet with microchip number '{request.MicrochipNumber}' already exists.");

        var pet = new Pet
        {
            Name = request.Name,
            Species = request.Species,
            Breed = request.Breed,
            DateOfBirth = request.DateOfBirth,
            Weight = request.Weight,
            Color = request.Color,
            MicrochipNumber = request.MicrochipNumber,
            OwnerId = request.OwnerId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Pets.Add(pet);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Created pet {PetId}: {Name}", pet.Id, pet.Name);

        return new PetResponse(pet.Id, pet.Name, pet.Species, pet.Breed, pet.DateOfBirth, pet.Weight, pet.Color,
            pet.MicrochipNumber, pet.IsActive, pet.OwnerId, pet.CreatedAt, pet.UpdatedAt);
    }

    public async Task<PetResponse?> UpdateAsync(int id, UpdatePetRequest request, CancellationToken ct)
    {
        var pet = await db.Pets.FindAsync([id], ct);
        if (pet is null) return null;

        if (!await db.Owners.AnyAsync(o => o.Id == request.OwnerId, ct))
            throw new KeyNotFoundException($"Owner with ID {request.OwnerId} not found.");

        if (!string.IsNullOrWhiteSpace(request.MicrochipNumber) &&
            await db.Pets.AnyAsync(p => p.MicrochipNumber == request.MicrochipNumber && p.Id != id, ct))
            throw new InvalidOperationException($"A pet with microchip number '{request.MicrochipNumber}' already exists.");

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

        return new PetResponse(pet.Id, pet.Name, pet.Species, pet.Breed, pet.DateOfBirth, pet.Weight, pet.Color,
            pet.MicrochipNumber, pet.IsActive, pet.OwnerId, pet.CreatedAt, pet.UpdatedAt);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var pet = await db.Pets.FindAsync([id], ct);
        if (pet is null)
            throw new KeyNotFoundException($"Pet with ID {id} not found.");

        pet.IsActive = false;
        pet.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Soft-deleted pet {PetId}", id);
        return true;
    }

    public async Task<PaginatedResponse<MedicalRecordResponse>?> GetMedicalRecordsAsync(int id, int page, int pageSize, CancellationToken ct)
    {
        if (!await db.Pets.AnyAsync(p => p.Id == id, ct))
            return null;

        var query = db.MedicalRecords.AsNoTracking()
            .Include(m => m.Pet)
            .Include(m => m.Veterinarian)
            .Where(m => m.PetId == id);

        var totalCount = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new MedicalRecordResponse(m.Id, m.AppointmentId, m.PetId, m.Pet.Name,
                m.VeterinarianId, $"{m.Veterinarian.FirstName} {m.Veterinarian.LastName}",
                m.Diagnosis, m.Treatment, m.Notes, m.FollowUpDate, m.CreatedAt))
            .ToListAsync(ct);

        return new PaginatedResponse<MedicalRecordResponse>(items, page, pageSize, totalCount, totalPages, page < totalPages, page > 1);
    }

    public async Task<List<VaccinationResponse>?> GetVaccinationsAsync(int id, CancellationToken ct)
    {
        if (!await db.Pets.AnyAsync(p => p.Id == id, ct))
            return null;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await db.Vaccinations.AsNoTracking()
            .Include(v => v.Pet)
            .Include(v => v.AdministeredBy)
            .Where(v => v.PetId == id)
            .OrderByDescending(v => v.DateAdministered)
            .Select(v => new VaccinationResponse(v.Id, v.PetId, v.Pet.Name, v.VaccineName,
                v.DateAdministered, v.ExpirationDate, v.BatchNumber, v.AdministeredByVetId,
                $"{v.AdministeredBy.FirstName} {v.AdministeredBy.LastName}",
                v.Notes, v.ExpirationDate < today, v.ExpirationDate >= today && v.ExpirationDate <= today.AddDays(30),
                v.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<List<VaccinationResponse>?> GetUpcomingVaccinationsAsync(int id, CancellationToken ct)
    {
        if (!await db.Pets.AnyAsync(p => p.Id == id, ct))
            return null;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var thirtyDays = today.AddDays(30);

        return await db.Vaccinations.AsNoTracking()
            .Include(v => v.Pet)
            .Include(v => v.AdministeredBy)
            .Where(v => v.PetId == id && v.ExpirationDate <= thirtyDays)
            .OrderBy(v => v.ExpirationDate)
            .Select(v => new VaccinationResponse(v.Id, v.PetId, v.Pet.Name, v.VaccineName,
                v.DateAdministered, v.ExpirationDate, v.BatchNumber, v.AdministeredByVetId,
                $"{v.AdministeredBy.FirstName} {v.AdministeredBy.LastName}",
                v.Notes, v.ExpirationDate < today, v.ExpirationDate >= today && v.ExpirationDate <= thirtyDays,
                v.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<List<PrescriptionResponse>?> GetActivePrescriptionsAsync(int id, CancellationToken ct)
    {
        if (!await db.Pets.AnyAsync(p => p.Id == id, ct))
            return null;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await db.Prescriptions.AsNoTracking()
            .Include(p => p.MedicalRecord)
            .Where(p => p.MedicalRecord.PetId == id && p.EndDate >= today)
            .OrderBy(p => p.EndDate)
            .Select(p => new PrescriptionResponse(p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage,
                p.DurationDays, p.StartDate, p.EndDate, p.Instructions, p.EndDate >= today, p.CreatedAt))
            .ToListAsync(ct);
    }
}
