using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public class PrescriptionService : IPrescriptionService
{
    private readonly VetClinicDbContext _db;
    private readonly ILogger<PrescriptionService> _logger;

    public PrescriptionService(VetClinicDbContext db, ILogger<PrescriptionService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PrescriptionResponseDto?> GetByIdAsync(int id)
    {
        var rx = await _db.Prescriptions.Include(p => p.MedicalRecord).FirstOrDefaultAsync(p => p.Id == id);
        return rx?.ToResponseDto();
    }

    public async Task<(PrescriptionResponseDto? Result, string? Error)> CreateAsync(PrescriptionCreateDto dto)
    {
        var record = await _db.MedicalRecords.FindAsync(dto.MedicalRecordId);
        if (record is null)
            return (null, "Medical record not found.");

        if (dto.DurationDays <= 0)
            return (null, "DurationDays must be positive.");

        var rx = new Prescription
        {
            MedicalRecordId = dto.MedicalRecordId,
            MedicationName = dto.MedicationName,
            Dosage = dto.Dosage,
            DurationDays = dto.DurationDays,
            StartDate = dto.StartDate,
            EndDate = dto.StartDate.AddDays(dto.DurationDays),
            Instructions = dto.Instructions,
            CreatedAt = DateTime.UtcNow
        };

        _db.Prescriptions.Add(rx);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Prescription created: {RxId} for medical record {RecordId}", rx.Id, rx.MedicalRecordId);
        return (rx.ToResponseDto(), null);
    }
}
