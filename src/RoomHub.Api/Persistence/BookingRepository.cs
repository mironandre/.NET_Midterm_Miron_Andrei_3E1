using Microsoft.EntityFrameworkCore;
using RoomHub.Api.Domain;

namespace RoomHub.Api.Persistence;

public sealed class BookingRepository(RoomHubDbContext db) : IBookingRepository
{
    public async Task AddAsync(Booking booking, CancellationToken ct)
        => await db.Bookings.AddAsync(booking, ct);

    public async Task<Booking?> GetAsync(Guid id, CancellationToken ct)
        => await db.Bookings.FirstOrDefaultAsync(b => b.Id == id, ct);

    public async Task<IReadOnlyList<Booking>> ListActiveForRoomAsync(Guid roomId, CancellationToken ct)
        => await db.Bookings
            .Where(b => b.RoomId == roomId && b.Status == BookingStatus.Active)
            .ToListAsync(ct);
}
