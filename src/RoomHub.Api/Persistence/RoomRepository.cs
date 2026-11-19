using Microsoft.EntityFrameworkCore;
using RoomHub.Api.Domain;

namespace RoomHub.Api.Persistence;

public sealed class RoomRepository(RoomHubDbContext db) : IRoomRepository
{
    public async Task AddAsync(Room room, CancellationToken ct)
        => await db.Rooms.AddAsync(room, ct);

    public async Task<Room?> GetAsync(Guid id, CancellationToken ct)
        => await db.Rooms.FirstOrDefaultAsync(r => r.Id == id, ct);
}
