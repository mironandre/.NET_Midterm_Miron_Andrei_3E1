namespace RoomHub.Api.Domain;

public interface IRoomRepository
{
    Task AddAsync(Room room, CancellationToken ct);
    Task<Room?> GetAsync(Guid id, CancellationToken ct);
}
