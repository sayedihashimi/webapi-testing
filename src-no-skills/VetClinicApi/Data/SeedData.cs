using VetClinicApi.Models;

namespace VetClinicApi.Data;

public static class SeedData
{
    public static void Initialize(VetClinicDbContext context)
    {
        if (context.Owners.Any()) return;

        // Owners
        var owners = new List<Owner>
        {
            new() { FirstName = "Sarah", LastName = "Johnson", Email = "sarah.johnson@email.com", Phone = "555-0101", Address = "123 Maple St", City = "Springfield", State = "IL", ZipCode = "62701" },
            new() { FirstName = "Michael", LastName = "Chen", Email = "michael.chen@email.com", Phone = "555-0102", Address = "456 Oak Ave", City = "Portland", State = "OR", ZipCode = "97201" },
            new() { FirstName = "Emily", LastName = "Rodriguez", Email = "emily.rodriguez@email.com", Phone = "555-0103", Address = "789 Pine Rd", City = "Austin", State = "TX", ZipCode = "73301" },
            new() { FirstName = "David", LastName = "Williams", Email = "david.williams@email.com", Phone = "555-0104", Address = "321 Elm Blvd", City = "Denver", State = "CO", ZipCode = "80201" },
            new() { FirstName = "Jessica", LastName = "Martinez", Email = "jessica.martinez@email.com", Phone = "555-0105", Address = "654 Birch Ln", City = "Seattle", State = "WA", ZipCode = "98101" },
        };
        context.Owners.AddRange(owners);
        context.SaveChanges();

        // Pets
        var pets = new List<Pet>
        {
            new() { Name = "Buddy", Species = "Dog", Breed = "Golden Retriever", DateOfBirth = new DateOnly(2020, 3, 15), Weight = 32.5m, Color = "Golden", MicrochipNumber = "MC-001-2020", OwnerId = owners[0].Id },
            new() { Name = "Whiskers", Species = "Cat", Breed = "Siamese", DateOfBirth = new DateOnly(2019, 7, 22), Weight = 4.2m, Color = "Cream", MicrochipNumber = "MC-002-2019", OwnerId = owners[0].Id },
            new() { Name = "Max", Species = "Dog", Breed = "German Shepherd", DateOfBirth = new DateOnly(2021, 1, 10), Weight = 38.0m, Color = "Black and Tan", MicrochipNumber = "MC-003-2021", OwnerId = owners[1].Id },
            new() { Name = "Luna", Species = "Cat", Breed = "Persian", DateOfBirth = new DateOnly(2022, 5, 3), Weight = 3.8m, Color = "White", OwnerId = owners[2].Id },
            new() { Name = "Charlie", Species = "Dog", Breed = "Beagle", DateOfBirth = new DateOnly(2020, 11, 8), Weight = 11.3m, Color = "Tricolor", MicrochipNumber = "MC-005-2020", OwnerId = owners[2].Id },
            new() { Name = "Tweety", Species = "Bird", Breed = "Canary", DateOfBirth = new DateOnly(2023, 2, 14), Weight = 0.025m, Color = "Yellow", OwnerId = owners[3].Id },
            new() { Name = "Thumper", Species = "Rabbit", Breed = "Holland Lop", DateOfBirth = new DateOnly(2023, 6, 1), Weight = 1.8m, Color = "Brown", OwnerId = owners[3].Id },
            new() { Name = "Rocky", Species = "Dog", Breed = "Bulldog", DateOfBirth = new DateOnly(2018, 9, 20), Weight = 25.0m, Color = "Brindle", MicrochipNumber = "MC-008-2018", OwnerId = owners[4].Id },
        };
        context.Pets.AddRange(pets);
        context.SaveChanges();

        // Veterinarians
        var vets = new List<Veterinarian>
        {
            new() { FirstName = "Dr. Amanda", LastName = "Foster", Email = "amanda.foster@happypaws.com", Phone = "555-0201", Specialization = "General Practice", LicenseNumber = "VET-2015-001", HireDate = new DateOnly(2015, 6, 1) },
            new() { FirstName = "Dr. James", LastName = "Park", Email = "james.park@happypaws.com", Phone = "555-0202", Specialization = "Surgery", LicenseNumber = "VET-2018-002", HireDate = new DateOnly(2018, 3, 15) },
            new() { FirstName = "Dr. Lisa", LastName = "Thompson", Email = "lisa.thompson@happypaws.com", Phone = "555-0203", Specialization = "Dentistry", LicenseNumber = "VET-2020-003", HireDate = new DateOnly(2020, 1, 10) },
        };
        context.Veterinarians.AddRange(vets);
        context.SaveChanges();

        var now = DateTime.UtcNow;

        // Appointments (mix of statuses)
        var appointments = new List<Appointment>
        {
            // Completed appointments (past)
            new() { PetId = pets[0].Id, VeterinarianId = vets[0].Id, AppointmentDate = now.AddDays(-30), DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Annual checkup", Notes = "All vitals normal" },
            new() { PetId = pets[1].Id, VeterinarianId = vets[0].Id, AppointmentDate = now.AddDays(-25), DurationMinutes = 45, Status = AppointmentStatus.Completed, Reason = "Dental cleaning", Notes = "Minor tartar buildup removed" },
            new() { PetId = pets[2].Id, VeterinarianId = vets[1].Id, AppointmentDate = now.AddDays(-20), DurationMinutes = 60, Status = AppointmentStatus.Completed, Reason = "Limping on front left leg", Notes = "X-ray showed no fracture, prescribed rest" },
            new() { PetId = pets[4].Id, VeterinarianId = vets[2].Id, AppointmentDate = now.AddDays(-15), DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Vaccination update", Notes = "Administered rabies and distemper vaccines" },
            // Cancelled
            new() { PetId = pets[3].Id, VeterinarianId = vets[0].Id, AppointmentDate = now.AddDays(-10), DurationMinutes = 30, Status = AppointmentStatus.Cancelled, Reason = "Routine checkup", CancellationReason = "Owner had scheduling conflict" },
            // Scheduled future appointments
            new() { PetId = pets[0].Id, VeterinarianId = vets[0].Id, AppointmentDate = now.AddDays(3).Date.AddHours(9), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Follow-up checkup" },
            new() { PetId = pets[5].Id, VeterinarianId = vets[0].Id, AppointmentDate = now.AddDays(5).Date.AddHours(10), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Wing examination" },
            new() { PetId = pets[6].Id, VeterinarianId = vets[2].Id, AppointmentDate = now.AddDays(7).Date.AddHours(14), DurationMinutes = 45, Status = AppointmentStatus.Scheduled, Reason = "Dental checkup" },
            // NoShow
            new() { PetId = pets[7].Id, VeterinarianId = vets[1].Id, AppointmentDate = now.AddDays(-5), DurationMinutes = 30, Status = AppointmentStatus.NoShow, Reason = "Skin irritation check" },
            // Today's scheduled
            new() { PetId = pets[2].Id, VeterinarianId = vets[1].Id, AppointmentDate = now.Date.AddHours(15), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Post-treatment follow-up" },
        };
        context.Appointments.AddRange(appointments);
        context.SaveChanges();

        // Medical Records (for completed appointments)
        var records = new List<MedicalRecord>
        {
            new() { AppointmentId = appointments[0].Id, PetId = pets[0].Id, VeterinarianId = vets[0].Id, Diagnosis = "Healthy - no issues found", Treatment = "No treatment needed. Continue current diet and exercise.", FollowUpDate = DateOnly.FromDateTime(now.AddMonths(6)) },
            new() { AppointmentId = appointments[1].Id, PetId = pets[1].Id, VeterinarianId = vets[0].Id, Diagnosis = "Mild periodontal disease stage 1", Treatment = "Professional dental cleaning performed. Recommend daily teeth brushing.", Notes = "Owner instructed on feline dental care" },
            new() { AppointmentId = appointments[2].Id, PetId = pets[2].Id, VeterinarianId = vets[1].Id, Diagnosis = "Soft tissue strain - left forelimb", Treatment = "Prescribed rest for 2 weeks. Anti-inflammatory medication. Cold compress twice daily.", FollowUpDate = DateOnly.FromDateTime(now.AddDays(10)) },
            new() { AppointmentId = appointments[3].Id, PetId = pets[4].Id, VeterinarianId = vets[2].Id, Diagnosis = "Healthy - vaccination administered", Treatment = "Rabies and distemper vaccines administered. No adverse reactions observed.", Notes = "Next vaccination due in 1 year" },
        };
        context.MedicalRecords.AddRange(records);
        context.SaveChanges();

        // Prescriptions (mix active/expired)
        var today = DateOnly.FromDateTime(now);
        var prescriptions = new List<Prescription>
        {
            new() { MedicalRecordId = records[2].Id, MedicationName = "Carprofen", Dosage = "25mg twice daily", DurationDays = 14, StartDate = today.AddDays(-20), Instructions = "Give with food. Monitor for GI upset." },
            new() { MedicalRecordId = records[2].Id, MedicationName = "Tramadol", Dosage = "50mg as needed", DurationDays = 7, StartDate = today.AddDays(-20), Instructions = "Only if pain persists. Max 2 doses per day." },
            new() { MedicalRecordId = records[1].Id, MedicationName = "Chlorhexidine Oral Rinse", Dosage = "Apply once daily", DurationDays = 30, StartDate = today.AddDays(-25), Instructions = "Apply to gums with finger brush." },
            new() { MedicalRecordId = records[0].Id, MedicationName = "Heartworm Prevention", Dosage = "1 tablet monthly", DurationDays = 90, StartDate = today, Instructions = "Give on the 1st of each month with food." },
            new() { MedicalRecordId = records[3].Id, MedicationName = "Benadryl", Dosage = "12.5mg if needed", DurationDays = 3, StartDate = today.AddDays(-15), Instructions = "Only if vaccination site shows swelling." },
        };
        context.Prescriptions.AddRange(prescriptions);
        context.SaveChanges();

        // Vaccinations (current, expiring soon, expired)
        var vaccinations = new List<Vaccination>
        {
            new() { PetId = pets[0].Id, VaccineName = "Rabies", DateAdministered = today.AddYears(-1), ExpirationDate = today.AddMonths(2), BatchNumber = "RAB-2024-001", AdministeredByVetId = vets[0].Id, Notes = "3-year vaccine" },
            new() { PetId = pets[0].Id, VaccineName = "DHPP", DateAdministered = today.AddMonths(-10), ExpirationDate = today.AddMonths(2), BatchNumber = "DHPP-2024-042", AdministeredByVetId = vets[0].Id },
            new() { PetId = pets[1].Id, VaccineName = "FVRCP", DateAdministered = today.AddMonths(-11), ExpirationDate = today.AddDays(20), BatchNumber = "FVR-2024-015", AdministeredByVetId = vets[0].Id, Notes = "Due for booster soon" },
            new() { PetId = pets[2].Id, VaccineName = "Rabies", DateAdministered = today.AddYears(-2), ExpirationDate = today.AddDays(-30), BatchNumber = "RAB-2023-088", AdministeredByVetId = vets[1].Id, Notes = "OVERDUE for renewal" },
            new() { PetId = pets[4].Id, VaccineName = "Rabies", DateAdministered = today.AddDays(-15), ExpirationDate = today.AddYears(3), BatchNumber = "RAB-2025-011", AdministeredByVetId = vets[2].Id },
            new() { PetId = pets[4].Id, VaccineName = "Bordetella", DateAdministered = today.AddMonths(-6), ExpirationDate = today.AddMonths(6), BatchNumber = "BOR-2024-033", AdministeredByVetId = vets[0].Id },
        };
        context.Vaccinations.AddRange(vaccinations);
        context.SaveChanges();
    }
}
