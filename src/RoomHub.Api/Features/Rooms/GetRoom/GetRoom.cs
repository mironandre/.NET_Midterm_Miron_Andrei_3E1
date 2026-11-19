using RoomHub.Api.Domain;
using RoomHub.Api.Features.Rooms.CreateRoom;

namespace RoomHub.Api.Features.Rooms.GetRoom;

public record GetRoomQuery(Guid Id);

public sealed class GetRoomHandler(IRoomRepository rooms)
    : ICommandHandler<GetRoomQuery, RoomResponse>
{
    public async Task<Result<RoomResponse>> HandleAsync(GetRoomQuery query, CancellationToken ct)
    {
        var room = await rooms.GetAsync(query.Id, ct);
        if (room is null)
            return Error.NotFound("RoomNotFound", "The room does not exist.");

        return new RoomResponse(room.Id, room.Name, room.Capacity);
    }
}

public static class GetRoomEndpoint
{
    public static void MapGetRoom(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/rooms/{id:guid}", async (
            Guid id,
            GetRoomHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new GetRoomQuery(id), ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.Error!.ToProblem();
        });
    }
}
