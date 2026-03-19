using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class VeterinarianService(VetClinicDbContext context, ILogger<VeterinarianService> logger) : IVeterinarianService
{
    public async Task<PagedResult<VeterinarianResponse>> GetAllAsync(string? specialization, bool? isAvailable, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = context.Veterinarians.AsQueryable();

        if (!string.IsNullOrWhiteSpace(specialization))
        {
            query = query.Where(v => v.Specialization != null && v.Specialization.ToLower().Contains(specialization.ToLower()));
        }

        if (isAvailable.HasValue)
        {
            query = query.Where(v => v.IsAvailable == isAvailable.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(v => v.LastName).ThenBy(v => v.FirstName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(v => MapToResponse(v))
            .ToListAsync(cancellationToken);

        return new PagedResult<VeterinarianResponse>(items, totalCount, page, pageSize);
    }

    public async Task<VeterinarianResponse?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var vet = await context.Veterinarians.FindAsync([id], cancellationToken);
        return vet is null ? null : MapToResponse(vet);
    }

    public async Task<VeterinarianResponse> CreateAsync(CreateVeterinarianRequest request, CancellationToken cancellationToken)
    {
        var vet = new Veterinarian
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Phone = request.Phone,
            Specialization = request.Specialization,
            LicenseNumber = request.LicenseNumber,
            HireDate = request.HireDate
        };

        context.Veterinarians.Add(vet);
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Veterinarian created: {VetId} {FirstName} {LastName}", vet.Id, vet.FirstName, vet.LastName);

        return MapToResponse(vet);
    }

    public async Task<VeterinarianResponse?> UpdateAsync(int id, UpdateVeterinarianRequest request, CancellationToken cancellationToken)
    {
        var vet = await context.Veterinarians.FindAsync([id], cancellationToken);
        if (vet is null)
        {
            return null;
        }

        vet.FirstName = request.FirstName;
        vet.LastName = request.LastName;
        vet.Email = request.Email;
        vet.Phone = request.Phone;
        vet.Specialization = request.Specialization;
        vet.LicenseNumber = request.LicenseNumber;
        vet.IsAvailable = request.IsAvailable;

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Veterinarian updated: {VetId}", vet.Id);

        return MapToResponse(vet);
    }

    public async Task<IReadOnlyList<AppointmentResponse>> GetScheduleAsync(int vetId, DateOnly date, CancellationToken cancellationToken)
    {
        var startOfDay = date.ToDateTime(TimeOnly.MinValue);
        var endOfDay = date.ToDateTime(TimeOnly.MaxValue);

        return await context.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Where(a => a.VeterinarianId == vetId &&
                        a.AppointmentDate >= startOfDay &&
                        a.AppointmentDate <= endOfDay)
            .OrderBy(a => a.AppointmentDate)
            .Select(a => new AppointmentResponse(
                a.Id, a.PetId, a.Pet.Name,
                a.VeterinarianId, $"{a.Veterinarian.FirstName} {a.Veterinarian.LastName}",
                a.AppointmentDate, a.DurationMinutes, a.Status,
                a.Reason, a.Notes, a.CancellationReason,
                a.CreatedAt, a.UpdatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<PagedResult<AppointmentResponse>> GetAppointmentsAsync(int vetId, string? status, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = context.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
            .Where(a => a.VeterinarianId == vetId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<AppointmentStatus>(status, true, out var statusEnum))
        {
            query = query.Where(a => a.Status == statusEnum);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(a => a.AppointmentDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new AppointmentResponse(
                a.Id, a.PetId, a.Pet.Name,
                a.VeterinarianId, $"{a.Veterinarian.FirstName} {a.Veterinarian.LastName}",
                a.AppointmentDate, a.DurationMinutes, a.Status,
                a.Reason, a.Notes, a.CancellationReason,
                a.CreatedAt, a.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<AppointmentResponse>(items, totalCount, page, pageSize);
    }

    private static VeterinarianResponse MapToResponse(Veterinarian vet) =>
        new(vet.Id, vet.FirstName, vet.LastName, vet.Email, vet.Phone,
            vet.Specialization, vet.LicenseNumber, vet.IsAvailable, vet.HireDate);
}
