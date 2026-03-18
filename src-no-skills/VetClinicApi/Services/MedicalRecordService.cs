using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class MedicalRecordService : IMedicalRecordService
{
    private readonly VetClinicDbContext _db;
    private readonly ILogger<MedicalRecordService> _logger;

    public MedicalRecordService(VetClinicDbContext db, ILogger<MedicalRecordService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<MedicalRecordResponseDto?> GetByIdAsync(int id)
    {
        var record = await _db.MedicalRecords
            .Include(m => m.Pet).Include(m => m.Veterinarian).Include(m => m.Prescriptions)
            .FirstOrDefaultAsync(m => m.Id == id);
        return record == null ? null : MapToResponse(record);
    }

    public async Task<MedicalRecordResponseDto> CreateAsync(CreateMedicalRecordDto dto)
    {
        var appointment = await _db.Appointments.FindAsync(dto.AppointmentId)
            ?? throw new KeyNotFoundException($"Appointment with ID {dto.AppointmentId} not found.");

        if (appointment.Status != AppointmentStatus.Completed && appointment.Status != AppointmentStatus.InProgress)
            throw new ArgumentException("Medical records can only be created for appointments with status 'Completed' or 'InProgress'.");

        if (await _db.MedicalRecords.AnyAsync(m => m.AppointmentId == dto.AppointmentId))
            throw new InvalidOperationException("A medical record already exists for this appointment.");

        if (!await _db.Pets.AnyAsync(p => p.Id == dto.PetId))
            throw new KeyNotFoundException($"Pet with ID {dto.PetId} not found.");
        if (!await _db.Veterinarians.AnyAsync(v => v.Id == dto.VeterinarianId))
            throw new KeyNotFoundException($"Veterinarian with ID {dto.VeterinarianId} not found.");

        var record = new MedicalRecord
        {
            AppointmentId = dto.AppointmentId, PetId = dto.PetId, VeterinarianId = dto.VeterinarianId,
            Diagnosis = dto.Diagnosis, Treatment = dto.Treatment, Notes = dto.Notes,
            FollowUpDate = dto.FollowUpDate, CreatedAt = DateTime.UtcNow
        };

        _db.MedicalRecords.Add(record);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Medical record created: {RecordId} for Appointment {AppointmentId}", record.Id, record.AppointmentId);

        return (await GetByIdAsync(record.Id))!;
    }

    public async Task<MedicalRecordResponseDto> UpdateAsync(int id, UpdateMedicalRecordDto dto)
    {
        var record = await _db.MedicalRecords
            .Include(m => m.Pet).Include(m => m.Veterinarian).Include(m => m.Prescriptions)
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new KeyNotFoundException($"Medical record with ID {id} not found.");

        record.Diagnosis = dto.Diagnosis;
        record.Treatment = dto.Treatment;
        record.Notes = dto.Notes;
        record.FollowUpDate = dto.FollowUpDate;

        await _db.SaveChangesAsync();
        return MapToResponse(record);
    }

    public static MedicalRecordResponseDto MapToResponse(MedicalRecord m) => new()
    {
        Id = m.Id, AppointmentId = m.AppointmentId, PetId = m.PetId, VeterinarianId = m.VeterinarianId,
        Diagnosis = m.Diagnosis, Treatment = m.Treatment, Notes = m.Notes, FollowUpDate = m.FollowUpDate,
        CreatedAt = m.CreatedAt,
        Pet = m.Pet != null ? new PetSummaryDto { Id = m.Pet.Id, Name = m.Pet.Name, Species = m.Pet.Species, Breed = m.Pet.Breed, IsActive = m.Pet.IsActive } : null,
        Veterinarian = m.Veterinarian != null ? new VeterinarianSummaryDto { Id = m.Veterinarian.Id, FirstName = m.Veterinarian.FirstName, LastName = m.Veterinarian.LastName, Specialization = m.Veterinarian.Specialization } : null,
        Prescriptions = m.Prescriptions?.Select(PrescriptionService.MapToResponse).ToList() ?? new()
    };
}
