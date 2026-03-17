using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Middleware;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public interface IOwnerService
{
    Task<PaginatedResponse<OwnerSummaryDto>> GetAllAsync(string? search, int page, int pageSize);
    Task<OwnerDto> GetByIdAsync(int id);
    Task<OwnerDto> CreateAsync(CreateOwnerDto dto);
    Task<OwnerDto> UpdateAsync(int id, UpdateOwnerDto dto);
    Task DeleteAsync(int id);
    Task<List<PetSummaryDto>> GetPetsAsync(int ownerId);
    Task<PaginatedResponse<AppointmentSummaryDto>> GetAppointmentsAsync(int ownerId, int page, int pageSize);
}

public class OwnerService : IOwnerService
{
    private readonly VetClinicDbContext _db;
    private readonly ILogger<OwnerService> _logger;

    public OwnerService(VetClinicDbContext db, ILogger<OwnerService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PaginatedResponse<OwnerSummaryDto>> GetAllAsync(string? search, int page, int pageSize)
    {
        var query = _db.Owners.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower();
            query = query.Where(o => o.FirstName.ToLower().Contains(s) ||
                                     o.LastName.ToLower().Contains(s) ||
                                     o.Email.ToLower().Contains(s));
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(o => o.LastName).ThenBy(o => o.FirstName)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(o => new OwnerSummaryDto
            {
                Id = o.Id, FirstName = o.FirstName, LastName = o.LastName,
                Email = o.Email, Phone = o.Phone
            }).ToListAsync();

        return new PaginatedResponse<OwnerSummaryDto>
        {
            Data = items, Page = page, PageSize = pageSize, TotalCount = totalCount
        };
    }

    public async Task<OwnerDto> GetByIdAsync(int id)
    {
        var owner = await _db.Owners.Include(o => o.Pets)
            .FirstOrDefaultAsync(o => o.Id == id)
            ?? throw new NotFoundException("Owner", id);

        return MapToDto(owner);
    }

    public async Task<OwnerDto> CreateAsync(CreateOwnerDto dto)
    {
        if (await _db.Owners.AnyAsync(o => o.Email == dto.Email))
            throw new BusinessRuleException("An owner with this email already exists.", StatusCodes.Status409Conflict);

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
        var owner = await _db.Owners.Include(o => o.Pets)
            .FirstOrDefaultAsync(o => o.Id == id)
            ?? throw new NotFoundException("Owner", id);

        if (await _db.Owners.AnyAsync(o => o.Email == dto.Email && o.Id != id))
            throw new BusinessRuleException("An owner with this email already exists.", StatusCodes.Status409Conflict);

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
        _logger.LogInformation("Updated owner {OwnerId}", owner.Id);
        return MapToDto(owner);
    }

    public async Task DeleteAsync(int id)
    {
        var owner = await _db.Owners.Include(o => o.Pets)
            .FirstOrDefaultAsync(o => o.Id == id)
            ?? throw new NotFoundException("Owner", id);

        if (owner.Pets.Any(p => p.IsActive))
            throw new BusinessRuleException("Cannot delete owner with active pets. Deactivate all pets first.");

        _db.Owners.Remove(owner);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Deleted owner {OwnerId}", owner.Id);
    }

    public async Task<List<PetSummaryDto>> GetPetsAsync(int ownerId)
    {
        if (!await _db.Owners.AnyAsync(o => o.Id == ownerId))
            throw new NotFoundException("Owner", ownerId);

        return await _db.Pets.Where(p => p.OwnerId == ownerId)
            .Select(p => new PetSummaryDto
            {
                Id = p.Id, Name = p.Name, Species = p.Species,
                Breed = p.Breed, IsActive = p.IsActive
            }).ToListAsync();
    }

    public async Task<PaginatedResponse<AppointmentSummaryDto>> GetAppointmentsAsync(int ownerId, int page, int pageSize)
    {
        if (!await _db.Owners.AnyAsync(o => o.Id == ownerId))
            throw new NotFoundException("Owner", ownerId);

        var query = _db.Appointments
            .Include(a => a.Pet).Include(a => a.Veterinarian)
            .Where(a => a.Pet.OwnerId == ownerId);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(a => a.AppointmentDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(a => new AppointmentSummaryDto
            {
                Id = a.Id, AppointmentDate = a.AppointmentDate,
                DurationMinutes = a.DurationMinutes, Status = a.Status,
                Reason = a.Reason,
                Pet = new PetSummaryDto { Id = a.Pet.Id, Name = a.Pet.Name, Species = a.Pet.Species, Breed = a.Pet.Breed, IsActive = a.Pet.IsActive },
                Veterinarian = new VeterinarianSummaryDto { Id = a.Veterinarian.Id, FirstName = a.Veterinarian.FirstName, LastName = a.Veterinarian.LastName, Specialization = a.Veterinarian.Specialization }
            }).ToListAsync();

        return new PaginatedResponse<AppointmentSummaryDto>
        {
            Data = items, Page = page, PageSize = pageSize, TotalCount = totalCount
        };
    }

    private static OwnerDto MapToDto(Owner owner) => new()
    {
        Id = owner.Id, FirstName = owner.FirstName, LastName = owner.LastName,
        Email = owner.Email, Phone = owner.Phone, Address = owner.Address,
        City = owner.City, State = owner.State, ZipCode = owner.ZipCode,
        CreatedAt = owner.CreatedAt, UpdatedAt = owner.UpdatedAt,
        Pets = owner.Pets.Select(p => new PetSummaryDto
        {
            Id = p.Id, Name = p.Name, Species = p.Species,
            Breed = p.Breed, IsActive = p.IsActive
        }).ToList()
    };
}
