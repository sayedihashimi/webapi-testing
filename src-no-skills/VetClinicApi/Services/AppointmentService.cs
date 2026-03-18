using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class AppointmentService : IAppointmentService
{
    private readonly VetClinicDbContext _db;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(VetClinicDbContext db, ILogger<AppointmentService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PagedResponse<AppointmentResponseDto>> GetAllAsync(DateTime? dateFrom, DateTime? dateTo, AppointmentStatus? status, int? vetId, int? petId, PaginationParams pagination)
    {
        var query = _db.Appointments
            .Include(a => a.Pet).Include(a => a.Veterinarian).Include(a => a.MedicalRecord)
            .AsQueryable();

        if (dateFrom.HasValue) query = query.Where(a => a.AppointmentDate >= dateFrom.Value);
        if (dateTo.HasValue) query = query.Where(a => a.AppointmentDate <= dateTo.Value);
        if (status.HasValue) query = query.Where(a => a.Status == status.Value);
        if (vetId.HasValue) query = query.Where(a => a.VeterinarianId == vetId.Value);
        if (petId.HasValue) query = query.Where(a => a.PetId == petId.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.AppointmentDate)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return new PagedResponse<AppointmentResponseDto>
        {
            Items = items.Select(MapToResponse).ToList(),
            TotalCount = totalCount, Page = pagination.Page, PageSize = pagination.PageSize
        };
    }

    public async Task<AppointmentResponseDto?> GetByIdAsync(int id)
    {
        var appt = await _db.Appointments
            .Include(a => a.Pet).Include(a => a.Veterinarian).Include(a => a.MedicalRecord)
            .FirstOrDefaultAsync(a => a.Id == id);
        return appt == null ? null : MapToResponse(appt);
    }

    public async Task<AppointmentResponseDto> CreateAsync(CreateAppointmentDto dto)
    {
        if (!await _db.Pets.AnyAsync(p => p.Id == dto.PetId))
            throw new KeyNotFoundException($"Pet with ID {dto.PetId} not found.");
        if (!await _db.Veterinarians.AnyAsync(v => v.Id == dto.VeterinarianId))
            throw new KeyNotFoundException($"Veterinarian with ID {dto.VeterinarianId} not found.");

        if (dto.AppointmentDate <= DateTime.UtcNow)
            throw new ArgumentException("Appointment date must be in the future.");

        await CheckSchedulingConflict(dto.VeterinarianId, dto.AppointmentDate, dto.DurationMinutes, null);

        var appt = new Appointment
        {
            PetId = dto.PetId, VeterinarianId = dto.VeterinarianId,
            AppointmentDate = dto.AppointmentDate, DurationMinutes = dto.DurationMinutes,
            Reason = dto.Reason, Notes = dto.Notes,
            Status = AppointmentStatus.Scheduled,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };

        _db.Appointments.Add(appt);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Appointment created: {AppointmentId} for Pet {PetId} with Vet {VetId}", appt.Id, appt.PetId, appt.VeterinarianId);

        return (await GetByIdAsync(appt.Id))!;
    }

    public async Task<AppointmentResponseDto> UpdateAsync(int id, UpdateAppointmentDto dto)
    {
        var appt = await _db.Appointments
            .Include(a => a.Pet).Include(a => a.Veterinarian).Include(a => a.MedicalRecord)
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new KeyNotFoundException($"Appointment with ID {id} not found.");

        if (appt.Status == AppointmentStatus.Completed || appt.Status == AppointmentStatus.Cancelled || appt.Status == AppointmentStatus.NoShow)
            throw new InvalidOperationException($"Cannot update an appointment with status '{appt.Status}'.");

        if (!await _db.Pets.AnyAsync(p => p.Id == dto.PetId))
            throw new KeyNotFoundException($"Pet with ID {dto.PetId} not found.");
        if (!await _db.Veterinarians.AnyAsync(v => v.Id == dto.VeterinarianId))
            throw new KeyNotFoundException($"Veterinarian with ID {dto.VeterinarianId} not found.");

        await CheckSchedulingConflict(dto.VeterinarianId, dto.AppointmentDate, dto.DurationMinutes, id);

        appt.PetId = dto.PetId; appt.VeterinarianId = dto.VeterinarianId;
        appt.AppointmentDate = dto.AppointmentDate; appt.DurationMinutes = dto.DurationMinutes;
        appt.Reason = dto.Reason; appt.Notes = dto.Notes;
        appt.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return MapToResponse(appt);
    }

    public async Task<AppointmentResponseDto> UpdateStatusAsync(int id, UpdateAppointmentStatusDto dto)
    {
        var appt = await _db.Appointments
            .Include(a => a.Pet).Include(a => a.Veterinarian).Include(a => a.MedicalRecord)
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new KeyNotFoundException($"Appointment with ID {id} not found.");

        ValidateStatusTransition(appt.Status, dto.Status);

        if (dto.Status == AppointmentStatus.Cancelled)
        {
            if (string.IsNullOrWhiteSpace(dto.CancellationReason))
                throw new ArgumentException("A cancellation reason is required when cancelling an appointment.");
            if (appt.AppointmentDate < DateTime.UtcNow)
                throw new ArgumentException("Cannot cancel a past appointment.");
            appt.CancellationReason = dto.CancellationReason;
        }

        appt.Status = dto.Status;
        appt.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        _logger.LogInformation("Appointment {AppointmentId} status changed to {Status}", id, dto.Status);
        return MapToResponse(appt);
    }

    public async Task<List<AppointmentResponseDto>> GetTodayAsync()
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var appointments = await _db.Appointments
            .Include(a => a.Pet).Include(a => a.Veterinarian).Include(a => a.MedicalRecord)
            .Where(a => a.AppointmentDate >= today && a.AppointmentDate < tomorrow)
            .OrderBy(a => a.AppointmentDate)
            .ToListAsync();

        return appointments.Select(MapToResponse).ToList();
    }

    private async Task CheckSchedulingConflict(int vetId, DateTime proposedStart, int durationMinutes, int? excludeAppointmentId)
    {
        var proposedEnd = proposedStart.AddMinutes(durationMinutes);

        var conflict = await _db.Appointments
            .Where(a => a.VeterinarianId == vetId
                && a.Status != AppointmentStatus.Cancelled
                && a.Status != AppointmentStatus.NoShow
                && (excludeAppointmentId == null || a.Id != excludeAppointmentId))
            .AnyAsync(a =>
                proposedStart < a.AppointmentDate.AddMinutes(a.DurationMinutes)
                && proposedEnd > a.AppointmentDate);

        if (conflict)
            throw new InvalidOperationException("The veterinarian has a scheduling conflict at the requested time.");
    }

    private static void ValidateStatusTransition(AppointmentStatus current, AppointmentStatus next)
    {
        var validTransitions = new Dictionary<AppointmentStatus, AppointmentStatus[]>
        {
            { AppointmentStatus.Scheduled, new[] { AppointmentStatus.CheckedIn, AppointmentStatus.Cancelled, AppointmentStatus.NoShow } },
            { AppointmentStatus.CheckedIn, new[] { AppointmentStatus.InProgress, AppointmentStatus.Cancelled } },
            { AppointmentStatus.InProgress, new[] { AppointmentStatus.Completed } },
        };

        if (!validTransitions.TryGetValue(current, out var allowed) || !allowed.Contains(next))
            throw new ArgumentException($"Invalid status transition from '{current}' to '{next}'.");
    }

    public static AppointmentResponseDto MapToResponse(Appointment a) => new()
    {
        Id = a.Id, PetId = a.PetId, VeterinarianId = a.VeterinarianId,
        AppointmentDate = a.AppointmentDate, DurationMinutes = a.DurationMinutes,
        Status = a.Status, Reason = a.Reason, Notes = a.Notes,
        CancellationReason = a.CancellationReason,
        CreatedAt = a.CreatedAt, UpdatedAt = a.UpdatedAt,
        Pet = a.Pet != null ? new PetSummaryDto { Id = a.Pet.Id, Name = a.Pet.Name, Species = a.Pet.Species, Breed = a.Pet.Breed, IsActive = a.Pet.IsActive } : null,
        Veterinarian = a.Veterinarian != null ? new VeterinarianSummaryDto { Id = a.Veterinarian.Id, FirstName = a.Veterinarian.FirstName, LastName = a.Veterinarian.LastName, Specialization = a.Veterinarian.Specialization } : null,
        MedicalRecord = a.MedicalRecord != null ? new MedicalRecordSummaryDto { Id = a.MedicalRecord.Id, Diagnosis = a.MedicalRecord.Diagnosis, Treatment = a.MedicalRecord.Treatment, CreatedAt = a.MedicalRecord.CreatedAt } : null
    };
}
