using Microsoft.EntityFrameworkCore;
using VetClinicApi.Data;
using VetClinicApi.DTOs;
using VetClinicApi.Models;

namespace VetClinicApi.Services;

public sealed class PrescriptionService(VetClinicDbContext context, ILogger<PrescriptionService> logger) : IPrescriptionService
{
    public async Task<PrescriptionResponse?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var prescription = await context.Prescriptions.FindAsync([id], cancellationToken);
        if (prescription is null)
        {
            return null;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return new PrescriptionResponse(
            prescription.Id, prescription.MedicalRecordId, prescription.MedicationName,
            prescription.Dosage, prescription.DurationDays, prescription.StartDate,
            prescription.EndDate, prescription.Instructions,
            prescription.EndDate >= today, prescription.CreatedAt);
    }

    public async Task<(PrescriptionResponse? Result, string? Error)> CreateAsync(CreatePrescriptionRequest request, CancellationToken cancellationToken)
    {
        var recordExists = await context.MedicalRecords.AnyAsync(m => m.Id == request.MedicalRecordId, cancellationToken);
        if (!recordExists)
        {
            return (null, "Medical record not found.");
        }

        var prescription = new Prescription
        {
            MedicalRecordId = request.MedicalRecordId,
            MedicationName = request.MedicationName,
            Dosage = request.Dosage,
            DurationDays = request.DurationDays,
            StartDate = request.StartDate,
            EndDate = request.StartDate.AddDays(request.DurationDays),
            Instructions = request.Instructions
        };

        context.Prescriptions.Add(prescription);
        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Prescription created: {PrescriptionId} for MedicalRecord {RecordId}", prescription.Id, prescription.MedicalRecordId);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return (new PrescriptionResponse(
            prescription.Id, prescription.MedicalRecordId, prescription.MedicationName,
            prescription.Dosage, prescription.DurationDays, prescription.StartDate,
            prescription.EndDate, prescription.Instructions,
            prescription.EndDate >= today, prescription.CreatedAt), null);
    }
}
