using KeystoneProperties.Models;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
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

        modelBuilder.Entity<Property>(e =>
        {
            e.HasMany(p => p.Units)
             .WithOne(u => u.Property)
             .HasForeignKey(u => u.PropertyId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Unit>(e =>
        {
            e.HasIndex(u => new { u.PropertyId, u.UnitNumber }).IsUnique();

            e.Property(u => u.MonthlyRent).HasColumnType("decimal(18,2)");
            e.Property(u => u.DepositAmount).HasColumnType("decimal(18,2)");
            e.Property(u => u.Bathrooms).HasColumnType("decimal(3,1)");
        });

        modelBuilder.Entity<Tenant>(e =>
        {
            e.HasIndex(t => t.Email).IsUnique();
        });

        modelBuilder.Entity<Lease>(e =>
        {
            e.HasOne(l => l.Unit)
             .WithMany(u => u.Leases)
             .HasForeignKey(l => l.UnitId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(l => l.Tenant)
             .WithMany(t => t.Leases)
             .HasForeignKey(l => l.TenantId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(l => l.RenewalOfLease)
             .WithMany()
             .HasForeignKey(l => l.RenewalOfLeaseId)
             .OnDelete(DeleteBehavior.Restrict);

            e.Property(l => l.MonthlyRentAmount).HasColumnType("decimal(18,2)");
            e.Property(l => l.DepositAmount).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<Payment>(e =>
        {
            e.HasOne(p => p.Lease)
             .WithMany(l => l.Payments)
             .HasForeignKey(p => p.LeaseId)
             .OnDelete(DeleteBehavior.Restrict);

            e.Property(p => p.Amount).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<MaintenanceRequest>(e =>
        {
            e.HasOne(m => m.Unit)
             .WithMany(u => u.MaintenanceRequests)
             .HasForeignKey(m => m.UnitId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(m => m.Tenant)
             .WithMany(t => t.MaintenanceRequests)
             .HasForeignKey(m => m.TenantId)
             .OnDelete(DeleteBehavior.Restrict);

            e.Property(m => m.EstimatedCost).HasColumnType("decimal(18,2)");
            e.Property(m => m.ActualCost).HasColumnType("decimal(18,2)");
        });

        modelBuilder.Entity<Inspection>(e =>
        {
            e.HasOne(i => i.Unit)
             .WithMany(u => u.Inspections)
             .HasForeignKey(i => i.UnitId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(i => i.Lease)
             .WithMany(l => l.Inspections)
             .HasForeignKey(i => i.LeaseId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }

    public override int SaveChanges()
    {
        SetTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void SetTimestamps()
    {
        var now = DateTime.UtcNow;
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity is Property p) { p.CreatedAt = now; p.UpdatedAt = now; }
                else if (entry.Entity is Unit u) { u.CreatedAt = now; u.UpdatedAt = now; }
                else if (entry.Entity is Tenant t) { t.CreatedAt = now; t.UpdatedAt = now; }
                else if (entry.Entity is Lease l) { l.CreatedAt = now; l.UpdatedAt = now; }
                else if (entry.Entity is Payment pay) { pay.CreatedAt = now; }
                else if (entry.Entity is MaintenanceRequest mr) { mr.CreatedAt = now; mr.UpdatedAt = now; mr.SubmittedDate = now; }
                else if (entry.Entity is Inspection ins) { ins.CreatedAt = now; }
            }
            else if (entry.State == EntityState.Modified)
            {
                if (entry.Entity is Property p) { p.UpdatedAt = now; }
                else if (entry.Entity is Unit u) { u.UpdatedAt = now; }
                else if (entry.Entity is Tenant t) { t.UpdatedAt = now; }
                else if (entry.Entity is Lease l) { l.UpdatedAt = now; }
                else if (entry.Entity is MaintenanceRequest mr) { mr.UpdatedAt = now; }
            }
        }
    }
}
