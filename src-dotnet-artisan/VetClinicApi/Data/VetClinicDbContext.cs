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
        // Owner
        modelBuilder.Entity<Owner>(e =>
        {
            e.HasIndex(o => o.Email).IsUnique();
            e.HasMany(o => o.Pets).WithOne(p => p.Owner).HasForeignKey(p => p.OwnerId);
        });

        // Pet
        modelBuilder.Entity<Pet>(e =>
        {
            e.HasIndex(p => p.MicrochipNumber).IsUnique().HasFilter("MicrochipNumber IS NOT NULL");
            e.Property(p => p.Weight).HasColumnType("decimal(10,2)");
            e.HasMany(p => p.Appointments).WithOne(a => a.Pet).HasForeignKey(a => a.PetId);
            e.HasMany(p => p.MedicalRecords).WithOne(m => m.Pet).HasForeignKey(m => m.PetId).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(p => p.Vaccinations).WithOne(v => v.Pet).HasForeignKey(v => v.PetId);
        });

        // Veterinarian
        modelBuilder.Entity<Veterinarian>(e =>
        {
            e.HasIndex(v => v.Email).IsUnique();
            e.HasIndex(v => v.LicenseNumber).IsUnique();
        });

        // Appointment
        modelBuilder.Entity<Appointment>(e =>
        {
            e.Property(a => a.Status).HasConversion<string>();
            e.HasOne(a => a.MedicalRecord).WithOne(m => m.Appointment).HasForeignKey<MedicalRecord>(m => m.AppointmentId);
            e.HasOne(a => a.Veterinarian).WithMany(v => v.Appointments).HasForeignKey(a => a.VeterinarianId).OnDelete(DeleteBehavior.Restrict);
        });

        // MedicalRecord
        modelBuilder.Entity<MedicalRecord>(e =>
        {
            e.HasIndex(m => m.AppointmentId).IsUnique();
            e.HasOne(m => m.Veterinarian).WithMany().HasForeignKey(m => m.VeterinarianId).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(m => m.Prescriptions).WithOne(p => p.MedicalRecord).HasForeignKey(p => p.MedicalRecordId);
        });

        // Prescription
        modelBuilder.Entity<Prescription>(e =>
        {
            e.Ignore(p => p.EndDate);
            e.Ignore(p => p.IsActive);
        });

        // Vaccination
        modelBuilder.Entity<Vaccination>(e =>
        {
            e.Ignore(v => v.IsExpired);
            e.Ignore(v => v.IsDueSoon);
            e.HasOne(v => v.AdministeredByVet).WithMany().HasForeignKey(v => v.AdministeredByVetId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
