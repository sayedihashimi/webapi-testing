using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class PetService(VetClinicDbContext context, ILogger<PetService> logger) : IPetService
{
    public async Task<PagedResult<PetResponse>> GetAllAsync(string? search, string? species, bool includeInactive, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = context.Pets.AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(p => p.IsActive);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(species))
        {
            query = query.Where(p => p.Species.ToLower() == species.ToLower());
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PetResponse(
                p.Id, p.Name, p.Species, p.Breed, p.DateOfBirth,
                p.Weight, p.Color, p.MicrochipNumber, p.IsActive,
                p.OwnerId, p.CreatedAt, p.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<PetResponse>(items, totalCount, page, pageSize);
    }

    public async Task<PetDetailResponse?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var pet = await context.Pets
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

        if (pet is null)
        {
            return null;
        }

        return new PetDetailResponse(
            pet.Id, pet.Name, pet.Species, pet.Breed, pet.DateOfBirth,
            pet.Weight, pet.Color, pet.MicrochipNumber, pet.IsActive,
            pet.OwnerId, pet.Owner.FirstName, pet.Owner.LastName,
            pet.CreatedAt, pet.UpdatedAt);
    }

    public async Task<PetResponse?> CreateAsync(CreatePetRequest request, CancellationToken cancellationToken)
    {
        var ownerExists = await context.Owners.AnyAsync(o => o.Id == request.OwnerId, cancellationToken);
        if (!ownerExists)
        {
            return null;
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

        context.Pets.Add(pet);
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Pet created: {PetId} {PetName} for Owner {OwnerId}", pet.Id, pet.Name, pet.OwnerId);

        return new PetResponse(
            pet.Id, pet.Name, pet.Species, pet.Breed, pet.DateOfBirth,
            pet.Weight, pet.Color, pet.MicrochipNumber, pet.IsActive,
            pet.OwnerId, pet.CreatedAt, pet.UpdatedAt);
    }

    public async Task<PetResponse?> UpdateAsync(int id, UpdatePetRequest request, CancellationToken cancellationToken)
    {
        var pet = await context.Pets.FindAsync([id], cancellationToken);
        if (pet is null)
        {
            return null;
        }

        var ownerExists = await context.Owners.AnyAsync(o => o.Id == request.OwnerId, cancellationToken);
        if (!ownerExists)
        {
            return null;
        }

        pet.Name = request.Name;
        pet.Species = request.Species;
        pet.Breed = request.Breed;
        pet.DateOfBirth = request.DateOfBirth;
        pet.Weight = request.Weight;
        pet.Color = request.Color;
        pet.MicrochipNumber = request.MicrochipNumber;
        pet.OwnerId = request.OwnerId;

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Pet updated: {PetId}", pet.Id);

        return new PetResponse(
            pet.Id, pet.Name, pet.Species, pet.Breed, pet.DateOfBirth,
            pet.Weight, pet.Color, pet.MicrochipNumber, pet.IsActive,
            pet.OwnerId, pet.CreatedAt, pet.UpdatedAt);
    }

    public async Task<bool> SoftDeleteAsync(int id, CancellationToken cancellationToken)
    {
        var pet = await context.Pets.FindAsync([id], cancellationToken);
        if (pet is null)
        {
            return false;
        }

        pet.IsActive = false;
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Pet soft-deleted: {PetId}", id);

        return true;
    }

    public async Task<IReadOnlyList<MedicalRecordResponse>> GetMedicalRecordsAsync(int petId, CancellationToken cancellationToken)
    {
        return await context.MedicalRecords
            .Include(m => m.Pet)
            .Include(m => m.Veterinarian)
            .Where(m => m.PetId == petId)
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new MedicalRecordResponse(
                m.Id, m.AppointmentId, m.PetId, m.Pet.Name,
                m.VeterinarianId, $"{m.Veterinarian.FirstName} {m.Veterinarian.LastName}",
                m.Diagnosis, m.Treatment, m.Notes, m.FollowUpDate,
                m.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<VaccinationResponse>> GetVaccinationsAsync(int petId, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await context.Vaccinations
            .Include(v => v.Pet)
            .Include(v => v.AdministeredByVet)
            .Where(v => v.PetId == petId)
            .OrderByDescending(v => v.DateAdministered)
            .Select(v => new VaccinationResponse(
                v.Id, v.PetId, v.Pet.Name, v.VaccineName,
                v.DateAdministered, v.ExpirationDate, v.BatchNumber,
                v.AdministeredByVetId, $"{v.AdministeredByVet.FirstName} {v.AdministeredByVet.LastName}",
                v.Notes,
                v.ExpirationDate < today,
                v.ExpirationDate >= today && v.ExpirationDate <= today.AddDays(30),
                v.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<VaccinationResponse>> GetUpcomingVaccinationsAsync(int petId, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await context.Vaccinations
            .Include(v => v.Pet)
            .Include(v => v.AdministeredByVet)
            .Where(v => v.PetId == petId && (v.ExpirationDate < today || v.ExpirationDate <= today.AddDays(30)))
            .OrderBy(v => v.ExpirationDate)
            .Select(v => new VaccinationResponse(
                v.Id, v.PetId, v.Pet.Name, v.VaccineName,
                v.DateAdministered, v.ExpirationDate, v.BatchNumber,
                v.AdministeredByVetId, $"{v.AdministeredByVet.FirstName} {v.AdministeredByVet.LastName}",
                v.Notes,
                v.ExpirationDate < today,
                v.ExpirationDate >= today && v.ExpirationDate <= today.AddDays(30),
                v.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<PrescriptionResponse>> GetActivePrescriptionsAsync(int petId, CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return await context.Prescriptions
            .Include(p => p.MedicalRecord)
            .Where(p => p.MedicalRecord.PetId == petId && p.EndDate >= today)
            .OrderBy(p => p.EndDate)
            .Select(p => new PrescriptionResponse(
                p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage,
                p.DurationDays, p.StartDate, p.EndDate, p.Instructions,
                p.EndDate >= today, p.CreatedAt))
            .ToListAsync(cancellationToken);
    }
}
