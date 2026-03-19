using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs.Appointment;
using VetClinicApi.DTOs.Common;
using VetClinicApi.DTOs.Owner;
using VetClinicApi.DTOs.Pet;
using VetClinicApi.Models;
using VetClinicApi.Services.Interfaces;

namespace VetClinicApi.Services.Implementations;

public class OwnerService : IOwnerService
{
    private readonly VetClinicDbContext _db;
    private readonly ILogger<OwnerService> _logger;

    public OwnerService(VetClinicDbContext db, ILogger<OwnerService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PagedResult<OwnerDto>> GetAllAsync(string? search, PaginationParams pagination)
    {
        var query = _db.Owners.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(o =>
                o.FirstName.ToLower().Contains(s) ||
                o.LastName.ToLower().Contains(s) ||
                o.Email.ToLower().Contains(s));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(o => o.LastName).ThenBy(o => o.FirstName)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(o => MapToDto(o))
            .ToListAsync();

        return new PagedResult<OwnerDto> { Items = items, TotalCount = totalCount, Page = pagination.Page, PageSize = pagination.PageSize };
    }

    public async Task<OwnerDetailDto> GetByIdAsync(int id)
    {
        var owner = await _db.Owners.Include(o => o.Pets).FirstOrDefaultAsync(o => o.Id == id)
            ?? throw new KeyNotFoundException($"Owner with ID {id} not found");

        return new OwnerDetailDto
        {
            Id = owner.Id, FirstName = owner.FirstName, LastName = owner.LastName,
            Email = owner.Email, Phone = owner.Phone, Address = owner.Address,
            City = owner.City, State = owner.State, ZipCode = owner.ZipCode,
            CreatedAt = owner.CreatedAt, UpdatedAt = owner.UpdatedAt,
            Pets = owner.Pets.Select(p => new OwnerPetDto
            {
                Id = p.Id, Name = p.Name, Species = p.Species, Breed = p.Breed, IsActive = p.IsActive
            }).ToList()
        };
    }

    public async Task<OwnerDto> CreateAsync(CreateOwnerDto dto)
    {
        if (await _db.Owners.AnyAsync(o => o.Email == dto.Email))
            throw new ArgumentException($"An owner with email '{dto.Email}' already exists");

        var owner = new Owner
        {
            FirstName = dto.FirstName, LastName = dto.LastName, Email = dto.Email,
            Phone = dto.Phone, Address = dto.Address, City = dto.City,
            State = dto.State, ZipCode = dto.ZipCode
        };

        _db.Owners.Add(owner);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Created owner {OwnerId}: {FirstName} {LastName}", owner.Id, owner.FirstName, owner.LastName);
        return MapToDto(owner);
    }

    public async Task<OwnerDto> UpdateAsync(int id, UpdateOwnerDto dto)
    {
        var owner = await _db.Owners.FindAsync(id)
            ?? throw new KeyNotFoundException($"Owner with ID {id} not found");

        if (await _db.Owners.AnyAsync(o => o.Email == dto.Email && o.Id != id))
            throw new ArgumentException($"An owner with email '{dto.Email}' already exists");

        owner.FirstName = dto.FirstName;
        owner.LastName = dto.LastName;
        owner.Email = dto.Email;
        owner.Phone = dto.Phone;
        owner.Address = dto.Address;
        owner.City = dto.City;
        owner.State = dto.State;
        owner.ZipCode = dto.ZipCode;
        owner.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Updated owner {OwnerId}", id);
        return MapToDto(owner);
    }

    public async Task DeleteAsync(int id)
    {
        var owner = await _db.Owners.Include(o => o.Pets).FirstOrDefaultAsync(o => o.Id == id)
            ?? throw new KeyNotFoundException($"Owner with ID {id} not found");

        if (owner.Pets.Any(p => p.IsActive))
            throw new InvalidOperationException("Cannot delete owner with active pets. Deactivate or transfer pets first.");

        _db.Owners.Remove(owner);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Deleted owner {OwnerId}", id);
    }

    public async Task<List<PetDto>> GetPetsAsync(int ownerId)
    {
        if (!await _db.Owners.AnyAsync(o => o.Id == ownerId))
            throw new KeyNotFoundException($"Owner with ID {ownerId} not found");

        return await _db.Pets
            .Where(p => p.OwnerId == ownerId)
            .Include(p => p.Owner)
            .Select(p => new PetDto
            {
                Id = p.Id, Name = p.Name, Species = p.Species, Breed = p.Breed,
                DateOfBirth = p.DateOfBirth, Weight = p.Weight, Color = p.Color,
                MicrochipNumber = p.MicrochipNumber, IsActive = p.IsActive,
                OwnerId = p.OwnerId, OwnerName = p.Owner.FirstName + " " + p.Owner.LastName,
                CreatedAt = p.CreatedAt, UpdatedAt = p.UpdatedAt
            }).ToListAsync();
    }

    public async Task<PagedResult<AppointmentDto>> GetAppointmentsAsync(int ownerId, PaginationParams pagination)
    {
        if (!await _db.Owners.AnyAsync(o => o.Id == ownerId))
            throw new KeyNotFoundException($"Owner with ID {ownerId} not found");

        var query = _db.Appointments
            .Include(a => a.Pet).Include(a => a.Veterinarian)
            .Where(a => a.Pet.OwnerId == ownerId);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.AppointmentDate)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(a => MapAppointmentToDto(a))
            .ToListAsync();

        return new PagedResult<AppointmentDto> { Items = items, TotalCount = totalCount, Page = pagination.Page, PageSize = pagination.PageSize };
    }

    private static OwnerDto MapToDto(Owner o) => new()
    {
        Id = o.Id, FirstName = o.FirstName, LastName = o.LastName,
        Email = o.Email, Phone = o.Phone, Address = o.Address,
        City = o.City, State = o.State, ZipCode = o.ZipCode,
        CreatedAt = o.CreatedAt, UpdatedAt = o.UpdatedAt
    };

    private static AppointmentDto MapAppointmentToDto(Appointment a) => new()
    {
        Id = a.Id, PetId = a.PetId, PetName = a.Pet.Name,
        VeterinarianId = a.VeterinarianId, VeterinarianName = a.Veterinarian.FirstName + " " + a.Veterinarian.LastName,
        AppointmentDate = a.AppointmentDate, DurationMinutes = a.DurationMinutes,
        Status = a.Status, Reason = a.Reason, Notes = a.Notes,
        CancellationReason = a.CancellationReason, CreatedAt = a.CreatedAt, UpdatedAt = a.UpdatedAt
    };
}
