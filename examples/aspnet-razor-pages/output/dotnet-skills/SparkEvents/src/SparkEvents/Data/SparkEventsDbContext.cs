using Microsoft.EntityFrameworkCore;
using SparkEvents.Models;

namespace SparkEvents.Data;

public class SparkEventsDbContext : DbContext
{
    public SparkEventsDbContext(DbContextOptions<SparkEventsDbContext> options) : base(options) { }

    public DbSet<EventCategory> EventCategories => Set<EventCategory>();
    public DbSet<Venue> Venues => Set<Venue>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<TicketType> TicketTypes => Set<TicketType>();
    public DbSet<Attendee> Attendees => Set<Attendee>();
    public DbSet<Registration> Registrations => Set<Registration>();
    public DbSet<CheckIn> CheckIns => Set<CheckIn>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<EventCategory>(entity =>
        {
            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<Venue>(entity =>
        {
            entity.HasIndex(e => e.Name);
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasOne(e => e.EventCategory)
                .WithMany(c => c.Events)
                .HasForeignKey(e => e.EventCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Venue)
                .WithMany(v => v.Events)
                .HasForeignKey(e => e.VenueId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.StartDate);
        });

        modelBuilder.Entity<TicketType>(entity =>
        {
            entity.HasOne(t => t.Event)
                .WithMany(e => e.TicketTypes)
                .HasForeignKey(t => t.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Attendee>(entity =>
        {
            entity.HasIndex(a => a.Email).IsUnique();
        });

        modelBuilder.Entity<Registration>(entity =>
        {
            entity.HasOne(r => r.Event)
                .WithMany(e => e.Registrations)
                .HasForeignKey(r => r.EventId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Attendee)
                .WithMany(a => a.Registrations)
                .HasForeignKey(r => r.AttendeeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.TicketType)
                .WithMany(t => t.Registrations)
                .HasForeignKey(r => r.TicketTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(r => r.ConfirmationNumber).IsUnique();
            entity.HasIndex(r => new { r.EventId, r.AttendeeId });
        });

        modelBuilder.Entity<CheckIn>(entity =>
        {
            entity.HasOne(c => c.Registration)
                .WithOne(r => r.CheckIn)
                .HasForeignKey<CheckIn>(c => c.RegistrationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(c => c.RegistrationId).IsUnique();
        });
    }
}
