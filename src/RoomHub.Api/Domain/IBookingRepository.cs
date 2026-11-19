namespace RoomHub.Api.Domain;

public interface IBookingRepository
{
    Task AddAsync(Booking booking, CancellationToken ct);
    Task<Booking?> GetAsync(Guid id, CancellationToken ct);
    Task<IReadOnlyList<Booking>> ListActiveForRoomAsync(Guid roomId, CancellationToken ct);
}
