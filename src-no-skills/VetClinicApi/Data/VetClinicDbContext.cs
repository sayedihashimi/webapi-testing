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
        modelBuilder.Entity<Owner>(e =>
        {
            e.HasIndex(o => o.Email).IsUnique();
            e.HasMany(o => o.Pets).WithOne(p => p.Owner).HasForeignKey(p => p.OwnerId).OnDelete(DeleteBehavior.Restrict);
        });

        // Pet
        modelBuilder.Entity<Pet>(e =>
        {
            e.HasIndex(p => p.MicrochipNumber).IsUnique().HasFilter("[MicrochipNumber] IS NOT NULL");
            e.HasMany(p => p.Appointments).WithOne(a => a.Pet).HasForeignKey(a => a.PetId).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(p => p.MedicalRecords).WithOne(m => m.Pet).HasForeignKey(m => m.PetId).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(p => p.Vaccinations).WithOne(v => v.Pet).HasForeignKey(v => v.PetId).OnDelete(DeleteBehavior.Restrict);
            e.Property(p => p.Weight).HasColumnType("decimal(10,2)");
        });

        // Veterinarian
        modelBuilder.Entity<Veterinarian>(e =>
        {
            e.HasIndex(v => v.Email).IsUnique();
            e.HasIndex(v => v.LicenseNumber).IsUnique();
            e.HasMany(v => v.Appointments).WithOne(a => a.Veterinarian).HasForeignKey(a => a.VeterinarianId).OnDelete(DeleteBehavior.Restrict);
        });

        // Appointment
        modelBuilder.Entity<Appointment>(e =>
        {
            e.Property(a => a.Status).HasConversion<string>();
            e.HasOne(a => a.MedicalRecord).WithOne(m => m.Appointment).HasForeignKey<MedicalRecord>(m => m.AppointmentId);
        });

        // MedicalRecord
        modelBuilder.Entity<MedicalRecord>(e =>
        {
            e.HasIndex(m => m.AppointmentId).IsUnique();
            e.HasMany(m => m.Prescriptions).WithOne(p => p.MedicalRecord).HasForeignKey(p => p.MedicalRecordId).OnDelete(DeleteBehavior.Cascade);
        });

        // Prescription
        modelBuilder.Entity<Prescription>(e =>
        {
            e.Ignore(p => p.IsActive); // computed in code
        });

        // Vaccination
        modelBuilder.Entity<Vaccination>(e =>
        {
            e.HasOne(v => v.AdministeredByVet).WithMany().HasForeignKey(v => v.AdministeredByVetId).OnDelete(DeleteBehavior.Restrict);
            e.Ignore(v => v.IsExpired);
            e.Ignore(v => v.IsDueSoon);
        });
    }
}
