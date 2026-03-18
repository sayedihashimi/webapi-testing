using VetClinicApi.Models;

namespace VetClinicApi.Data;

public static class SeedData
{
    public static async Task InitializeAsync(VetClinicDbContext db)
    {
        if (await db.Owners.AnyAsync())
            return;

        var now = DateTime.UtcNow;

        var owners = new List<Owner>
        {
            new() { FirstName = "Sarah", LastName = "Johnson", Email = "sarah.johnson@email.com", Phone = "555-0101", Address = "123 Oak Street", City = "Springfield", State = "IL", ZipCode = "62701", CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Michael", LastName = "Chen", Email = "michael.chen@email.com", Phone = "555-0102", Address = "456 Maple Ave", City = "Portland", State = "OR", ZipCode = "97201", CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Emily", LastName = "Rodriguez", Email = "emily.rodriguez@email.com", Phone = "555-0103", Address = "789 Pine Road", City = "Austin", State = "TX", ZipCode = "73301", CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "David", LastName = "Kim", Email = "david.kim@email.com", Phone = "555-0104", Address = "321 Elm Blvd", City = "Seattle", State = "WA", ZipCode = "98101", CreatedAt = now, UpdatedAt = now },
            new() { FirstName = "Jessica", LastName = "Patel", Email = "jessica.patel@email.com", Phone = "555-0105", Address = "654 Cedar Lane", City = "Denver", State = "CO", ZipCode = "80201", CreatedAt = now, UpdatedAt = now }
        };
        db.Owners.AddRange(owners);
        await db.SaveChangesAsync();

        var pets = new List<Pet>
        {
            new() { Name = "Buddy", Species = "Dog", Breed = "Golden Retriever", DateOfBirth = new DateOnly(2020, 3, 15), Weight = 32.5m, Color = "Golden", MicrochipNumber = "MC-001-2020", OwnerId = owners[0].Id, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Whiskers", Species = "Cat", Breed = "Siamese", DateOfBirth = new DateOnly(2019, 7, 22), Weight = 4.2m, Color = "Cream", MicrochipNumber = "MC-002-2019", OwnerId = owners[0].Id, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Max", Species = "Dog", Breed = "German Shepherd", DateOfBirth = new DateOnly(2021, 1, 10), Weight = 38.0m, Color = "Black and Tan", MicrochipNumber = "MC-003-2021", OwnerId = owners[1].Id, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Luna", Species = "Cat", Breed = "Maine Coon", DateOfBirth = new DateOnly(2022, 5, 1), Weight = 5.8m, Color = "Tabby", OwnerId = owners[2].Id, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Charlie", Species = "Dog", Breed = "Labrador", DateOfBirth = new DateOnly(2020, 11, 30), Weight = 29.0m, Color = "Chocolate", MicrochipNumber = "MC-005-2020", OwnerId = owners[2].Id, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Tweety", Species = "Bird", Breed = "Cockatiel", DateOfBirth = new DateOnly(2023, 2, 14), Weight = 0.09m, Color = "Yellow", OwnerId = owners[3].Id, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Bella", Species = "Dog", Breed = "Poodle", DateOfBirth = new DateOnly(2021, 8, 20), Weight = 12.5m, Color = "White", MicrochipNumber = "MC-007-2021", OwnerId = owners[3].Id, CreatedAt = now, UpdatedAt = now },
            new() { Name = "Thumper", Species = "Rabbit", Breed = "Holland Lop", DateOfBirth = new DateOnly(2023, 6, 5), Weight = 1.8m, Color = "Brown", OwnerId = owners[4].Id, CreatedAt = now, UpdatedAt = now }
        };
        db.Pets.AddRange(pets);
        await db.SaveChangesAsync();

        var vets = new List<Veterinarian>
        {
            new() { FirstName = "Dr. Amanda", LastName = "Wilson", Email = "amanda.wilson@happypaws.com", Phone = "555-0201", Specialization = "General Practice", LicenseNumber = "VET-2015-001", HireDate = new DateOnly(2015, 6, 1) },
            new() { FirstName = "Dr. Robert", LastName = "Garcia", Email = "robert.garcia@happypaws.com", Phone = "555-0202", Specialization = "Surgery", LicenseNumber = "VET-2018-002", HireDate = new DateOnly(2018, 3, 15) },
            new() { FirstName = "Dr. Lisa", LastName = "Thompson", Email = "lisa.thompson@happypaws.com", Phone = "555-0203", Specialization = "Exotic Animals", LicenseNumber = "VET-2020-003", HireDate = new DateOnly(2020, 1, 10) }
        };
        db.Veterinarians.AddRange(vets);
        await db.SaveChangesAsync();

        var pastDate1 = now.AddDays(-30);
        var pastDate2 = now.AddDays(-20);
        var pastDate3 = now.AddDays(-10);
        var pastDate4 = now.AddDays(-5);
        var futureDate1 = now.AddDays(3);
        var futureDate2 = now.AddDays(7);
        var futureDate3 = now.AddDays(14);

        var appointments = new List<Appointment>
        {
            new() { PetId = pets[0].Id, VeterinarianId = vets[0].Id, AppointmentDate = pastDate1, DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Annual checkup", Notes = "Routine wellness exam", CreatedAt = pastDate1.AddDays(-7), UpdatedAt = pastDate1 },
            new() { PetId = pets[1].Id, VeterinarianId = vets[0].Id, AppointmentDate = pastDate2, DurationMinutes = 45, Status = AppointmentStatus.Completed, Reason = "Vaccination update", CreatedAt = pastDate2.AddDays(-5), UpdatedAt = pastDate2 },
            new() { PetId = pets[2].Id, VeterinarianId = vets[1].Id, AppointmentDate = pastDate3, DurationMinutes = 60, Status = AppointmentStatus.Completed, Reason = "Limping on front left leg", Notes = "X-ray required", CreatedAt = pastDate3.AddDays(-3), UpdatedAt = pastDate3 },
            new() { PetId = pets[3].Id, VeterinarianId = vets[0].Id, AppointmentDate = pastDate4, DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Dental cleaning", CreatedAt = pastDate4.AddDays(-10), UpdatedAt = pastDate4 },
            new() { PetId = pets[4].Id, VeterinarianId = vets[1].Id, AppointmentDate = pastDate4.AddHours(2), DurationMinutes = 30, Status = AppointmentStatus.Cancelled, Reason = "Skin irritation check", CancellationReason = "Owner had scheduling conflict", CreatedAt = pastDate4.AddDays(-3), UpdatedAt = pastDate4 },
            new() { PetId = pets[0].Id, VeterinarianId = vets[0].Id, AppointmentDate = futureDate1, DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Follow-up on blood work", CreatedAt = now, UpdatedAt = now },
            new() { PetId = pets[5].Id, VeterinarianId = vets[2].Id, AppointmentDate = futureDate1.AddHours(2), DurationMinutes = 45, Status = AppointmentStatus.Scheduled, Reason = "Wing feather trimming", CreatedAt = now, UpdatedAt = now },
            new() { PetId = pets[6].Id, VeterinarianId = vets[0].Id, AppointmentDate = futureDate2, DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Annual vaccination", CreatedAt = now, UpdatedAt = now },
            new() { PetId = pets[7].Id, VeterinarianId = vets[2].Id, AppointmentDate = futureDate2.AddHours(1), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Nail trimming and health check", CreatedAt = now, UpdatedAt = now },
            new() { PetId = pets[2].Id, VeterinarianId = vets[1].Id, AppointmentDate = futureDate3, DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Post-surgery follow-up", CreatedAt = now, UpdatedAt = now }
        };
        db.Appointments.AddRange(appointments);
        await db.SaveChangesAsync();

        var medicalRecords = new List<MedicalRecord>
        {
            new() { AppointmentId = appointments[0].Id, PetId = pets[0].Id, VeterinarianId = vets[0].Id, Diagnosis = "Healthy - no concerns", Treatment = "Routine wellness exam completed. All vitals normal.", Notes = "Weight stable. Recommend continued current diet.", FollowUpDate = DateOnly.FromDateTime(now.AddMonths(6)), CreatedAt = pastDate1 },
            new() { AppointmentId = appointments[1].Id, PetId = pets[1].Id, VeterinarianId = vets[0].Id, Diagnosis = "Up to date on vaccinations", Treatment = "Administered FVRCP booster vaccine", Notes = "No adverse reactions observed during 15-min wait", CreatedAt = pastDate2 },
            new() { AppointmentId = appointments[2].Id, PetId = pets[2].Id, VeterinarianId = vets[1].Id, Diagnosis = "Minor sprain in left front leg", Treatment = "Anti-inflammatory medication prescribed. Rest for 2 weeks.", Notes = "X-ray showed no fracture. Soft tissue injury.", FollowUpDate = DateOnly.FromDateTime(now.AddDays(14)), CreatedAt = pastDate3 },
            new() { AppointmentId = appointments[3].Id, PetId = pets[3].Id, VeterinarianId = vets[0].Id, Diagnosis = "Mild gingivitis", Treatment = "Professional dental cleaning performed under anesthesia", Notes = "Recommend daily dental treats and annual cleanings", CreatedAt = pastDate4 }
        };
        db.MedicalRecords.AddRange(medicalRecords);
        await db.SaveChangesAsync();

        var prescriptions = new List<Prescription>
        {
            new() { MedicalRecordId = medicalRecords[2].Id, MedicationName = "Carprofen", Dosage = "50mg twice daily", DurationDays = 14, StartDate = DateOnly.FromDateTime(pastDate3), EndDate = DateOnly.FromDateTime(pastDate3).AddDays(14), Instructions = "Give with food. Monitor for GI upset.", CreatedAt = pastDate3 },
            new() { MedicalRecordId = medicalRecords[2].Id, MedicationName = "Gabapentin", Dosage = "100mg once daily", DurationDays = 7, StartDate = DateOnly.FromDateTime(pastDate3), EndDate = DateOnly.FromDateTime(pastDate3).AddDays(7), Instructions = "For pain management. May cause drowsiness.", CreatedAt = pastDate3 },
            new() { MedicalRecordId = medicalRecords[3].Id, MedicationName = "Clindamycin", Dosage = "75mg twice daily", DurationDays = 10, StartDate = DateOnly.FromDateTime(pastDate4), EndDate = DateOnly.FromDateTime(pastDate4).AddDays(10), Instructions = "Antibiotic for gum infection prevention. Complete full course.", CreatedAt = pastDate4 },
            new() { MedicalRecordId = medicalRecords[0].Id, MedicationName = "Heartgard Plus", Dosage = "1 chewable monthly", DurationDays = 365, StartDate = DateOnly.FromDateTime(pastDate1), EndDate = DateOnly.FromDateTime(pastDate1).AddDays(365), Instructions = "Monthly heartworm prevention.", CreatedAt = pastDate1 },
            new() { MedicalRecordId = medicalRecords[0].Id, MedicationName = "NexGard", Dosage = "1 chewable monthly", DurationDays = 90, StartDate = DateOnly.FromDateTime(now.AddDays(-60)), EndDate = DateOnly.FromDateTime(now.AddDays(-60)).AddDays(90), Instructions = "Flea and tick prevention. Give with food.", CreatedAt = now.AddDays(-60) }
        };
        db.Prescriptions.AddRange(prescriptions);
        await db.SaveChangesAsync();

        var vaccinations = new List<Vaccination>
        {
            new() { PetId = pets[0].Id, VaccineName = "Rabies", DateAdministered = DateOnly.FromDateTime(now.AddMonths(-6)), ExpirationDate = DateOnly.FromDateTime(now.AddMonths(6)), BatchNumber = "RAB-2024-A1", AdministeredByVetId = vets[0].Id, CreatedAt = now.AddMonths(-6) },
            new() { PetId = pets[0].Id, VaccineName = "DHPP", DateAdministered = DateOnly.FromDateTime(now.AddMonths(-6)), ExpirationDate = DateOnly.FromDateTime(now.AddMonths(6)), BatchNumber = "DHPP-2024-B2", AdministeredByVetId = vets[0].Id, CreatedAt = now.AddMonths(-6) },
            new() { PetId = pets[1].Id, VaccineName = "FVRCP", DateAdministered = DateOnly.FromDateTime(now.AddDays(-20)), ExpirationDate = DateOnly.FromDateTime(now.AddMonths(12)), BatchNumber = "FVR-2024-C3", AdministeredByVetId = vets[0].Id, CreatedAt = now.AddDays(-20) },
            new() { PetId = pets[2].Id, VaccineName = "Rabies", DateAdministered = DateOnly.FromDateTime(now.AddMonths(-11)), ExpirationDate = DateOnly.FromDateTime(now.AddDays(25)), BatchNumber = "RAB-2024-D4", AdministeredByVetId = vets[1].Id, Notes = "Due for renewal soon", CreatedAt = now.AddMonths(-11) },
            new() { PetId = pets[4].Id, VaccineName = "Bordetella", DateAdministered = DateOnly.FromDateTime(now.AddMonths(-13)), ExpirationDate = DateOnly.FromDateTime(now.AddDays(-30)), BatchNumber = "BOR-2023-E5", AdministeredByVetId = vets[0].Id, Notes = "Expired - needs booster", CreatedAt = now.AddMonths(-13) },
            new() { PetId = pets[6].Id, VaccineName = "Rabies", DateAdministered = DateOnly.FromDateTime(now.AddMonths(-3)), ExpirationDate = DateOnly.FromDateTime(now.AddMonths(9)), BatchNumber = "RAB-2024-F6", AdministeredByVetId = vets[0].Id, CreatedAt = now.AddMonths(-3) }
        };
        db.Vaccinations.AddRange(vaccinations);
        await db.SaveChangesAsync();
    }

    private static async Task<bool> AnyAsync(this Microsoft.EntityFrameworkCore.DbSet<Owner> set)
    {
        return await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.AnyAsync(set);
    }
}
