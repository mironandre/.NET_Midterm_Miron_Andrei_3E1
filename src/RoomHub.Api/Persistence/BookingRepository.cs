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

    public async Task<int> CountActiveForUserOnDayAsync(string bookedBy, DateOnly dayUtc, CancellationToken ct)
    {
        var dayStart = dayUtc.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var dayEnd = dayStart.AddDays(1);

        return await db.Bookings.CountAsync(
            b => b.Status == BookingStatus.Active
                 && b.BookedBy == bookedBy
                 && b.StartUtc >= dayStart
                 && b.StartUtc < dayEnd,
            ct);
    }
}
