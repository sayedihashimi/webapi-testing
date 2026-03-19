using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class PetService(VetClinicDbContext db, ILogger<PetService> logger) : IPetService
{
    private static readonly string[] ValidSpecies = ["Dog", "Cat", "Bird", "Rabbit"];

    public async Task<PaginatedResponse<PetResponse>> GetAllAsync(
        string? search, string? species, bool includeInactive, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Pets.AsNoTracking().Include(p => p.Owner).AsQueryable();

        if (!includeInactive)
            query = query.Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(s));
        }

        if (!string.IsNullOrWhiteSpace(species))
            query = query.Where(p => p.Species == species);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PetResponse(
                p.Id, p.Name, p.Species, p.Breed, p.DateOfBirth, p.Weight,
                p.Color, p.MicrochipNumber, p.IsActive, p.OwnerId,
                p.Owner.FirstName + " " + p.Owner.LastName,
                p.CreatedAt, p.UpdatedAt))
            .ToListAsync(ct);

        return PaginatedResponse<PetResponse>.Create(items, page, pageSize, totalCount);
    }

    public async Task<PetDetailResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var pet = await db.Pets.AsNoTracking()
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (pet is null) return null;

        var ownerResponse = new OwnerResponse(
            pet.Owner.Id, pet.Owner.FirstName, pet.Owner.LastName, pet.Owner.Email,
            pet.Owner.Phone, pet.Owner.Address, pet.Owner.City, pet.Owner.State,
            pet.Owner.ZipCode, pet.Owner.CreatedAt, pet.Owner.UpdatedAt);

        return new PetDetailResponse(
            pet.Id, pet.Name, pet.Species, pet.Breed, pet.DateOfBirth, pet.Weight,
            pet.Color, pet.MicrochipNumber, pet.IsActive, pet.OwnerId,
            pet.Owner.FirstName + " " + pet.Owner.LastName,
            pet.CreatedAt, pet.UpdatedAt, ownerResponse);
    }

    public async Task<PetResponse> CreateAsync(CreatePetRequest request, CancellationToken ct)
    {
        if (!ValidSpecies.Contains(request.Species))
            throw new ArgumentException($"Invalid species '{request.Species}'. Valid values: {string.Join(", ", ValidSpecies)}");

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
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Pets.Add(pet);
        await db.SaveChangesAsync(ct);

        var owner = await db.Owners.FindAsync([pet.OwnerId], ct);
        logger.LogInformation("Created pet {PetId}: {Name} for owner {OwnerId}", pet.Id, pet.Name, pet.OwnerId);

        return new PetResponse(
            pet.Id, pet.Name, pet.Species, pet.Breed, pet.DateOfBirth, pet.Weight,
            pet.Color, pet.MicrochipNumber, pet.IsActive, pet.OwnerId,
            owner!.FirstName + " " + owner.LastName,
            pet.CreatedAt, pet.UpdatedAt);
    }

    public async Task<PetResponse?> UpdateAsync(int id, UpdatePetRequest request, CancellationToken ct)
    {
        var pet = await db.Pets.Include(p => p.Owner).FirstOrDefaultAsync(p => p.Id == id, ct);
        if (pet is null) return null;

        if (!ValidSpecies.Contains(request.Species))
            throw new ArgumentException($"Invalid species '{request.Species}'. Valid values: {string.Join(", ", ValidSpecies)}");

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

        var owner = await db.Owners.FindAsync([pet.OwnerId], ct);
        logger.LogInformation("Updated pet {PetId}", id);

        return new PetResponse(
            pet.Id, pet.Name, pet.Species, pet.Breed, pet.DateOfBirth, pet.Weight,
            pet.Color, pet.MicrochipNumber, pet.IsActive, pet.OwnerId,
            owner!.FirstName + " " + owner.LastName,
            pet.CreatedAt, pet.UpdatedAt);
    }

    public async Task<bool> SoftDeleteAsync(int id, CancellationToken ct)
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

    public async Task<IReadOnlyList<MedicalRecordResponse>> GetMedicalRecordsAsync(int petId, CancellationToken ct)
    {
        if (!await db.Pets.AnyAsync(p => p.Id == petId, ct))
            throw new KeyNotFoundException($"Pet with ID {petId} not found.");

        return await db.MedicalRecords.AsNoTracking()
            .Include(m => m.Pet).Include(m => m.Veterinarian)
            .Where(m => m.PetId == petId)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new MedicalRecordResponse(
                m.Id, m.AppointmentId, m.PetId, m.Pet.Name,
                m.VeterinarianId, m.Veterinarian.FirstName + " " + m.Veterinarian.LastName,
                m.Diagnosis, m.Treatment, m.Notes, m.FollowUpDate, m.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<VaccinationResponse>> GetVaccinationsAsync(int petId, CancellationToken ct)
    {
        if (!await db.Pets.AnyAsync(p => p.Id == petId, ct))
            throw new KeyNotFoundException($"Pet with ID {petId} not found.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await db.Vaccinations.AsNoTracking()
            .Include(v => v.Pet).Include(v => v.AdministeredByVet)
            .Where(v => v.PetId == petId)
            .OrderByDescending(v => v.DateAdministered)
            .Select(v => new VaccinationResponse(
                v.Id, v.PetId, v.Pet.Name, v.VaccineName,
                v.DateAdministered, v.ExpirationDate, v.BatchNumber,
                v.AdministeredByVetId,
                v.AdministeredByVet.FirstName + " " + v.AdministeredByVet.LastName,
                v.Notes,
                v.ExpirationDate < today,
                v.ExpirationDate >= today && v.ExpirationDate <= today.AddDays(30),
                v.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<VaccinationResponse>> GetUpcomingVaccinationsAsync(int petId, CancellationToken ct)
    {
        if (!await db.Pets.AnyAsync(p => p.Id == petId, ct))
            throw new KeyNotFoundException($"Pet with ID {petId} not found.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await db.Vaccinations.AsNoTracking()
            .Include(v => v.Pet).Include(v => v.AdministeredByVet)
            .Where(v => v.PetId == petId &&
                (v.ExpirationDate < today || v.ExpirationDate <= today.AddDays(30)))
            .OrderBy(v => v.ExpirationDate)
            .Select(v => new VaccinationResponse(
                v.Id, v.PetId, v.Pet.Name, v.VaccineName,
                v.DateAdministered, v.ExpirationDate, v.BatchNumber,
                v.AdministeredByVetId,
                v.AdministeredByVet.FirstName + " " + v.AdministeredByVet.LastName,
                v.Notes,
                v.ExpirationDate < today,
                v.ExpirationDate >= today && v.ExpirationDate <= today.AddDays(30),
                v.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PrescriptionResponse>> GetActivePrescriptionsAsync(int petId, CancellationToken ct)
    {
        if (!await db.Pets.AnyAsync(p => p.Id == petId, ct))
            throw new KeyNotFoundException($"Pet with ID {petId} not found.");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await db.Prescriptions.AsNoTracking()
            .Include(p => p.MedicalRecord)
            .Where(p => p.MedicalRecord.PetId == petId && p.EndDate >= today)
            .OrderBy(p => p.EndDate)
            .Select(p => new PrescriptionResponse(
                p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage,
                p.DurationDays, p.StartDate, p.EndDate, p.Instructions,
                p.EndDate >= today, p.CreatedAt))
            .ToListAsync(ct);
    }
}
