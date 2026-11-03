namespace GroupPreOrder.Api.DTOs;

public class GroupSessionsResponse
{
    public List<ParticipantDto> Participants { get; set; } = new();

    public decimal Total { get; set; }
}

public class ParticipantDto
{
    public Guid Id { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public List<SessionItemDto> Items { get; set; } = new();

    public decimal Total { get; set; }

    public bool IsSubmitted { get; set; }
}

public class SessionItemDto
{
    public DishDto Dish { get; set; } = new();

    public int Quantity { get; set; }
}

public class DishDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }
}