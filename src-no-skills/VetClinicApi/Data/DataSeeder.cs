using Microsoft.EntityFrameworkCore;
using VetClinicApi.Models;

namespace VetClinicApi.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(VetClinicDbContext context)
    {
        if (await context.Owners.AnyAsync())
            return;

        // Owners
        var owners = new List<Owner>
        {
            new() { Id = 1, FirstName = "Sarah", LastName = "Johnson", Email = "sarah.johnson@email.com", Phone = "555-0101", Address = "123 Oak Street", City = "Springfield", State = "IL", ZipCode = "62701", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 2, FirstName = "Michael", LastName = "Chen", Email = "michael.chen@email.com", Phone = "555-0102", Address = "456 Maple Avenue", City = "Portland", State = "OR", ZipCode = "97201", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 3, FirstName = "Emily", LastName = "Rodriguez", Email = "emily.rodriguez@email.com", Phone = "555-0103", Address = "789 Pine Road", City = "Austin", State = "TX", ZipCode = "73301", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 4, FirstName = "James", LastName = "Williams", Email = "james.williams@email.com", Phone = "555-0104", Address = "321 Elm Drive", City = "Denver", State = "CO", ZipCode = "80201", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 5, FirstName = "Lisa", LastName = "Thompson", Email = "lisa.thompson@email.com", Phone = "555-0105", Address = "654 Birch Lane", City = "Seattle", State = "WA", ZipCode = "98101", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
        };
        context.Owners.AddRange(owners);
        await context.SaveChangesAsync();

        // Pets
        var pets = new List<Pet>
        {
            new() { Id = 1, Name = "Buddy", Species = "Dog", Breed = "Golden Retriever", DateOfBirth = new DateOnly(2020, 3, 15), Weight = 32.5m, Color = "Golden", MicrochipNumber = "MC-001-2020", IsActive = true, OwnerId = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 2, Name = "Whiskers", Species = "Cat", Breed = "Siamese", DateOfBirth = new DateOnly(2019, 7, 22), Weight = 4.5m, Color = "Cream", MicrochipNumber = "MC-002-2019", IsActive = true, OwnerId = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 3, Name = "Max", Species = "Dog", Breed = "German Shepherd", DateOfBirth = new DateOnly(2021, 1, 10), Weight = 38.0m, Color = "Black and Tan", MicrochipNumber = "MC-003-2021", IsActive = true, OwnerId = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 4, Name = "Luna", Species = "Cat", Breed = "Maine Coon", DateOfBirth = new DateOnly(2022, 5, 8), Weight = 6.2m, Color = "Tabby", MicrochipNumber = "MC-004-2022", IsActive = true, OwnerId = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 5, Name = "Charlie", Species = "Dog", Breed = "Beagle", DateOfBirth = new DateOnly(2021, 11, 3), Weight = 12.0m, Color = "Tricolor", MicrochipNumber = "MC-005-2021", IsActive = true, OwnerId = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 6, Name = "Polly", Species = "Bird", Breed = "African Grey Parrot", DateOfBirth = new DateOnly(2018, 9, 14), Weight = 0.4m, Color = "Grey", MicrochipNumber = null, IsActive = true, OwnerId = 4, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 7, Name = "Thumper", Species = "Rabbit", Breed = "Holland Lop", DateOfBirth = new DateOnly(2023, 2, 28), Weight = 1.8m, Color = "White", MicrochipNumber = "MC-007-2023", IsActive = true, OwnerId = 5, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { Id = 8, Name = "Bella", Species = "Dog", Breed = "Labrador Retriever", DateOfBirth = new DateOnly(2020, 6, 20), Weight = 28.0m, Color = "Chocolate", MicrochipNumber = "MC-008-2020", IsActive = true, OwnerId = 5, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
        };
        context.Pets.AddRange(pets);
        await context.SaveChangesAsync();

        // Veterinarians
        var vets = new List<Veterinarian>
        {
            new() { Id = 1, FirstName = "Dr. Amanda", LastName = "Foster", Email = "amanda.foster@happypaws.com", Phone = "555-0201", Specialization = "General Practice", LicenseNumber = "VET-2015-001", IsAvailable = true, HireDate = new DateOnly(2015, 6, 1) },
            new() { Id = 2, FirstName = "Dr. Robert", LastName = "Kim", Email = "robert.kim@happypaws.com", Phone = "555-0202", Specialization = "Surgery", LicenseNumber = "VET-2018-002", IsAvailable = true, HireDate = new DateOnly(2018, 3, 15) },
            new() { Id = 3, FirstName = "Dr. Maria", LastName = "Santos", Email = "maria.santos@happypaws.com", Phone = "555-0203", Specialization = "Dentistry", LicenseNumber = "VET-2020-003", IsAvailable = true, HireDate = new DateOnly(2020, 1, 10) },
        };
        context.Veterinarians.AddRange(vets);
        await context.SaveChangesAsync();

        var now = DateTime.UtcNow;
        // Appointments: mix of past completed, future scheduled, cancelled, etc.
        var appointments = new List<Appointment>
        {
            new() { Id = 1, PetId = 1, VeterinarianId = 1, AppointmentDate = now.AddDays(-30), DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Annual checkup", Notes = "Routine wellness exam", CreatedAt = now.AddDays(-35), UpdatedAt = now.AddDays(-30) },
            new() { Id = 2, PetId = 2, VeterinarianId = 1, AppointmentDate = now.AddDays(-20), DurationMinutes = 45, Status = AppointmentStatus.Completed, Reason = "Vaccination booster", Notes = "FVRCP booster due", CreatedAt = now.AddDays(-25), UpdatedAt = now.AddDays(-20) },
            new() { Id = 3, PetId = 3, VeterinarianId = 2, AppointmentDate = now.AddDays(-15), DurationMinutes = 60, Status = AppointmentStatus.Completed, Reason = "Limping on right front leg", Notes = "X-ray recommended", CreatedAt = now.AddDays(-20), UpdatedAt = now.AddDays(-15) },
            new() { Id = 4, PetId = 4, VeterinarianId = 3, AppointmentDate = now.AddDays(-10), DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Dental cleaning", Notes = "Mild tartar buildup", CreatedAt = now.AddDays(-15), UpdatedAt = now.AddDays(-10) },
            new() { Id = 5, PetId = 5, VeterinarianId = 1, AppointmentDate = now.AddDays(-5), DurationMinutes = 30, Status = AppointmentStatus.Cancelled, Reason = "Skin irritation check", CancellationReason = "Owner rescheduled", CreatedAt = now.AddDays(-10), UpdatedAt = now.AddDays(-5) },
            new() { Id = 6, PetId = 1, VeterinarianId = 2, AppointmentDate = now.AddDays(1).Date.AddHours(9), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Follow-up on medication", CreatedAt = now, UpdatedAt = now },
            new() { Id = 7, PetId = 3, VeterinarianId = 1, AppointmentDate = now.AddDays(2).Date.AddHours(10), DurationMinutes = 45, Status = AppointmentStatus.Scheduled, Reason = "Post-surgery checkup", CreatedAt = now, UpdatedAt = now },
            new() { Id = 8, PetId = 6, VeterinarianId = 1, AppointmentDate = now.AddDays(3).Date.AddHours(14), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Feather plucking concern", CreatedAt = now, UpdatedAt = now },
            new() { Id = 9, PetId = 7, VeterinarianId = 3, AppointmentDate = now.AddDays(5).Date.AddHours(11), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Routine wellness exam", CreatedAt = now, UpdatedAt = now },
            new() { Id = 10, PetId = 8, VeterinarianId = 2, AppointmentDate = now.AddDays(-2), DurationMinutes = 30, Status = AppointmentStatus.NoShow, Reason = "Ear infection followup", CreatedAt = now.AddDays(-7), UpdatedAt = now.AddDays(-2) },
        };
        context.Appointments.AddRange(appointments);
        await context.SaveChangesAsync();

        // Medical records for completed appointments
        var medicalRecords = new List<MedicalRecord>
        {
            new() { Id = 1, AppointmentId = 1, PetId = 1, VeterinarianId = 1, Diagnosis = "Healthy, no concerns", Treatment = "Annual vaccines administered (DHPP, Rabies)", Notes = "Weight stable, teeth in good condition", FollowUpDate = DateOnly.FromDateTime(now.AddDays(365)), CreatedAt = now.AddDays(-30) },
            new() { Id = 2, AppointmentId = 2, PetId = 2, VeterinarianId = 1, Diagnosis = "Due for FVRCP booster", Treatment = "FVRCP vaccine administered", Notes = "Cat was calm during procedure", CreatedAt = now.AddDays(-20) },
            new() { Id = 3, AppointmentId = 3, PetId = 3, VeterinarianId = 2, Diagnosis = "Minor sprain in right front leg", Treatment = "Anti-inflammatory medication prescribed, rest recommended for 2 weeks", Notes = "X-ray showed no fracture", FollowUpDate = DateOnly.FromDateTime(now.AddDays(14)), CreatedAt = now.AddDays(-15) },
            new() { Id = 4, AppointmentId = 4, PetId = 4, VeterinarianId = 3, Diagnosis = "Mild periodontal disease grade 1", Treatment = "Professional dental cleaning performed, polishing done", Notes = "Recommend dental treats and regular brushing", FollowUpDate = DateOnly.FromDateTime(now.AddDays(180)), CreatedAt = now.AddDays(-10) },
        };
        context.MedicalRecords.AddRange(medicalRecords);
        await context.SaveChangesAsync();

        // Prescriptions
        var prescriptions = new List<Prescription>
        {
            new() { Id = 1, MedicalRecordId = 1, MedicationName = "Heartgard Plus", Dosage = "1 chewable tablet monthly", DurationDays = 365, StartDate = DateOnly.FromDateTime(now.AddDays(-30)), EndDate = DateOnly.FromDateTime(now.AddDays(335)), Instructions = "Give with food on the first of each month", CreatedAt = now.AddDays(-30) },
            new() { Id = 2, MedicalRecordId = 3, MedicationName = "Carprofen", Dosage = "25mg twice daily", DurationDays = 14, StartDate = DateOnly.FromDateTime(now.AddDays(-15)), EndDate = DateOnly.FromDateTime(now.AddDays(-1)), Instructions = "Give with food, monitor for vomiting or diarrhea", CreatedAt = now.AddDays(-15) },
            new() { Id = 3, MedicalRecordId = 3, MedicationName = "Gabapentin", Dosage = "100mg once daily at bedtime", DurationDays = 10, StartDate = DateOnly.FromDateTime(now.AddDays(-15)), EndDate = DateOnly.FromDateTime(now.AddDays(-5)), Instructions = "May cause drowsiness", CreatedAt = now.AddDays(-15) },
            new() { Id = 4, MedicalRecordId = 4, MedicationName = "Clindamycin", Dosage = "75mg twice daily", DurationDays = 10, StartDate = DateOnly.FromDateTime(now.AddDays(-10)), EndDate = DateOnly.FromDateTime(now), Instructions = "Complete full course even if symptoms improve", CreatedAt = now.AddDays(-10) },
            new() { Id = 5, MedicalRecordId = 1, MedicationName = "NexGard", Dosage = "1 chewable tablet monthly", DurationDays = 365, StartDate = DateOnly.FromDateTime(now.AddDays(-30)), EndDate = DateOnly.FromDateTime(now.AddDays(335)), Instructions = "Flea and tick prevention, give with food", CreatedAt = now.AddDays(-30) },
        };
        context.Prescriptions.AddRange(prescriptions);
        await context.SaveChangesAsync();

        // Vaccinations
        var vaccinations = new List<Vaccination>
        {
            new() { Id = 1, PetId = 1, VaccineName = "Rabies", DateAdministered = DateOnly.FromDateTime(now.AddDays(-30)), ExpirationDate = DateOnly.FromDateTime(now.AddDays(335)), BatchNumber = "RAB-2024-1001", AdministeredByVetId = 1, Notes = "3-year rabies vaccine", CreatedAt = now.AddDays(-30) },
            new() { Id = 2, PetId = 1, VaccineName = "DHPP", DateAdministered = DateOnly.FromDateTime(now.AddDays(-30)), ExpirationDate = DateOnly.FromDateTime(now.AddDays(335)), BatchNumber = "DHPP-2024-2001", AdministeredByVetId = 1, CreatedAt = now.AddDays(-30) },
            new() { Id = 3, PetId = 2, VaccineName = "FVRCP", DateAdministered = DateOnly.FromDateTime(now.AddDays(-20)), ExpirationDate = DateOnly.FromDateTime(now.AddDays(345)), BatchNumber = "FVRCP-2024-3001", AdministeredByVetId = 1, Notes = "Booster shot", CreatedAt = now.AddDays(-20) },
            new() { Id = 4, PetId = 3, VaccineName = "Rabies", DateAdministered = DateOnly.FromDateTime(now.AddDays(-400)), ExpirationDate = DateOnly.FromDateTime(now.AddDays(-35)), BatchNumber = "RAB-2023-0501", AdministeredByVetId = 2, Notes = "1-year rabies vaccine - EXPIRED", CreatedAt = now.AddDays(-400) },
            new() { Id = 5, PetId = 5, VaccineName = "Bordetella", DateAdministered = DateOnly.FromDateTime(now.AddDays(-340)), ExpirationDate = DateOnly.FromDateTime(now.AddDays(25)), BatchNumber = "BOR-2024-4001", AdministeredByVetId = 1, Notes = "Kennel cough vaccine - expiring soon", CreatedAt = now.AddDays(-340) },
            new() { Id = 6, PetId = 8, VaccineName = "DHPP", DateAdministered = DateOnly.FromDateTime(now.AddDays(-200)), ExpirationDate = DateOnly.FromDateTime(now.AddDays(165)), BatchNumber = "DHPP-2024-5001", AdministeredByVetId = 2, CreatedAt = now.AddDays(-200) },
        };
        context.Vaccinations.AddRange(vaccinations);
        await context.SaveChangesAsync();
    }
}
