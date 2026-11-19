namespace RoomHub.Api.Domain;

public interface IBookingRepository
{
    Task AddAsync(Booking booking, CancellationToken ct);
    Task<Booking?> GetAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Booking>> ListActiveForRoomAsync(Guid roomId, CancellationToken ct);

    // R9: count a participant's active bookings whose startUtc falls on a given UTC date, across all rooms.
    Task<int> CountActiveForUserOnDayAsync(string bookedBy, DateOnly dayUtc, CancellationToken ct);
}
