using VetClinicApi.Models;
using VetClinicApi.Models.Enums;

namespace VetClinicApi.Data;

public static class SeedData
{
    public static void Initialize(VetClinicDbContext context)
    {
        if (context.Owners.Any()) return;

        // Owners
        var owners = new List<Owner>
        {
            new() { Id = 1, FirstName = "Sarah", LastName = "Johnson", Email = "sarah.johnson@email.com", Phone = "555-0101", Address = "123 Maple St", City = "Springfield", State = "IL", ZipCode = "62704" },
            new() { Id = 2, FirstName = "Michael", LastName = "Chen", Email = "michael.chen@email.com", Phone = "555-0102", Address = "456 Oak Ave", City = "Portland", State = "OR", ZipCode = "97201" },
            new() { Id = 3, FirstName = "Emily", LastName = "Rodriguez", Email = "emily.rodriguez@email.com", Phone = "555-0103", Address = "789 Pine Rd", City = "Austin", State = "TX", ZipCode = "73301" },
            new() { Id = 4, FirstName = "James", LastName = "Wilson", Email = "james.wilson@email.com", Phone = "555-0104", Address = "321 Elm Blvd", City = "Denver", State = "CO", ZipCode = "80201" },
            new() { Id = 5, FirstName = "Lisa", LastName = "Patel", Email = "lisa.patel@email.com", Phone = "555-0105", Address = "654 Cedar Ln", City = "Seattle", State = "WA", ZipCode = "98101" }
        };
        context.Owners.AddRange(owners);
        context.SaveChanges();

        // Pets
        var pets = new List<Pet>
        {
            new() { Id = 1, Name = "Buddy", Species = "Dog", Breed = "Golden Retriever", DateOfBirth = new DateOnly(2019, 3, 15), Weight = 32.5m, Color = "Golden", MicrochipNumber = "MC-001-2019", OwnerId = 1 },
            new() { Id = 2, Name = "Whiskers", Species = "Cat", Breed = "Siamese", DateOfBirth = new DateOnly(2020, 7, 22), Weight = 4.8m, Color = "Cream/Brown", MicrochipNumber = "MC-002-2020", OwnerId = 1 },
            new() { Id = 3, Name = "Max", Species = "Dog", Breed = "German Shepherd", DateOfBirth = new DateOnly(2018, 11, 5), Weight = 38.2m, Color = "Black/Tan", MicrochipNumber = "MC-003-2018", OwnerId = 2 },
            new() { Id = 4, Name = "Luna", Species = "Cat", Breed = "Maine Coon", DateOfBirth = new DateOnly(2021, 1, 10), Weight = 6.3m, Color = "Tabby", OwnerId = 3 },
            new() { Id = 5, Name = "Charlie", Species = "Dog", Breed = "Beagle", DateOfBirth = new DateOnly(2022, 5, 18), Weight = 12.1m, Color = "Tricolor", MicrochipNumber = "MC-005-2022", OwnerId = 3 },
            new() { Id = 6, Name = "Tweety", Species = "Bird", Breed = "Cockatiel", DateOfBirth = new DateOnly(2023, 2, 14), Weight = 0.09m, Color = "Yellow/Grey", OwnerId = 4 },
            new() { Id = 7, Name = "Bella", Species = "Dog", Breed = "Labrador Retriever", DateOfBirth = new DateOnly(2020, 9, 1), Weight = 28.7m, Color = "Chocolate", MicrochipNumber = "MC-007-2020", OwnerId = 5 },
            new() { Id = 8, Name = "Thumper", Species = "Rabbit", Breed = "Holland Lop", DateOfBirth = new DateOnly(2023, 6, 20), Weight = 1.8m, Color = "White/Brown", OwnerId = 4 }
        };
        context.Pets.AddRange(pets);
        context.SaveChanges();

        // Veterinarians
        var vets = new List<Veterinarian>
        {
            new() { Id = 1, FirstName = "Dr. Amanda", LastName = "Foster", Email = "amanda.foster@happypaws.com", Phone = "555-0201", Specialization = "General Practice", LicenseNumber = "VET-2015-001", HireDate = new DateOnly(2015, 6, 1) },
            new() { Id = 2, FirstName = "Dr. Robert", LastName = "Kim", Email = "robert.kim@happypaws.com", Phone = "555-0202", Specialization = "Surgery", LicenseNumber = "VET-2018-002", HireDate = new DateOnly(2018, 3, 15) },
            new() { Id = 3, FirstName = "Dr. Maria", LastName = "Santos", Email = "maria.santos@happypaws.com", Phone = "555-0203", Specialization = "Exotic Animals", LicenseNumber = "VET-2020-003", HireDate = new DateOnly(2020, 1, 10) }
        };
        context.Veterinarians.AddRange(vets);
        context.SaveChanges();

        var now = DateTime.UtcNow;

        // Appointments — various statuses
        var appointments = new List<Appointment>
        {
            // Completed appointments (past)
            new() { Id = 1, PetId = 1, VeterinarianId = 1, AppointmentDate = now.AddDays(-30), DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Annual wellness exam for Buddy" },
            new() { Id = 2, PetId = 3, VeterinarianId = 2, AppointmentDate = now.AddDays(-25), DurationMinutes = 60, Status = AppointmentStatus.Completed, Reason = "Hip evaluation and X-rays for Max" },
            new() { Id = 3, PetId = 2, VeterinarianId = 1, AppointmentDate = now.AddDays(-20), DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Vaccination update for Whiskers" },
            new() { Id = 4, PetId = 5, VeterinarianId = 1, AppointmentDate = now.AddDays(-15), DurationMinutes = 45, Status = AppointmentStatus.Completed, Reason = "Skin allergy consultation for Charlie" },

            // Cancelled appointment
            new() { Id = 5, PetId = 4, VeterinarianId = 3, AppointmentDate = now.AddDays(-10), DurationMinutes = 30, Status = AppointmentStatus.Cancelled, Reason = "Dental cleaning for Luna", CancellationReason = "Owner had scheduling conflict" },

            // Today/current
            new() { Id = 6, PetId = 7, VeterinarianId = 1, AppointmentDate = now.Date.AddHours(9), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Routine checkup for Bella" },
            new() { Id = 7, PetId = 6, VeterinarianId = 3, AppointmentDate = now.Date.AddHours(10), DurationMinutes = 45, Status = AppointmentStatus.Scheduled, Reason = "Wing feather trimming for Tweety" },

            // Future appointments
            new() { Id = 8, PetId = 1, VeterinarianId = 2, AppointmentDate = now.AddDays(7).Date.AddHours(14), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Follow-up exam for Buddy" },
            new() { Id = 9, PetId = 8, VeterinarianId = 3, AppointmentDate = now.AddDays(10).Date.AddHours(11), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Nail trimming for Thumper" },
            new() { Id = 10, PetId = 4, VeterinarianId = 1, AppointmentDate = now.AddDays(14).Date.AddHours(15), DurationMinutes = 60, Status = AppointmentStatus.Scheduled, Reason = "Dental cleaning reschedule for Luna" }
        };
        context.Appointments.AddRange(appointments);
        context.SaveChanges();

        // Medical Records (for completed appointments)
        var medicalRecords = new List<MedicalRecord>
        {
            new() { Id = 1, AppointmentId = 1, PetId = 1, VeterinarianId = 1, Diagnosis = "Healthy — no issues found", Treatment = "Annual vaccines administered (DHPP, Rabies). Heartworm test negative.", Notes = "Weight stable. Teeth in good condition. Recommend dental cleaning within 6 months.", FollowUpDate = DateOnly.FromDateTime(now.AddMonths(6)) },
            new() { Id = 2, AppointmentId = 2, PetId = 3, VeterinarianId = 2, Diagnosis = "Mild hip dysplasia — left hip", Treatment = "Anti-inflammatory medication prescribed. Recommend joint supplement. Limited high-impact activity.", Notes = "X-rays show early signs of hip dysplasia. Monitoring recommended.", FollowUpDate = DateOnly.FromDateTime(now.AddMonths(3)) },
            new() { Id = 3, AppointmentId = 3, PetId = 2, VeterinarianId = 1, Diagnosis = "Healthy — routine vaccination", Treatment = "FVRCP booster administered. Flea/tick prevention applied.", Notes = "Slight tartar buildup on back teeth. Otherwise excellent health." },
            new() { Id = 4, AppointmentId = 4, PetId = 5, VeterinarianId = 1, Diagnosis = "Allergic dermatitis — suspected environmental allergens", Treatment = "Prescribed antihistamine and medicated shampoo. Elimination diet recommended for 8 weeks.", Notes = "Skin scraping negative for mites. Likely seasonal allergies.", FollowUpDate = DateOnly.FromDateTime(now.AddMonths(2)) }
        };
        context.MedicalRecords.AddRange(medicalRecords);
        context.SaveChanges();

        // Prescriptions (mix of active and expired)
        var today = DateOnly.FromDateTime(now);
        var prescriptions = new List<Prescription>
        {
            new() { Id = 1, MedicalRecordId = 2, MedicationName = "Carprofen", Dosage = "50mg twice daily", DurationDays = 14, StartDate = today.AddDays(-25), Instructions = "Give with food. Monitor for GI upset." },
            new() { Id = 2, MedicalRecordId = 2, MedicationName = "Glucosamine Supplement", Dosage = "500mg once daily", DurationDays = 90, StartDate = today.AddDays(-25), Instructions = "Mix with food. Long-term joint support." },
            new() { Id = 3, MedicalRecordId = 4, MedicationName = "Diphenhydramine", Dosage = "25mg twice daily", DurationDays = 30, StartDate = today.AddDays(-15), Instructions = "Administer orally. May cause drowsiness." },
            new() { Id = 4, MedicalRecordId = 4, MedicationName = "Chlorhexidine Shampoo", Dosage = "Apply topically every 3 days", DurationDays = 21, StartDate = today.AddDays(-15), Instructions = "Lather and leave on for 10 minutes before rinsing." },
            new() { Id = 5, MedicalRecordId = 1, MedicationName = "Heartgard Plus", Dosage = "1 chewable monthly", DurationDays = 180, StartDate = today.AddDays(-30), Instructions = "Give on the same day each month." }
        };
        context.Prescriptions.AddRange(prescriptions);
        context.SaveChanges();

        // Vaccinations (current, expiring soon, expired)
        var vaccinations = new List<Vaccination>
        {
            new() { Id = 1, PetId = 1, VaccineName = "Rabies", DateAdministered = today.AddDays(-30), ExpirationDate = today.AddYears(1).AddDays(-30), BatchNumber = "RAB-2024-A1", AdministeredByVetId = 1, Notes = "3-year rabies vaccine" },
            new() { Id = 2, PetId = 1, VaccineName = "DHPP", DateAdministered = today.AddDays(-30), ExpirationDate = today.AddYears(1).AddDays(-30), BatchNumber = "DHPP-2024-B2", AdministeredByVetId = 1 },
            new() { Id = 3, PetId = 2, VaccineName = "FVRCP", DateAdministered = today.AddDays(-20), ExpirationDate = today.AddYears(1).AddDays(-20), BatchNumber = "FVR-2024-C3", AdministeredByVetId = 1 },
            // Expiring soon (within 30 days)
            new() { Id = 4, PetId = 3, VaccineName = "Bordetella", DateAdministered = today.AddMonths(-11), ExpirationDate = today.AddDays(20), BatchNumber = "BOR-2023-D4", AdministeredByVetId = 2, Notes = "Kennel cough vaccine — due for renewal" },
            // Expired
            new() { Id = 5, PetId = 7, VaccineName = "Rabies", DateAdministered = today.AddYears(-2), ExpirationDate = today.AddDays(-30), BatchNumber = "RAB-2022-E5", AdministeredByVetId = 1, Notes = "Expired — needs renewal" },
            new() { Id = 6, PetId = 5, VaccineName = "Leptospirosis", DateAdministered = today.AddMonths(-6), ExpirationDate = today.AddMonths(6), BatchNumber = "LEP-2024-F6", AdministeredByVetId = 1 }
        };
        context.Vaccinations.AddRange(vaccinations);
        context.SaveChanges();
    }
}
