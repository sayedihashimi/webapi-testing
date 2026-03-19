using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class PrescriptionService(VetClinicDbContext db, ILogger<PrescriptionService> logger) : IPrescriptionService
{
    public async Task<PrescriptionDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var prescription = await db.Prescriptions.FindAsync([id], ct);
        if (prescription is null)
        {
            return null;
        }

        return MapToDto(prescription);
    }

    public async Task<PrescriptionDto> CreateAsync(CreatePrescriptionDto dto, CancellationToken ct = default)
    {
        var recordExists = await db.MedicalRecords.AnyAsync(m => m.Id == dto.MedicalRecordId, ct);
        if (!recordExists)
        {
            throw new InvalidOperationException($"Medical record {dto.MedicalRecordId} not found.");
        }

        var prescription = new Prescription
        {
            MedicalRecordId = dto.MedicalRecordId,
            MedicationName = dto.MedicationName,
            Dosage = dto.Dosage,
            DurationDays = dto.DurationDays,
            StartDate = dto.StartDate,
            Instructions = dto.Instructions
        };

        db.Prescriptions.Add(prescription);
        await db.SaveChangesAsync(ct);

        logger.LogInformation("Created prescription {PrescriptionId} for medical record {RecordId}",
            prescription.Id, prescription.MedicalRecordId);

        return MapToDto(prescription);
    }

    private static PrescriptionDto MapToDto(Prescription p) =>
        new(p.Id, p.MedicalRecordId, p.MedicationName, p.Dosage,
            p.DurationDays, p.StartDate, p.EndDate, p.IsActive,
            p.Instructions, p.CreatedAt);
}
