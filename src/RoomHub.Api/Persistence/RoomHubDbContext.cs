using Microsoft.EntityFrameworkCore;
using RoomHub.Api.Domain;

namespace RoomHub.Api.Persistence;

public sealed class RoomHubDbContext(DbContextOptions<RoomHubDbContext> options) : DbContext(options)
{
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Booking> Bookings => Set<Booking>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Room>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Name).IsRequired();
            e.Property(r => r.Capacity);
        });

        modelBuilder.Entity<Booking>(e =>
        {
            e.HasKey(b => b.Id);
            e.Property(b => b.RoomId);
            e.Property(b => b.BookedBy).IsRequired();
            e.Property(b => b.StartUtc);
            e.Property(b => b.EndUtc);
            e.Property(b => b.Attendees);
            e.Property(b => b.Status).HasConversion<string>();
        });
    }
}
