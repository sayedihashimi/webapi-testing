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

    public async Task<PagedResult<VetResponseDto>> GetAllAsync(string? specialization, bool? isAvailable, PaginationParams pagination)
    {
        var query = _db.Veterinarians.AsQueryable();
        if (!string.IsNullOrWhiteSpace(specialization))
            query = query.Where(v => v.Specialization != null && v.Specialization.ToLower().Contains(specialization.ToLower()));
        if (isAvailable.HasValue)
            query = query.Where(v => v.IsAvailable == isAvailable.Value);
        query = query.OrderBy(v => v.LastName);
        return await query.Select(v => v.ToResponseDto()).ToPagedResultAsync(pagination);
    }

    public async Task<VetResponseDto?> GetByIdAsync(int id)
    {
        var vet = await _db.Veterinarians.FindAsync(id);
        return vet?.ToResponseDto();
    }

    public async Task<VetResponseDto> CreateAsync(VetCreateDto dto)
    {
        var vet = new Veterinarian
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            Phone = dto.Phone,
            Specialization = dto.Specialization,
            LicenseNumber = dto.LicenseNumber,
            HireDate = dto.HireDate
        };
        _db.Veterinarians.Add(vet);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Veterinarian created: {VetId} Dr. {Name}", vet.Id, $"{vet.FirstName} {vet.LastName}");
        return vet.ToResponseDto();
    }

    public async Task<VetResponseDto?> UpdateAsync(int id, VetUpdateDto dto)
    {
        var vet = await _db.Veterinarians.FindAsync(id);
        if (vet is null) return null;
        vet.FirstName = dto.FirstName;
        vet.LastName = dto.LastName;
        vet.Email = dto.Email;
        vet.Phone = dto.Phone;
        vet.Specialization = dto.Specialization;
        vet.LicenseNumber = dto.LicenseNumber;
        vet.IsAvailable = dto.IsAvailable;
        vet.HireDate = dto.HireDate;
        await _db.SaveChangesAsync();
        return vet.ToResponseDto();
    }

    public async Task<IEnumerable<AppointmentResponseDto>> GetScheduleAsync(int vetId, DateOnly date)
    {
        var startOfDay = date.ToDateTime(TimeOnly.MinValue);
        var endOfDay = date.ToDateTime(TimeOnly.MaxValue);
        return await _db.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Where(a => a.VeterinarianId == vetId && a.AppointmentDate >= startOfDay && a.AppointmentDate <= endOfDay)
            .OrderBy(a => a.AppointmentDate)
            .Select(a => a.ToResponseDto())
            .ToListAsync();
    }

    public async Task<PagedResult<AppointmentResponseDto>> GetAppointmentsAsync(int vetId, string? status, PaginationParams pagination)
    {
        var query = _db.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Where(a => a.VeterinarianId == vetId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<AppointmentStatus>(status, true, out var statusEnum))
            query = query.Where(a => a.Status == statusEnum);

        query = query.OrderByDescending(a => a.AppointmentDate);
        return await query.Select(a => a.ToResponseDto()).ToPagedResultAsync(pagination);
    }
}
