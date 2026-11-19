namespace RoomHub.Api.Domain;

public class Room
{
    // Parameterless ctor for EF Core materialization.
    private Room() { }

    private Room(Guid id, string name, int capacity)
    {
        Id = id;
        Name = name;
        Capacity = capacity;
    }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int Capacity { get; private set; }

    // R1: a room has a non-empty name and a strictly positive capacity.
    public static Result<Room> Create(string? name, int capacity)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Error.Validation("InvalidName", "Room name is required.");

        if (capacity <= 0)
            return Error.Validation("InvalidCapacity", "Capacity must be strictly positive.");

        return new Room(Guid.NewGuid(), name.Trim(), capacity);
    }
}
