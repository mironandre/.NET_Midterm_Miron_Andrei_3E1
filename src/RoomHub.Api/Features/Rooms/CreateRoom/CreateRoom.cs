using RoomHub.Api.Domain;

namespace RoomHub.Api.Features.Rooms.CreateRoom;

public record CreateRoomRequest(string? Name, int Capacity);

public record CreateRoomCommand(string? Name, int Capacity);

public record RoomResponse(Guid Id, string Name, int Capacity);

public sealed class CreateRoomHandler(IRoomRepository rooms, IUnitOfWork unitOfWork)
    : ICommandHandler<CreateRoomCommand, RoomResponse>
{
    public async Task<Result<RoomResponse>> HandleAsync(CreateRoomCommand command, CancellationToken ct)
    {
        var result = Room.Create(command.Name, command.Capacity);
        if (result.IsFailure)
            return result.Error!;

        var room = result.Value;
        await rooms.AddAsync(room, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return new RoomResponse(room.Id, room.Name, room.Capacity);
    }
}

public static class CreateRoomEndpoint
{
    public static void MapCreateRoom(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/rooms", async (
            CreateRoomRequest request,
            CreateRoomHandler handler,
            CancellationToken ct) =>
        {
            var result = await handler.HandleAsync(new CreateRoomCommand(request.Name, request.Capacity), ct);

            return result.IsSuccess
                ? Results.Created($"/api/rooms/{result.Value.Id}", result.Value)
                : result.Error!.ToProblem();
        });
    }
}
