using Microsoft.EntityFrameworkCore;
using VetClinicApi.Models;

namespace VetClinicApi.Data;

public sealed class VetClinicDbContext(DbContextOptions<VetClinicDbContext> options)
    : DbContext(options)
{
    public DbSet<Owner> Owners => Set<Owner>();
    public DbSet<Pet> Pets => Set<Pet>();
    public DbSet<Veterinarian> Veterinarians => Set<Veterinarian>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<Vaccination> Vaccinations => Set<Vaccination>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureOwner(modelBuilder);
        ConfigurePet(modelBuilder);
        ConfigureVeterinarian(modelBuilder);
        ConfigureAppointment(modelBuilder);
        ConfigureMedicalRecord(modelBuilder);
        ConfigurePrescription(modelBuilder);
        ConfigureVaccination(modelBuilder);
        SeedData(modelBuilder);
    }

    private static void ConfigureOwner(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Owner>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Phone).IsRequired();
            entity.Property(e => e.State).HasMaxLength(2);
        });
    }

    private static void ConfigurePet(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Pet>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Species).IsRequired();
            entity.Property(e => e.Breed).HasMaxLength(100);
            entity.Property(e => e.Weight).HasColumnType("decimal(10,2)");
            entity.HasIndex(e => e.MicrochipNumber).IsUnique().HasFilter("[MicrochipNumber] IS NOT NULL");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(e => e.Owner)
                .WithMany(o => o.Pets)
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureVeterinarian(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Veterinarian>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Phone).IsRequired();
            entity.Property(e => e.LicenseNumber).IsRequired();
            entity.HasIndex(e => e.LicenseNumber).IsUnique();
            entity.Property(e => e.IsAvailable).HasDefaultValue(true);
        });
    }

    private static void ConfigureAppointment(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.Reason).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Notes).HasMaxLength(2000);
            entity.Property(e => e.CancellationReason).HasMaxLength(500);
            entity.Property(e => e.DurationMinutes).HasDefaultValue(30);

            entity.HasOne(e => e.Pet)
                .WithMany(p => p.Appointments)
                .HasForeignKey(e => e.PetId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Veterinarian)
                .WithMany(v => v.Appointments)
                .HasForeignKey(e => e.VeterinarianId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureMedicalRecord(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MedicalRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.AppointmentId).IsUnique();
            entity.Property(e => e.Diagnosis).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.Treatment).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Notes).HasMaxLength(2000);

            entity.HasOne(e => e.Appointment)
                .WithOne(a => a.MedicalRecord)
                .HasForeignKey<MedicalRecord>(e => e.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Pet)
                .WithMany(p => p.MedicalRecords)
                .HasForeignKey(e => e.PetId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Veterinarian)
                .WithMany()
                .HasForeignKey(e => e.VeterinarianId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigurePrescription(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.MedicationName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Dosage).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Instructions).HasMaxLength(500);

            entity.HasOne(e => e.MedicalRecord)
                .WithMany(m => m.Prescriptions)
                .HasForeignKey(e => e.MedicalRecordId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureVaccination(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Vaccination>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.VaccineName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Notes).HasMaxLength(500);

            entity.HasOne(e => e.Pet)
                .WithMany(p => p.Vaccinations)
                .HasForeignKey(e => e.PetId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.AdministeredByVet)
                .WithMany()
                .HasForeignKey(e => e.AdministeredByVetId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        var now = new DateTime(2025, 1, 15, 9, 0, 0, DateTimeKind.Utc);

        // 5 Owners
        modelBuilder.Entity<Owner>().HasData(
            new Owner { Id = 1, FirstName = "Sarah", LastName = "Johnson", Email = "sarah.johnson@email.com", Phone = "555-0101", Address = "123 Oak Street", City = "Springfield", State = "IL", ZipCode = "62701", CreatedAt = now, UpdatedAt = now },
            new Owner { Id = 2, FirstName = "Michael", LastName = "Chen", Email = "michael.chen@email.com", Phone = "555-0102", Address = "456 Maple Ave", City = "Portland", State = "OR", ZipCode = "97201", CreatedAt = now, UpdatedAt = now },
            new Owner { Id = 3, FirstName = "Emily", LastName = "Rodriguez", Email = "emily.rodriguez@email.com", Phone = "555-0103", Address = "789 Pine Road", City = "Austin", State = "TX", ZipCode = "73301", CreatedAt = now, UpdatedAt = now },
            new Owner { Id = 4, FirstName = "David", LastName = "Kim", Email = "david.kim@email.com", Phone = "555-0104", Address = "321 Elm Blvd", City = "Seattle", State = "WA", ZipCode = "98101", CreatedAt = now, UpdatedAt = now },
            new Owner { Id = 5, FirstName = "Jessica", LastName = "Taylor", Email = "jessica.taylor@email.com", Phone = "555-0105", Address = "654 Cedar Lane", City = "Denver", State = "CO", ZipCode = "80201", CreatedAt = now, UpdatedAt = now }
        );

        // 8 Pets
        modelBuilder.Entity<Pet>().HasData(
            new Pet { Id = 1, Name = "Buddy", Species = "Dog", Breed = "Golden Retriever", DateOfBirth = new DateOnly(2020, 3, 15), Weight = 32.5m, Color = "Golden", MicrochipNumber = "MC-001-2020", IsActive = true, OwnerId = 1, CreatedAt = now, UpdatedAt = now },
            new Pet { Id = 2, Name = "Whiskers", Species = "Cat", Breed = "Siamese", DateOfBirth = new DateOnly(2019, 7, 22), Weight = 4.8m, Color = "Cream", MicrochipNumber = "MC-002-2019", IsActive = true, OwnerId = 1, CreatedAt = now, UpdatedAt = now },
            new Pet { Id = 3, Name = "Max", Species = "Dog", Breed = "German Shepherd", DateOfBirth = new DateOnly(2021, 1, 10), Weight = 38.0m, Color = "Black and Tan", MicrochipNumber = "MC-003-2021", IsActive = true, OwnerId = 2, CreatedAt = now, UpdatedAt = now },
            new Pet { Id = 4, Name = "Luna", Species = "Cat", Breed = "Maine Coon", DateOfBirth = new DateOnly(2022, 5, 8), Weight = 6.2m, Color = "Tabby", MicrochipNumber = "MC-004-2022", IsActive = true, OwnerId = 3, CreatedAt = now, UpdatedAt = now },
            new Pet { Id = 5, Name = "Charlie", Species = "Dog", Breed = "Beagle", DateOfBirth = new DateOnly(2020, 11, 30), Weight = 12.5m, Color = "Tricolor", MicrochipNumber = "MC-005-2020", IsActive = true, OwnerId = 3, CreatedAt = now, UpdatedAt = now },
            new Pet { Id = 6, Name = "Tweety", Species = "Bird", Breed = "Cockatiel", DateOfBirth = new DateOnly(2023, 2, 14), Weight = 0.09m, Color = "Yellow", MicrochipNumber = null, IsActive = true, OwnerId = 4, CreatedAt = now, UpdatedAt = now },
            new Pet { Id = 7, Name = "Thumper", Species = "Rabbit", Breed = "Holland Lop", DateOfBirth = new DateOnly(2023, 6, 20), Weight = 1.8m, Color = "White", MicrochipNumber = "MC-007-2023", IsActive = true, OwnerId = 5, CreatedAt = now, UpdatedAt = now },
            new Pet { Id = 8, Name = "Shadow", Species = "Cat", Breed = "Bombay", DateOfBirth = new DateOnly(2018, 9, 3), Weight = 5.1m, Color = "Black", MicrochipNumber = "MC-008-2018", IsActive = false, OwnerId = 4, CreatedAt = now, UpdatedAt = now }
        );

        // 3 Veterinarians
        modelBuilder.Entity<Veterinarian>().HasData(
            new Veterinarian { Id = 1, FirstName = "Dr. Amanda", LastName = "Foster", Email = "amanda.foster@happypaws.com", Phone = "555-0201", Specialization = "Small Animals", LicenseNumber = "VET-2015-001", IsAvailable = true, HireDate = new DateOnly(2015, 6, 1) },
            new Veterinarian { Id = 2, FirstName = "Dr. Robert", LastName = "Martinez", Email = "robert.martinez@happypaws.com", Phone = "555-0202", Specialization = "Surgery", LicenseNumber = "VET-2018-002", IsAvailable = true, HireDate = new DateOnly(2018, 3, 15) },
            new Veterinarian { Id = 3, FirstName = "Dr. Lisa", LastName = "Park", Email = "lisa.park@happypaws.com", Phone = "555-0203", Specialization = "Exotic Animals", LicenseNumber = "VET-2020-003", IsAvailable = false, HireDate = new DateOnly(2020, 9, 1) }
        );

        // 10 Appointments in various statuses
        modelBuilder.Entity<Appointment>().HasData(
            new Appointment { Id = 1, PetId = 1, VeterinarianId = 1, AppointmentDate = new DateTime(2025, 1, 10, 10, 0, 0, DateTimeKind.Utc), DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Annual checkup", Notes = "All good", CreatedAt = now, UpdatedAt = now },
            new Appointment { Id = 2, PetId = 2, VeterinarianId = 1, AppointmentDate = new DateTime(2025, 1, 10, 11, 0, 0, DateTimeKind.Utc), DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Vaccination update", CreatedAt = now, UpdatedAt = now },
            new Appointment { Id = 3, PetId = 3, VeterinarianId = 2, AppointmentDate = new DateTime(2025, 1, 12, 9, 0, 0, DateTimeKind.Utc), DurationMinutes = 60, Status = AppointmentStatus.Completed, Reason = "Limping on right front leg", CreatedAt = now, UpdatedAt = now },
            new Appointment { Id = 4, PetId = 4, VeterinarianId = 1, AppointmentDate = new DateTime(2025, 1, 13, 14, 0, 0, DateTimeKind.Utc), DurationMinutes = 30, Status = AppointmentStatus.Completed, Reason = "Skin irritation", CreatedAt = now, UpdatedAt = now },
            new Appointment { Id = 5, PetId = 5, VeterinarianId = 2, AppointmentDate = new DateTime(2025, 1, 14, 10, 0, 0, DateTimeKind.Utc), DurationMinutes = 30, Status = AppointmentStatus.Cancelled, Reason = "Dental cleaning", CancellationReason = "Owner rescheduled", CreatedAt = now, UpdatedAt = now },
            new Appointment { Id = 6, PetId = 1, VeterinarianId = 1, AppointmentDate = new DateTime(2025, 2, 1, 9, 0, 0, DateTimeKind.Utc), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Follow-up visit", CreatedAt = now, UpdatedAt = now },
            new Appointment { Id = 7, PetId = 6, VeterinarianId = 3, AppointmentDate = new DateTime(2025, 2, 1, 11, 0, 0, DateTimeKind.Utc), DurationMinutes = 45, Status = AppointmentStatus.Scheduled, Reason = "Wing clipping and checkup", CreatedAt = now, UpdatedAt = now },
            new Appointment { Id = 8, PetId = 7, VeterinarianId = 3, AppointmentDate = new DateTime(2025, 2, 2, 10, 0, 0, DateTimeKind.Utc), DurationMinutes = 30, Status = AppointmentStatus.Scheduled, Reason = "Nail trimming", CreatedAt = now, UpdatedAt = now },
            new Appointment { Id = 9, PetId = 3, VeterinarianId = 2, AppointmentDate = new DateTime(2025, 1, 15, 9, 0, 0, DateTimeKind.Utc), DurationMinutes = 30, Status = AppointmentStatus.CheckedIn, Reason = "Post-surgery follow-up", CreatedAt = now, UpdatedAt = now },
            new Appointment { Id = 10, PetId = 5, VeterinarianId = 1, AppointmentDate = new DateTime(2025, 1, 8, 15, 0, 0, DateTimeKind.Utc), DurationMinutes = 30, Status = AppointmentStatus.NoShow, Reason = "Routine vaccination", CreatedAt = now, UpdatedAt = now }
        );

        // 4 Medical records for completed appointments
        modelBuilder.Entity<MedicalRecord>().HasData(
            new MedicalRecord { Id = 1, AppointmentId = 1, PetId = 1, VeterinarianId = 1, Diagnosis = "Healthy - no issues found", Treatment = "No treatment needed. Continue current diet and exercise.", Notes = "Weight is ideal. Teeth in good condition.", FollowUpDate = new DateOnly(2026, 1, 10), CreatedAt = now },
            new MedicalRecord { Id = 2, AppointmentId = 2, PetId = 2, VeterinarianId = 1, Diagnosis = "Due for FVRCP booster", Treatment = "Administered FVRCP vaccine booster.", Notes = "Cat was calm during procedure.", CreatedAt = now },
            new MedicalRecord { Id = 3, AppointmentId = 3, PetId = 3, VeterinarianId = 2, Diagnosis = "Mild sprain in right front leg", Treatment = "Rest for 2 weeks. Anti-inflammatory medication prescribed.", Notes = "X-ray showed no fracture.", FollowUpDate = new DateOnly(2025, 1, 26), CreatedAt = now },
            new MedicalRecord { Id = 4, AppointmentId = 4, PetId = 4, VeterinarianId = 1, Diagnosis = "Allergic dermatitis", Treatment = "Topical corticosteroid cream. Dietary adjustment recommended.", Notes = "Likely food allergy. Trial elimination diet.", FollowUpDate = new DateOnly(2025, 2, 13), CreatedAt = now }
        );

        // 5 Prescriptions (mix active/expired)
        modelBuilder.Entity<Prescription>().HasData(
            new Prescription { Id = 1, MedicalRecordId = 3, MedicationName = "Carprofen", Dosage = "25mg twice daily", DurationDays = 14, StartDate = new DateOnly(2025, 1, 12), EndDate = new DateOnly(2025, 1, 26), Instructions = "Give with food. Monitor for stomach upset.", CreatedAt = now },
            new Prescription { Id = 2, MedicalRecordId = 4, MedicationName = "Hydrocortisone Cream", Dosage = "Apply thin layer twice daily", DurationDays = 21, StartDate = new DateOnly(2025, 1, 13), EndDate = new DateOnly(2025, 2, 3), Instructions = "Apply to affected areas. Prevent licking with cone if needed.", CreatedAt = now },
            new Prescription { Id = 3, MedicalRecordId = 3, MedicationName = "Tramadol", Dosage = "50mg as needed", DurationDays = 7, StartDate = new DateOnly(2025, 1, 12), EndDate = new DateOnly(2025, 1, 19), Instructions = "Only if showing signs of significant pain.", CreatedAt = now },
            new Prescription { Id = 4, MedicalRecordId = 4, MedicationName = "Omega-3 Fish Oil", Dosage = "1 capsule daily", DurationDays = 90, StartDate = new DateOnly(2025, 1, 13), EndDate = new DateOnly(2025, 4, 13), Instructions = "Mix with food. Supports skin health.", CreatedAt = now },
            new Prescription { Id = 5, MedicalRecordId = 1, MedicationName = "Heartworm Prevention", Dosage = "1 tablet monthly", DurationDays = 365, StartDate = new DateOnly(2025, 1, 10), EndDate = new DateOnly(2026, 1, 10), Instructions = "Give on the same day each month with a meal.", CreatedAt = now }
        );

        // 6 Vaccinations (current, expiring soon, expired)
        modelBuilder.Entity<Vaccination>().HasData(
            new Vaccination { Id = 1, PetId = 1, VaccineName = "Rabies", DateAdministered = new DateOnly(2024, 3, 15), ExpirationDate = new DateOnly(2027, 3, 15), BatchNumber = "RAB-2024-1001", AdministeredByVetId = 1, Notes = "3-year rabies vaccine", CreatedAt = now },
            new Vaccination { Id = 2, PetId = 1, VaccineName = "DHPP", DateAdministered = new DateOnly(2024, 3, 15), ExpirationDate = new DateOnly(2025, 3, 15), BatchNumber = "DHPP-2024-2001", AdministeredByVetId = 1, Notes = "Annual booster", CreatedAt = now },
            new Vaccination { Id = 3, PetId = 2, VaccineName = "FVRCP", DateAdministered = new DateOnly(2025, 1, 10), ExpirationDate = new DateOnly(2026, 1, 10), BatchNumber = "FVRCP-2025-3001", AdministeredByVetId = 1, CreatedAt = now },
            new Vaccination { Id = 4, PetId = 3, VaccineName = "Rabies", DateAdministered = new DateOnly(2023, 6, 1), ExpirationDate = new DateOnly(2024, 6, 1), BatchNumber = "RAB-2023-4001", AdministeredByVetId = 2, Notes = "1-year rabies vaccine - EXPIRED", CreatedAt = now },
            new Vaccination { Id = 5, PetId = 5, VaccineName = "DHPP", DateAdministered = new DateOnly(2024, 6, 15), ExpirationDate = new DateOnly(2025, 2, 15), BatchNumber = "DHPP-2024-5001", AdministeredByVetId = 2, Notes = "Expiring soon", CreatedAt = now },
            new Vaccination { Id = 6, PetId = 7, VaccineName = "RHDV2", DateAdministered = new DateOnly(2024, 8, 20), ExpirationDate = new DateOnly(2025, 8, 20), BatchNumber = "RHDV-2024-6001", AdministeredByVetId = 3, Notes = "Rabbit hemorrhagic disease vaccine", CreatedAt = now }
        );
    }
}
