using Microsoft.EntityFrameworkCore;
using VetClinicApi.Models;

namespace VetClinicApi.Data;

public class VetClinicDbContext : DbContext
{
    public VetClinicDbContext(DbContextOptions<VetClinicDbContext> options) : base(options) { }

    public DbSet<Owner> Owners => Set<Owner>();
    public DbSet<Pet> Pets => Set<Pet>();
    public DbSet<Veterinarian> Veterinarians => Set<Veterinarian>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<Vaccination> Vaccinations => Set<Vaccination>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Owner
        modelBuilder.Entity<Owner>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now')");
        });

        // Pet
        modelBuilder.Entity<Pet>(entity =>
        {
            entity.HasIndex(e => e.MicrochipNumber).IsUnique().HasFilter("[MicrochipNumber] IS NOT NULL");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.HasOne(e => e.Owner)
                  .WithMany(o => o.Pets)
                  .HasForeignKey(e => e.OwnerId)
                  .OnDelete(DeleteBehavior.Restrict);
            // Ignore computed properties
            entity.Ignore(e => e.Appointments);
        });

        // Remove the Ignore above - we want the Appointments nav property
        modelBuilder.Entity<Pet>(entity =>
        {
            // re-configure: Pet has many Appointments
        });

        // Veterinarian
        modelBuilder.Entity<Veterinarian>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.LicenseNumber).IsUnique();
        });

        // Appointment
        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasOne(e => e.Pet)
                  .WithMany(p => p.Appointments)
                  .HasForeignKey(e => e.PetId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Veterinarian)
                  .WithMany(v => v.Appointments)
                  .HasForeignKey(e => e.VeterinarianId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.Property(e => e.Status)
                  .HasConversion<string>();
        });

        // MedicalRecord
        modelBuilder.Entity<MedicalRecord>(entity =>
        {
            entity.HasIndex(e => e.AppointmentId).IsUnique();
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

        // Prescription - ignore computed properties
        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.Ignore(e => e.EndDate);
            entity.Ignore(e => e.IsActive);

            entity.HasOne(e => e.MedicalRecord)
                  .WithMany(m => m.Prescriptions)
                  .HasForeignKey(e => e.MedicalRecordId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Vaccination - ignore computed properties
        modelBuilder.Entity<Vaccination>(entity =>
        {
            entity.Ignore(e => e.IsExpired);
            entity.Ignore(e => e.IsDueSoon);

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
}
