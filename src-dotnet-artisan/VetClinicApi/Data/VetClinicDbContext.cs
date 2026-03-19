using Microsoft.EntityFrameworkCore;
using VetClinicApi.Models;

namespace VetClinicApi.Data;

public sealed class VetClinicDbContext(DbContextOptions<VetClinicDbContext> options) : DbContext(options)
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
        modelBuilder.Entity<Owner>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
        });

        modelBuilder.Entity<Pet>(entity =>
        {
            entity.HasIndex(e => e.MicrochipNumber).IsUnique().HasFilter("MicrochipNumber IS NOT NULL");
            entity.HasOne(e => e.Owner)
                .WithMany(o => o.Pets)
                .HasForeignKey(e => e.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Veterinarian>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.LicenseNumber).IsUnique();
        });

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

        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.HasOne(e => e.MedicalRecord)
                .WithMany(m => m.Prescriptions)
                .HasForeignKey(e => e.MedicalRecordId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Ignore(e => e.EndDate);
            entity.Ignore(e => e.IsActive);
        });

        modelBuilder.Entity<Vaccination>(entity =>
        {
            entity.HasOne(e => e.Pet)
                .WithMany(p => p.Vaccinations)
                .HasForeignKey(e => e.PetId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.AdministeredByVet)
                .WithMany()
                .HasForeignKey(e => e.AdministeredByVetId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Ignore(e => e.IsExpired);
            entity.Ignore(e => e.IsDueSoon);
        });
    }
}
