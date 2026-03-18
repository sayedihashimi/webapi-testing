using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class VeterinarianService : IVeterinarianService
{
    private readonly VetClinicDbContext _db;
    private readonly ILogger<VeterinarianService> _logger;

    public VeterinarianService(VetClinicDbContext db, ILogger<VeterinarianService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PagedResponse<VeterinarianResponseDto>> GetAllAsync(string? specialization, bool? isAvailable, PaginationParams pagination)
    {
        var query = _db.Veterinarians.AsQueryable();

        if (!string.IsNullOrWhiteSpace(specialization))
            query = query.Where(v => v.Specialization != null && v.Specialization.ToLower().Contains(specialization.ToLower()));

        if (isAvailable.HasValue)
            query = query.Where(v => v.IsAvailable == isAvailable.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(v => v.LastName)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(v => MapToResponse(v))
            .ToListAsync();

        return new PagedResponse<VeterinarianResponseDto> { Items = items, TotalCount = totalCount, Page = pagination.Page, PageSize = pagination.PageSize };
    }

    public async Task<VeterinarianResponseDto?> GetByIdAsync(int id)
    {
        var vet = await _db.Veterinarians.FindAsync(id);
        return vet == null ? null : MapToResponse(vet);
    }

    public async Task<VeterinarianResponseDto> CreateAsync(CreateVeterinarianDto dto)
    {
        if (await _db.Veterinarians.AnyAsync(v => v.Email == dto.Email))
            throw new ArgumentException($"A veterinarian with email '{dto.Email}' already exists.");

        if (await _db.Veterinarians.AnyAsync(v => v.LicenseNumber == dto.LicenseNumber))
            throw new ArgumentException($"A veterinarian with license number '{dto.LicenseNumber}' already exists.");

        var vet = new Veterinarian
        {
            FirstName = dto.FirstName, LastName = dto.LastName, Email = dto.Email,
            Phone = dto.Phone, Specialization = dto.Specialization,
            LicenseNumber = dto.LicenseNumber, IsAvailable = dto.IsAvailable, HireDate = dto.HireDate
        };

        _db.Veterinarians.Add(vet);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Veterinarian created: {VetId} {Name}", vet.Id, $"{vet.FirstName} {vet.LastName}");
        return MapToResponse(vet);
    }

    public async Task<VeterinarianResponseDto> UpdateAsync(int id, UpdateVeterinarianDto dto)
    {
        var vet = await _db.Veterinarians.FindAsync(id) ?? throw new KeyNotFoundException($"Veterinarian with ID {id} not found.");

        if (await _db.Veterinarians.AnyAsync(v => v.Email == dto.Email && v.Id != id))
            throw new ArgumentException($"A veterinarian with email '{dto.Email}' already exists.");

        if (await _db.Veterinarians.AnyAsync(v => v.LicenseNumber == dto.LicenseNumber && v.Id != id))
            throw new ArgumentException($"A veterinarian with license number '{dto.LicenseNumber}' already exists.");

        vet.FirstName = dto.FirstName; vet.LastName = dto.LastName; vet.Email = dto.Email;
        vet.Phone = dto.Phone; vet.Specialization = dto.Specialization;
        vet.LicenseNumber = dto.LicenseNumber; vet.IsAvailable = dto.IsAvailable; vet.HireDate = dto.HireDate;

        await _db.SaveChangesAsync();
        return MapToResponse(vet);
    }

    public async Task<List<AppointmentResponseDto>> GetScheduleAsync(int vetId, DateOnly date)
    {
        if (!await _db.Veterinarians.AnyAsync(v => v.Id == vetId))
            throw new KeyNotFoundException($"Veterinarian with ID {vetId} not found.");

        var startOfDay = date.ToDateTime(TimeOnly.MinValue);
        var endOfDay = date.ToDateTime(TimeOnly.MaxValue);

        var appointments = await _db.Appointments
            .Include(a => a.Pet).Include(a => a.Veterinarian).Include(a => a.MedicalRecord)
            .Where(a => a.VeterinarianId == vetId && a.AppointmentDate >= startOfDay && a.AppointmentDate <= endOfDay)
            .OrderBy(a => a.AppointmentDate)
            .ToListAsync();

        return appointments.Select(AppointmentService.MapToResponse).ToList();
    }

    public async Task<PagedResponse<AppointmentResponseDto>> GetAppointmentsAsync(int vetId, string? status, PaginationParams pagination)
    {
        if (!await _db.Veterinarians.AnyAsync(v => v.Id == vetId))
            throw new KeyNotFoundException($"Veterinarian with ID {vetId} not found.");

        var query = _db.Appointments
            .Include(a => a.Pet).Include(a => a.Veterinarian).Include(a => a.MedicalRecord)
            .Where(a => a.VeterinarianId == vetId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<AppointmentStatus>(status, true, out var statusEnum))
            query = query.Where(a => a.Status == statusEnum);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.AppointmentDate)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return new PagedResponse<AppointmentResponseDto>
        {
            Items = items.Select(AppointmentService.MapToResponse).ToList(),
            TotalCount = totalCount, Page = pagination.Page, PageSize = pagination.PageSize
        };
    }

    public static VeterinarianResponseDto MapToResponse(Veterinarian v) => new()
    {
        Id = v.Id, FirstName = v.FirstName, LastName = v.LastName, Email = v.Email,
        Phone = v.Phone, Specialization = v.Specialization, LicenseNumber = v.LicenseNumber,
        IsAvailable = v.IsAvailable, HireDate = v.HireDate
    };
}
