namespace GroupPreOrder.Api.Models;

public class GuestSession
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid GroupReservationId { get; set; }
    public GroupReservation GroupReservation { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public GuestSessionType Type { get; set; }

    public bool IsSubmitted { get; set; }

    public List<GuestSessionItem> Items { get; set; } = new();
}

public enum GuestSessionType
{
    Guest,
    Owner
}