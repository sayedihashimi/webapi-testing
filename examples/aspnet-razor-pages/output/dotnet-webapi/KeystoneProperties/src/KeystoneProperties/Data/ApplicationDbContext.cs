using KeystoneProperties.Models;
using Microsoft.EntityFrameworkCore;

namespace KeystoneProperties.Data;

public sealed class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

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
            entity.Property(p => p.PropertyType).HasConversion<string>();
            entity.Property(p => p.CreatedAt).HasDefaultValueSql("datetime('now')");
            entity.Property(p => p.UpdatedAt).HasDefaultValueSql("datetime('now')");
        });

        // Unit
        modelBuilder.Entity<Unit>(entity =>
        {
            entity.HasIndex(u => new { u.PropertyId, u.UnitNumber }).IsUnique();
            entity.Property(u => u.Status).HasConversion<string>();
            entity.Property(u => u.MonthlyRent).HasColumnType("decimal(10,2)");
            entity.Property(u => u.DepositAmount).HasColumnType("decimal(10,2)");
            entity.Property(u => u.Bathrooms).HasColumnType("decimal(3,1)");
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("datetime('now')");
            entity.Property(u => u.UpdatedAt).HasDefaultValueSql("datetime('now')");
            entity.HasOne(u => u.Property)
                  .WithMany(p => p.Units)
                  .HasForeignKey(u => u.PropertyId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Tenant
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasIndex(t => t.Email).IsUnique();
            entity.Property(t => t.CreatedAt).HasDefaultValueSql("datetime('now')");
            entity.Property(t => t.UpdatedAt).HasDefaultValueSql("datetime('now')");
        });

        // Lease
        modelBuilder.Entity<Lease>(entity =>
        {
            entity.Property(l => l.Status).HasConversion<string>();
            entity.Property(l => l.DepositStatus).HasConversion<string>();
            entity.Property(l => l.MonthlyRentAmount).HasColumnType("decimal(10,2)");
            entity.Property(l => l.DepositAmount).HasColumnType("decimal(10,2)");
            entity.Property(l => l.CreatedAt).HasDefaultValueSql("datetime('now')");
            entity.Property(l => l.UpdatedAt).HasDefaultValueSql("datetime('now')");
            entity.HasOne(l => l.Unit)
                  .WithMany(u => u.Leases)
                  .HasForeignKey(l => l.UnitId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(l => l.Tenant)
                  .WithMany(t => t.Leases)
                  .HasForeignKey(l => l.TenantId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(l => l.RenewalOfLease)
                  .WithMany(l => l.Renewals)
                  .HasForeignKey(l => l.RenewalOfLeaseId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Payment
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.Property(p => p.PaymentMethod).HasConversion<string>();
            entity.Property(p => p.PaymentType).HasConversion<string>();
            entity.Property(p => p.Status).HasConversion<string>();
            entity.Property(p => p.Amount).HasColumnType("decimal(10,2)");
            entity.Property(p => p.CreatedAt).HasDefaultValueSql("datetime('now')");
            entity.HasOne(p => p.Lease)
                  .WithMany(l => l.Payments)
                  .HasForeignKey(p => p.LeaseId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // MaintenanceRequest
        modelBuilder.Entity<MaintenanceRequest>(entity =>
        {
            entity.Property(m => m.Priority).HasConversion<string>();
            entity.Property(m => m.Status).HasConversion<string>();
            entity.Property(m => m.Category).HasConversion<string>();
            entity.Property(m => m.EstimatedCost).HasColumnType("decimal(10,2)");
            entity.Property(m => m.ActualCost).HasColumnType("decimal(10,2)");
            entity.Property(m => m.CreatedAt).HasDefaultValueSql("datetime('now')");
            entity.Property(m => m.UpdatedAt).HasDefaultValueSql("datetime('now')");
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
            entity.Property(i => i.InspectionType).HasConversion<string>();
            entity.Property(i => i.OverallCondition).HasConversion<string>();
            entity.Property(i => i.CreatedAt).HasDefaultValueSql("datetime('now')");
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
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            var now = DateTime.UtcNow;
            if (entry.Metadata.FindProperty("UpdatedAt") is not null)
            {
                entry.Property("UpdatedAt").CurrentValue = now;
            }
            if (entry.State == EntityState.Added)
            {
                if (entry.Metadata.FindProperty("CreatedAt") is not null)
                {
                    entry.Property("CreatedAt").CurrentValue = now;
                }
                if (entry.Metadata.FindProperty("SubmittedDate") is not null
                    && entry.Property("SubmittedDate").CurrentValue is DateTime dt && dt == default)
                {
                    entry.Property("SubmittedDate").CurrentValue = now;
                }
            }
        }
    }
}
