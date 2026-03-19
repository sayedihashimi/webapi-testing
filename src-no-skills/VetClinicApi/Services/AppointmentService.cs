using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Middleware;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class AppointmentService : IAppointmentService
{
    private readonly VetClinicDbContext _context;
    private readonly ILogger<AppointmentService> _logger;

    public AppointmentService(VetClinicDbContext context, ILogger<AppointmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaginatedResponse<AppointmentResponseDto>> GetAllAsync(
        DateTime? fromDate, DateTime? toDate, string? status, int? vetId, int? petId, int page, int pageSize)
    {
        var query = _context.Appointments
            .Include(a => a.Pet).Include(a => a.Veterinarian).Include(a => a.MedicalRecord)
            .AsQueryable();

        if (fromDate.HasValue) query = query.Where(a => a.AppointmentDate >= fromDate.Value);
        if (toDate.HasValue) query = query.Where(a => a.AppointmentDate <= toDate.Value);
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<AppointmentStatus>(status, true, out var statusEnum))
            query = query.Where(a => a.Status == statusEnum);
        if (vetId.HasValue) query = query.Where(a => a.VeterinarianId == vetId.Value);
        if (petId.HasValue) query = query.Where(a => a.PetId == petId.Value);

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.AppointmentDate)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync();

        return new PaginatedResponse<AppointmentResponseDto>
        {
            Items = items.Select(MapToResponse).ToList(),
            TotalCount = totalCount, Page = page, PageSize = pageSize
        };
    }

    public async Task<AppointmentResponseDto> GetByIdAsync(int id)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Pet).Include(a => a.Veterinarian).Include(a => a.MedicalRecord)
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new KeyNotFoundException($"Appointment with ID {id} not found.");

        return MapToResponse(appointment);
    }

    public async Task<AppointmentResponseDto> CreateAsync(CreateAppointmentDto dto)
    {
        if (!await _context.Pets.AnyAsync(p => p.Id == dto.PetId))
            throw new BusinessRuleException($"Pet with ID {dto.PetId} not found.");
        if (!await _context.Veterinarians.AnyAsync(v => v.Id == dto.VeterinarianId))
            throw new BusinessRuleException($"Veterinarian with ID {dto.VeterinarianId} not found.");

        if (dto.AppointmentDate <= DateTime.UtcNow)
            throw new BusinessRuleException("Appointment date must be in the future.");

        await CheckSchedulingConflict(dto.VeterinarianId, dto.AppointmentDate, dto.DurationMinutes, null);

        var appointment = new Appointment
        {
            PetId = dto.PetId, VeterinarianId = dto.VeterinarianId,
            AppointmentDate = dto.AppointmentDate, DurationMinutes = dto.DurationMinutes,
            Reason = dto.Reason, Notes = dto.Notes,
            Status = AppointmentStatus.Scheduled,
            CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
        };

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Appointment created: {AppointmentId} for Pet {PetId} with Vet {VetId}", appointment.Id, appointment.PetId, appointment.VeterinarianId);

        await _context.Entry(appointment).Reference(a => a.Pet).LoadAsync();
        await _context.Entry(appointment).Reference(a => a.Veterinarian).LoadAsync();
        return MapToResponse(appointment);
    }

    public async Task<AppointmentResponseDto> UpdateAsync(int id, UpdateAppointmentDto dto)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Pet).Include(a => a.Veterinarian).Include(a => a.MedicalRecord)
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new KeyNotFoundException($"Appointment with ID {id} not found.");

        if (appointment.Status == AppointmentStatus.Completed || appointment.Status == AppointmentStatus.Cancelled || appointment.Status == AppointmentStatus.NoShow)
            throw new BusinessRuleException($"Cannot update appointment in {appointment.Status} status.");

        if (!await _context.Pets.AnyAsync(p => p.Id == dto.PetId))
            throw new BusinessRuleException($"Pet with ID {dto.PetId} not found.");
        if (!await _context.Veterinarians.AnyAsync(v => v.Id == dto.VeterinarianId))
            throw new BusinessRuleException($"Veterinarian with ID {dto.VeterinarianId} not found.");

        if (dto.AppointmentDate != appointment.AppointmentDate || dto.VeterinarianId != appointment.VeterinarianId || dto.DurationMinutes != appointment.DurationMinutes)
            await CheckSchedulingConflict(dto.VeterinarianId, dto.AppointmentDate, dto.DurationMinutes, id);

        appointment.PetId = dto.PetId;
        appointment.VeterinarianId = dto.VeterinarianId;
        appointment.AppointmentDate = dto.AppointmentDate;
        appointment.DurationMinutes = dto.DurationMinutes;
        appointment.Reason = dto.Reason;
        appointment.Notes = dto.Notes;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapToResponse(appointment);
    }

    public async Task<AppointmentResponseDto> UpdateStatusAsync(int id, UpdateAppointmentStatusDto dto)
    {
        var appointment = await _context.Appointments
            .Include(a => a.Pet).Include(a => a.Veterinarian).Include(a => a.MedicalRecord)
            .FirstOrDefaultAsync(a => a.Id == id)
            ?? throw new KeyNotFoundException($"Appointment with ID {id} not found.");

        ValidateStatusTransition(appointment.Status, dto.Status);

        if (dto.Status == AppointmentStatus.Cancelled)
        {
            if (string.IsNullOrWhiteSpace(dto.CancellationReason))
                throw new BusinessRuleException("CancellationReason is required when cancelling an appointment.");

            if (appointment.AppointmentDate < DateTime.UtcNow)
                throw new BusinessRuleException("Cannot cancel a past appointment.");

            appointment.CancellationReason = dto.CancellationReason;
        }

        appointment.Status = dto.Status;
        appointment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Appointment {AppointmentId} status changed to {Status}", id, dto.Status);

        return MapToResponse(appointment);
    }

    public async Task<List<AppointmentResponseDto>> GetTodayAsync()
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var appointments = await _context.Appointments
            .Include(a => a.Pet).Include(a => a.Veterinarian).Include(a => a.MedicalRecord)
            .Where(a => a.AppointmentDate >= today && a.AppointmentDate < tomorrow)
            .OrderBy(a => a.AppointmentDate)
            .ToListAsync();

        return appointments.Select(MapToResponse).ToList();
    }

    private async Task CheckSchedulingConflict(int vetId, DateTime appointmentDate, int durationMinutes, int? excludeAppointmentId)
    {
        var newEnd = appointmentDate.AddMinutes(durationMinutes);

        var conflicting = await _context.Appointments
            .Where(a => a.VeterinarianId == vetId
                && a.Status != AppointmentStatus.Cancelled
                && a.Status != AppointmentStatus.NoShow
                && (!excludeAppointmentId.HasValue || a.Id != excludeAppointmentId.Value))
            .ToListAsync();

        var hasConflict = conflicting.Any(a =>
        {
            var existingEnd = a.AppointmentDate.AddMinutes(a.DurationMinutes);
            return appointmentDate < existingEnd && newEnd > a.AppointmentDate;
        });

        if (hasConflict)
            throw new BusinessRuleException("Scheduling conflict: the veterinarian already has an appointment during this time.", 409, "Scheduling Conflict");
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
            throw new BusinessRuleException($"Invalid status transition from {current} to {next}.");
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
