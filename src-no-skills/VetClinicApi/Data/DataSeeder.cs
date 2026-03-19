using Microsoft.EntityFrameworkCore;
using VetClinicApi.Models;

namespace VetClinicApi.Data;

public static class DataSeeder
{
    public static void Seed(VetClinicDbContext context)
    {
        if (context.Owners.Any()) return;

        // Owners
        var owners = new List<Owner>
        {
            new() { Id = 1, FirstName = "Sarah", LastName = "Johnson", Email = "sarah.johnson@email.com", Phone = "555-0101", Address = "123 Oak Street", City = "Springfield", State = "IL", ZipCode = "62701" },
            new() { Id = 2, FirstName = "Michael", LastName = "Chen", Email = "michael.chen@email.com", Phone = "555-0102", Address = "456 Maple Avenue", City = "Springfield", State = "IL", ZipCode = "62702" },
            new() { Id = 3, FirstName = "Emily", LastName = "Rodriguez", Email = "emily.rodriguez@email.com", Phone = "555-0103", Address = "789 Pine Road", City = "Springfield", State = "IL", ZipCode = "62703" },
            new() { Id = 4, FirstName = "David", LastName = "Thompson", Email = "david.thompson@email.com", Phone = "555-0104", Address = "321 Elm Court", City = "Springfield", State = "IL", ZipCode = "62704" },
            new() { Id = 5, FirstName = "Jessica", LastName = "Williams", Email = "jessica.williams@email.com", Phone = "555-0105", Address = "654 Birch Lane", City = "Springfield", State = "IL", ZipCode = "62705" },
        };
        context.Owners.AddRange(owners);
        context.SaveChanges();

        // Pets
        var pets = new List<Pet>
        {
            new() { Id = 1, Name = "Buddy", Species = "Dog", Breed = "Golden Retriever", DateOfBirth = new DateOnly(2020, 3, 15), Weight = 32.5m, Color = "Golden", MicrochipNumber = "MC-10001", OwnerId = 1 },
            new() { Id = 2, Name = "Whiskers", Species = "Cat", Breed = "Siamese", DateOfBirth = new DateOnly(2019, 7, 22), Weight = 4.8m, Color = "Cream", MicrochipNumber = "MC-10002", OwnerId = 1 },
            new() { Id = 3, Name = "Max", Species = "Dog", Breed = "German Shepherd", DateOfBirth = new DateOnly(2021, 1, 10), Weight = 38.0m, Color = "Black and Tan", MicrochipNumber = "MC-10003", OwnerId = 2 },
            new() { Id = 4, Name = "Luna", Species = "Cat", Breed = "Maine Coon", DateOfBirth = new DateOnly(2022, 5, 8), Weight = 6.2m, Color = "Tabby", MicrochipNumber = "MC-10004", OwnerId = 2 },
            new() { Id = 5, Name = "Charlie", Species = "Dog", Breed = "Beagle", DateOfBirth = new DateOnly(2021, 11, 3), Weight = 12.0m, Color = "Tricolor", MicrochipNumber = "MC-10005", OwnerId = 3 },
            new() { Id = 6, Name = "Tweety", Species = "Bird", Breed = "Cockatiel", DateOfBirth = new DateOnly(2023, 2, 14), Weight = 0.1m, Color = "Yellow", OwnerId = 4 },
            new() { Id = 7, Name = "Bella", Species = "Dog", Breed = "Labrador Retriever", DateOfBirth = new DateOnly(2020, 8, 20), Weight = 28.0m, Color = "Chocolate", MicrochipNumber = "MC-10006", OwnerId = 4 },
            new() { Id = 8, Name = "Oliver", Species = "Rabbit", Breed = "Holland Lop", DateOfBirth = new DateOnly(2023, 6, 1), Weight = 1.8m, Color = "White", OwnerId = 5 },
        };
        context.Pets.AddRange(pets);
        context.SaveChanges();

        // Veterinarians
        var vets = new List<Veterinarian>
        {
            new() { Id = 1, FirstName = "Dr. Amanda", LastName = "Foster", Email = "amanda.foster@happypaws.com", Phone = "555-0201", Specialization = "General Practice", LicenseNumber = "VET-2015-001", HireDate = new DateOnly(2015, 6, 1) },
            new() { Id = 2, FirstName = "Dr. Robert", LastName = "Kim", Email = "robert.kim@happypaws.com", Phone = "555-0202", Specialization = "Surgery", LicenseNumber = "VET-2018-002", HireDate = new DateOnly(2018, 3, 15) },
            new() { Id = 3, FirstName = "Dr. Lisa", LastName = "Patel", Email = "lisa.patel@happypaws.com", Phone = "555-0203", Specialization = "Dentistry", LicenseNumber = "VET-2020-003", HireDate = new DateOnly(2020, 1, 10) },
        };
        context.Veterinarians.AddRange(vets);
        context.SaveChanges();

        var now = DateTime.UtcNow;
        var today = DateOnly.FromDateTime(now);

        // Appointments (mix of statuses)
        var appointments = new List<Appointment>
        {
            new() { Id = 1, PetId = 1, VeterinarianId = 1, AppointmentDate = now.AddDays(-30), DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Annual checkup", Notes = "All vitals normal" },
            new() { Id = 2, PetId = 2, VeterinarianId = 1, AppointmentDate = now.AddDays(-20), DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Vaccination booster", Notes = "Administered FVRCP" },
            new() { Id = 3, PetId = 3, VeterinarianId = 2, AppointmentDate = now.AddDays(-15), DurationMinutes = 60, Status = AppointmentStatus.Completed, Reason = "Limping on front left leg", Notes = "X-ray taken" },
            new() { Id = 4, PetId = 5, VeterinarianId = 3, AppointmentDate = now.AddDays(-10), DurationMinutes = 45, Status = AppointmentStatus.Completed, Reason = "Dental cleaning", Notes = "Two teeth extracted" },
            new() { Id = 5, PetId = 4, VeterinarianId = 1, AppointmentDate = now.AddDays(-5), DurationMinutes = 30, Status = AppointmentStatus.Cancelled, Reason = "Skin irritation", CancellationReason = "Owner rescheduled" },
            new() { Id = 6, PetId = 1, VeterinarianId = 1, AppointmentDate = now.AddDays(1).Date.AddHours(9), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Follow-up checkup" },
            new() { Id = 7, PetId = 7, VeterinarianId = 2, AppointmentDate = now.AddDays(2).Date.AddHours(10), DurationMinutes = 45, Status = AppointmentStatus.Scheduled, Reason = "Hip evaluation" },
            new() { Id = 8, PetId = 6, VeterinarianId = 1, AppointmentDate = now.AddDays(3).Date.AddHours(14), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Wing clipping and checkup" },
            new() { Id = 9, PetId = 8, VeterinarianId = 3, AppointmentDate = now.AddDays(5).Date.AddHours(11), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Dental check" },
            new() { Id = 10, PetId = 3, VeterinarianId = 2, AppointmentDate = now.AddDays(-2), DurationMinutes = 30, Status = AppointmentStatus.NoShow, Reason = "Follow-up for leg" },
        };
        context.Appointments.AddRange(appointments);
        context.SaveChanges();

        // Medical Records (for completed appointments)
        var records = new List<MedicalRecord>
        {
            new() { Id = 1, AppointmentId = 1, PetId = 1, VeterinarianId = 1, Diagnosis = "Healthy - no issues found", Treatment = "No treatment needed. Recommended continued regular diet and exercise.", Notes = "Weight stable. Good dental health.", FollowUpDate = today.AddMonths(6) },
            new() { Id = 2, AppointmentId = 2, PetId = 2, VeterinarianId = 1, Diagnosis = "Due for vaccination booster", Treatment = "Administered FVRCP vaccine booster", Notes = "Mild reaction expected. Monitor for 24 hours." },
            new() { Id = 3, AppointmentId = 3, PetId = 3, VeterinarianId = 2, Diagnosis = "Mild sprain in left front leg", Treatment = "Prescribed anti-inflammatory medication and rest for 2 weeks", Notes = "X-ray shows no fracture. Soft tissue injury.", FollowUpDate = today.AddDays(14) },
            new() { Id = 4, AppointmentId = 4, PetId = 5, VeterinarianId = 3, Diagnosis = "Periodontal disease - stage 2", Treatment = "Professional dental cleaning performed. Extracted two damaged premolars.", Notes = "Recovery expected in 5-7 days. Soft food recommended.", FollowUpDate = today.AddDays(7) },
        };
        context.MedicalRecords.AddRange(records);
        context.SaveChanges();

        // Prescriptions
        var prescriptions = new List<Prescription>
        {
            new() { Id = 1, MedicalRecordId = 3, MedicationName = "Carprofen", Dosage = "25mg twice daily", DurationDays = 14, StartDate = today.AddDays(-15), EndDate = today.AddDays(-1), Instructions = "Give with food" },
            new() { Id = 2, MedicalRecordId = 3, MedicationName = "Glucosamine Supplement", Dosage = "500mg once daily", DurationDays = 60, StartDate = today.AddDays(-15), EndDate = today.AddDays(45), Instructions = "Can be mixed with food" },
            new() { Id = 3, MedicalRecordId = 4, MedicationName = "Amoxicillin", Dosage = "250mg twice daily", DurationDays = 10, StartDate = today.AddDays(-10), EndDate = today, Instructions = "Complete full course even if symptoms improve" },
            new() { Id = 4, MedicalRecordId = 4, MedicationName = "Meloxicam", Dosage = "1.5mg once daily", DurationDays = 5, StartDate = today.AddDays(-10), EndDate = today.AddDays(-5), Instructions = "Give with food. Do not exceed recommended dose." },
            new() { Id = 5, MedicalRecordId = 1, MedicationName = "Heartworm Prevention", Dosage = "1 tablet monthly", DurationDays = 365, StartDate = today.AddDays(-30), EndDate = today.AddDays(335), Instructions = "Give on the same day each month" },
        };
        context.Prescriptions.AddRange(prescriptions);
        context.SaveChanges();

        // Vaccinations
        var vaccinations = new List<Vaccination>
        {
            new() { Id = 1, PetId = 1, VaccineName = "Rabies", DateAdministered = today.AddMonths(-11), ExpirationDate = today.AddMonths(1), BatchNumber = "RAB-2024-001", AdministeredByVetId = 1, Notes = "3-year rabies vaccine" },
            new() { Id = 2, PetId = 1, VaccineName = "DHPP", DateAdministered = today.AddMonths(-6), ExpirationDate = today.AddMonths(6), BatchNumber = "DHPP-2024-015", AdministeredByVetId = 1 },
            new() { Id = 3, PetId = 2, VaccineName = "FVRCP", DateAdministered = today.AddDays(-20), ExpirationDate = today.AddMonths(12), BatchNumber = "FVR-2024-042", AdministeredByVetId = 1, Notes = "Booster administered" },
            new() { Id = 4, PetId = 3, VaccineName = "Rabies", DateAdministered = today.AddMonths(-13), ExpirationDate = today.AddDays(-30), BatchNumber = "RAB-2023-088", AdministeredByVetId = 2, Notes = "Expired - needs renewal" },
            new() { Id = 5, PetId = 5, VaccineName = "Bordetella", DateAdministered = today.AddMonths(-5), ExpirationDate = today.AddDays(25), BatchNumber = "BOR-2024-007", AdministeredByVetId = 3 },
            new() { Id = 6, PetId = 7, VaccineName = "DHPP", DateAdministered = today.AddMonths(-10), ExpirationDate = today.AddMonths(2), BatchNumber = "DHPP-2024-033", AdministeredByVetId = 2 },
        };
        context.Vaccinations.AddRange(vaccinations);
        context.SaveChanges();
    }
}
