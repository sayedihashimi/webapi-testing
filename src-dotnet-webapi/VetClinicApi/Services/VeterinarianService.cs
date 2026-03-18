using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class VeterinarianService(VetClinicDbContext db, ILogger<VeterinarianService> logger) : IVeterinarianService
{
    public async Task<PagedResponse<VeterinarianResponse>> GetAllAsync(string? specialization, bool? isAvailable, int page, int pageSize, CancellationToken ct)
    {
        var query = db.Veterinarians.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(specialization))
            query = query.Where(v => v.Specialization != null && v.Specialization.ToLower().Contains(specialization.ToLower()));

        if (isAvailable.HasValue)
            query = query.Where(v => v.IsAvailable == isAvailable.Value);

        var totalCount = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await query
            .OrderBy(v => v.LastName).ThenBy(v => v.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => MapToResponse(v))
            .ToListAsync(ct);

        return new PagedResponse<VeterinarianResponse>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
    }

    public async Task<VeterinarianResponse?> GetByIdAsync(int id, CancellationToken ct)
    {
        var vet = await db.Veterinarians.AsNoTracking().FirstOrDefaultAsync(v => v.Id == id, ct);
        return vet is null ? null : MapToResponse(vet);
    }

    public async Task<VeterinarianResponse> CreateAsync(CreateVeterinarianRequest request, CancellationToken ct)
    {
        var emailExists = await db.Veterinarians.AsNoTracking().AnyAsync(v => v.Email == request.Email, ct);
        if (emailExists)
            throw new InvalidOperationException($"A veterinarian with email '{request.Email}' already exists.");

        var licenseExists = await db.Veterinarians.AsNoTracking().AnyAsync(v => v.LicenseNumber == request.LicenseNumber, ct);
        if (licenseExists)
            throw new InvalidOperationException($"A veterinarian with license number '{request.LicenseNumber}' already exists.");

        var vet = new Veterinarian
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Specialization = request.Specialization,
            LicenseNumber = request.LicenseNumber,
            IsAvailable = request.IsAvailable,
            HireDate = request.HireDate
        };

        db.Veterinarians.Add(vet);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Created veterinarian {VetId}", vet.Id);

        return MapToResponse(vet);
    }

    public async Task<VeterinarianResponse?> UpdateAsync(int id, UpdateVeterinarianRequest request, CancellationToken ct)
    {
        var vet = await db.Veterinarians.FirstOrDefaultAsync(v => v.Id == id, ct);
        if (vet is null) return null;

        var emailConflict = await db.Veterinarians.AsNoTracking().AnyAsync(v => v.Email == request.Email && v.Id != id, ct);
        if (emailConflict)
            throw new InvalidOperationException($"A veterinarian with email '{request.Email}' already exists.");

        var licenseConflict = await db.Veterinarians.AsNoTracking().AnyAsync(v => v.LicenseNumber == request.LicenseNumber && v.Id != id, ct);
        if (licenseConflict)
            throw new InvalidOperationException($"A veterinarian with license number '{request.LicenseNumber}' already exists.");

        vet.FirstName = request.FirstName;
        vet.LastName = request.LastName;
        vet.Email = request.Email;
        vet.Phone = request.Phone;
        vet.Specialization = request.Specialization;
        vet.LicenseNumber = request.LicenseNumber;
        vet.IsAvailable = request.IsAvailable;
        vet.HireDate = request.HireDate;

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Updated veterinarian {VetId}", vet.Id);

        return MapToResponse(vet);
    }

    public async Task<List<AppointmentResponse>> GetScheduleAsync(int vetId, DateOnly date, CancellationToken ct)
    {
        var vetExists = await db.Veterinarians.AsNoTracking().AnyAsync(v => v.Id == vetId, ct);
        if (!vetExists)
            throw new KeyNotFoundException($"Veterinarian with ID {vetId} not found.");

        var startOfDay = date.ToDateTime(TimeOnly.MinValue);
        var endOfDay = date.ToDateTime(TimeOnly.MaxValue);

        var appointments = await db.Appointments.AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Where(a => a.VeterinarianId == vetId && a.AppointmentDate >= startOfDay && a.AppointmentDate <= endOfDay)
            .OrderBy(a => a.AppointmentDate)
            .ToListAsync(ct);

        return appointments.Select(MapAppointmentToResponse).ToList();
    }

    public async Task<PagedResponse<AppointmentResponse>> GetAppointmentsAsync(int vetId, string? status, int page, int pageSize, CancellationToken ct)
    {
        var vetExists = await db.Veterinarians.AsNoTracking().AnyAsync(v => v.Id == vetId, ct);
        if (!vetExists)
            throw new KeyNotFoundException($"Veterinarian with ID {vetId} not found.");

        var query = db.Appointments.AsNoTracking()
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Where(a => a.VeterinarianId == vetId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<AppointmentStatus>(status, true, out var parsedStatus))
            query = query.Where(a => a.Status == parsedStatus);

        var totalCount = await query.CountAsync(ct);
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var items = await query
            .OrderByDescending(a => a.AppointmentDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResponse<AppointmentResponse>
        {
            Items = items.Select(MapAppointmentToResponse).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPage = page < totalPages,
            HasPreviousPage = page > 1
        };
    }

    private static VeterinarianResponse MapToResponse(Veterinarian vet) => new()
    {
        Id = vet.Id,
        FirstName = vet.FirstName,
        LastName = vet.LastName,
        Email = vet.Email,
        Phone = vet.Phone,
        Specialization = vet.Specialization,
        LicenseNumber = vet.LicenseNumber,
        IsAvailable = vet.IsAvailable,
        HireDate = vet.HireDate
    };

    private static AppointmentResponse MapAppointmentToResponse(Appointment a) => new()
    {
        Id = a.Id,
        PetId = a.PetId,
        VeterinarianId = a.VeterinarianId,
        AppointmentDate = a.AppointmentDate,
        DurationMinutes = a.DurationMinutes,
        Status = a.Status,
        Reason = a.Reason,
        Notes = a.Notes,
        CancellationReason = a.CancellationReason,
        CreatedAt = a.CreatedAt,
        UpdatedAt = a.UpdatedAt,
        Pet = a.Pet != null ? new PetSummaryResponse
        {
            Id = a.Pet.Id,
            Name = a.Pet.Name,
            Species = a.Pet.Species,
            Breed = a.Pet.Breed,
            IsActive = a.Pet.IsActive
        } : null,
        Veterinarian = a.Veterinarian != null ? new VeterinarianSummaryResponse
        {
            Id = a.Veterinarian.Id,
            FirstName = a.Veterinarian.FirstName,
            LastName = a.Veterinarian.LastName,
            Specialization = a.Veterinarian.Specialization
        } : null
    };
}
