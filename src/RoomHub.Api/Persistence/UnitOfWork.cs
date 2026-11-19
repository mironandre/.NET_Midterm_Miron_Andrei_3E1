using RoomHub.Api.Domain;

namespace RoomHub.Api.Persistence;

public sealed class UnitOfWork(RoomHubDbContext db) : IUnitOfWork
{
    public async Task SaveChangesAsync(CancellationToken ct)
        => await db.SaveChangesAsync(ct);
}
