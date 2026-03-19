using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class OwnerService(VetClinicDbContext context, ILogger<OwnerService> logger) : IOwnerService
{
    public async Task<PagedResult<OwnerResponse>> GetAllAsync(string? search, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = context.Owners.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLower();
            query = query.Where(o =>
                o.FirstName.ToLower().Contains(term) ||
                o.LastName.ToLower().Contains(term) ||
                o.Email.ToLower().Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(o => o.LastName).ThenBy(o => o.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => MapToResponse(o))
            .ToListAsync(cancellationToken);

        return new PagedResult<OwnerResponse>(items, totalCount, page, pageSize);
    }

    public async Task<OwnerDetailResponse?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var owner = await context.Owners
            .Include(o => o.Pets)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (owner is null)
        {
            return null;
        }

        return new OwnerDetailResponse(
            owner.Id, owner.FirstName, owner.LastName, owner.Email, owner.Phone,
            owner.Address, owner.City, owner.State, owner.ZipCode,
            owner.CreatedAt, owner.UpdatedAt,
            owner.Pets.Select(p => new PetSummaryResponse(p.Id, p.Name, p.Species, p.Breed, p.IsActive)).ToList());
    }

    public async Task<OwnerResponse> CreateAsync(CreateOwnerRequest request, CancellationToken cancellationToken)
    {
        var owner = new Owner
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            City = request.City,
            State = request.State,
            ZipCode = request.ZipCode
        };

        context.Owners.Add(owner);
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Owner created: {OwnerId} {FirstName} {LastName}", owner.Id, owner.FirstName, owner.LastName);

        return MapToResponse(owner);
    }

    public async Task<OwnerResponse?> UpdateAsync(int id, UpdateOwnerRequest request, CancellationToken cancellationToken)
    {
        var owner = await context.Owners.FindAsync([id], cancellationToken);
        if (owner is null)
        {
            return null;
        }

        owner.FirstName = request.FirstName;
        owner.LastName = request.LastName;
        owner.Email = request.Email;
        owner.Phone = request.Phone;
        owner.Address = request.Address;
        owner.City = request.City;
        owner.State = request.State;
        owner.ZipCode = request.ZipCode;

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Owner updated: {OwnerId}", owner.Id);

        return MapToResponse(owner);
    }

    public async Task<(bool Found, bool HasActivePets)> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var owner = await context.Owners
            .Include(o => o.Pets)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

        if (owner is null)
        {
            return (false, false);
        }

        if (owner.Pets.Any(p => p.IsActive))
        {
            return (true, true);
        }

        context.Owners.Remove(owner);
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Owner deleted: {OwnerId}", id);

        return (true, false);
    }

    public async Task<IReadOnlyList<PetResponse>> GetPetsAsync(int ownerId, CancellationToken cancellationToken)
    {
        return await context.Pets
            .Where(p => p.OwnerId == ownerId)
            .Select(p => new PetResponse(
                p.Id, p.Name, p.Species, p.Breed, p.DateOfBirth,
                p.Weight, p.Color, p.MicrochipNumber, p.IsActive,
                p.OwnerId, p.CreatedAt, p.UpdatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AppointmentResponse>> GetAppointmentsAsync(int ownerId, CancellationToken cancellationToken)
    {
        return await context.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Where(a => a.Pet.OwnerId == ownerId)
            .OrderByDescending(a => a.AppointmentDate)
            .Select(a => new AppointmentResponse(
                a.Id, a.PetId, a.Pet.Name,
                a.VeterinarianId, $"{a.Veterinarian.FirstName} {a.Veterinarian.LastName}",
                a.AppointmentDate, a.DurationMinutes, a.Status,
                a.Reason, a.Notes, a.CancellationReason,
                a.CreatedAt, a.UpdatedAt))
            .ToListAsync(cancellationToken);
    }

    private static OwnerResponse MapToResponse(Owner owner) =>
        new(owner.Id, owner.FirstName, owner.LastName, owner.Email, owner.Phone,
            owner.Address, owner.City, owner.State, owner.ZipCode,
            owner.CreatedAt, owner.UpdatedAt);
}
