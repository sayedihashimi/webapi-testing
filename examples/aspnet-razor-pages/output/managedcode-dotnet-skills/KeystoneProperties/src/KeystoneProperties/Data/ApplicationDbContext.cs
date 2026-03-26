using KeystoneProperties.Models;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Property> Properties => Set<Property>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Lease> Leases => Set<Lease>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<MaintenanceRequest> MaintenanceRequests => Set<MaintenanceRequest>();
    public DbSet<Inspection> Inspections => Set<Inspection>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Property
        modelBuilder.Entity<Property>(entity =>
        {
            entity.ToTable("Properties");
        });

        // Unit
        modelBuilder.Entity<Unit>(entity =>
        {
            entity.HasIndex(u => new { u.PropertyId, u.UnitNumber }).IsUnique();

            entity.Property(u => u.MonthlyRent).HasPrecision(18, 2);
            entity.Property(u => u.DepositAmount).HasPrecision(18, 2);
            entity.Property(u => u.Bathrooms).HasPrecision(3, 1);

            entity.HasOne(u => u.Property)
                .WithMany(p => p.Units)
                .HasForeignKey(u => u.PropertyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Tenant
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasIndex(t => t.Email).IsUnique();
        });

        // Lease
        modelBuilder.Entity<Lease>(entity =>
        {
            entity.Property(l => l.MonthlyRentAmount).HasPrecision(18, 2);
            entity.Property(l => l.DepositAmount).HasPrecision(18, 2);

            entity.HasOne(l => l.Unit)
                .WithMany(u => u.Leases)
                .HasForeignKey(l => l.UnitId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(l => l.Tenant)
                .WithMany(t => t.Leases)
                .HasForeignKey(l => l.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(l => l.RenewalOfLease)
                .WithMany()
                .HasForeignKey(l => l.RenewalOfLeaseId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Payment
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.Property(p => p.Amount).HasPrecision(18, 2);

            entity.HasOne(p => p.Lease)
                .WithMany(l => l.Payments)
                .HasForeignKey(p => p.LeaseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // MaintenanceRequest
        modelBuilder.Entity<MaintenanceRequest>(entity =>
        {
            entity.Property(m => m.EstimatedCost).HasPrecision(18, 2);
            entity.Property(m => m.ActualCost).HasPrecision(18, 2);

            entity.HasOne(m => m.Unit)
                .WithMany(u => u.MaintenanceRequests)
                .HasForeignKey(m => m.UnitId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(m => m.Tenant)
                .WithMany(t => t.MaintenanceRequests)
                .HasForeignKey(m => m.TenantId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Inspection
        modelBuilder.Entity<Inspection>(entity =>
        {
            entity.HasOne(i => i.Unit)
                .WithMany(u => u.Inspections)
                .HasForeignKey(i => i.UnitId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(i => i.Lease)
                .WithMany(l => l.Inspections)
                .HasForeignKey(i => i.LeaseId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State is EntityState.Added or EntityState.Modified)
            {
                var updatedAtProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "UpdatedAt");
                if (updatedAtProp is not null)
                {
                    updatedAtProp.CurrentValue = now;
                }

                if (entry.State is EntityState.Added)
                {
                    var createdAtProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "CreatedAt");
                    if (createdAtProp is not null && createdAtProp.CurrentValue is DateTime dt && dt == default)
                    {
                        createdAtProp.CurrentValue = now;
                    }
                }
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
