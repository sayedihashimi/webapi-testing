using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class OwnerService(VetClinicDbContext db, ILogger<OwnerService> logger) : IOwnerService
{
    public async Task<PagedResult<OwnerDto>> GetAllAsync(string? search, PaginationParams pagination)
    {
        var query = db.Owners.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(o => o.FirstName.ToLower().Contains(s)
                || o.LastName.ToLower().Contains(s)
                || o.Email.ToLower().Contains(s));
        }

        var total = await query.CountAsync();
        var items = await query
            .OrderBy(o => o.LastName).ThenBy(o => o.FirstName)
            .Skip(pagination.Skip).Take(pagination.PageSize)
            .Select(o => MapToDto(o))
            .ToListAsync();

        return new PagedResult<OwnerDto>(items, total, pagination.Page, pagination.PageSize);
    }

    public async Task<OwnerDetailDto?> GetByIdAsync(int id)
    {
        var owner = await db.Owners.Include(o => o.Pets).FirstOrDefaultAsync(o => o.Id == id);
        if (owner is null) return null;

        return new OwnerDetailDto(
            owner.Id, owner.FirstName, owner.LastName, owner.Email, owner.Phone,
            owner.Address, owner.City, owner.State, owner.ZipCode,
            owner.CreatedAt, owner.UpdatedAt,
            owner.Pets.Select(p => new PetSummaryDto(p.Id, p.Name, p.Species, p.Breed, p.IsActive)).ToList());
    }

    public async Task<OwnerDto> CreateAsync(CreateOwnerDto dto)
    {
        var owner = new Owner
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            Address = dto.Address,
            City = dto.City,
            State = dto.State,
            ZipCode = dto.ZipCode
        };

        db.Owners.Add(owner);
        await db.SaveChangesAsync();
        logger.LogInformation("Created owner {OwnerId} ({Name})", owner.Id, $"{owner.FirstName} {owner.LastName}");
        return MapToDto(owner);
    }

    public async Task<OwnerDto?> UpdateAsync(int id, UpdateOwnerDto dto)
    {
        var owner = await db.Owners.FindAsync(id);
        if (owner is null) return null;

        owner.FirstName = dto.FirstName;
        owner.LastName = dto.LastName;
        owner.Email = dto.Email;
        owner.Phone = dto.Phone;
        owner.Address = dto.Address;
        owner.City = dto.City;
        owner.State = dto.State;
        owner.ZipCode = dto.ZipCode;
        owner.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();
        logger.LogInformation("Updated owner {OwnerId}", owner.Id);
        return MapToDto(owner);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var owner = await db.Owners.Include(o => o.Pets).FirstOrDefaultAsync(o => o.Id == id);
        if (owner is null) return false;

        if (owner.Pets.Any(p => p.IsActive))
            throw new InvalidOperationException("Cannot delete owner with active pets. Deactivate pets first.");

        db.Owners.Remove(owner);
        await db.SaveChangesAsync();
        logger.LogInformation("Deleted owner {OwnerId}", id);
        return true;
    }

    public async Task<IReadOnlyList<PetDto>> GetPetsAsync(int ownerId)
    {
        return await db.Pets
            .Where(p => p.OwnerId == ownerId)
            .OrderBy(p => p.Name)
            .Select(p => new PetDto(p.Id, p.Name, p.Species, p.Breed, p.DateOfBirth, p.Weight, p.Color, p.MicrochipNumber, p.IsActive, p.OwnerId, p.CreatedAt, p.UpdatedAt))
            .ToListAsync();
    }

    public async Task<PagedResult<AppointmentDto>> GetAppointmentsAsync(int ownerId, PaginationParams pagination)
    {
        var query = db.Appointments
            .Include(a => a.Pet).Include(a => a.Veterinarian)
            .Where(a => a.Pet.OwnerId == ownerId);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.AppointmentDate)
            .Skip(pagination.Skip).Take(pagination.PageSize)
            .Select(a => new AppointmentDto(a.Id, a.PetId, a.Pet.Name, a.VeterinarianId,
                a.Veterinarian.FirstName + " " + a.Veterinarian.LastName,
                a.AppointmentDate, a.DurationMinutes, a.Status, a.Reason, a.Notes, a.CancellationReason,
                a.CreatedAt, a.UpdatedAt))
            .ToListAsync();

        return new PagedResult<AppointmentDto>(items, total, pagination.Page, pagination.PageSize);
    }

    private static OwnerDto MapToDto(Owner o) => new(
        o.Id, o.FirstName, o.LastName, o.Email, o.Phone,
        o.Address, o.City, o.State, o.ZipCode, o.CreatedAt, o.UpdatedAt);
}
