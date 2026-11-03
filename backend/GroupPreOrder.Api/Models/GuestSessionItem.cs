namespace GroupPreOrder.Api.Models;

public class GuestSessionItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid GuestSessionId { get; set; }
    public GuestSession GuestSession { get; set; } = null!;

    public Guid DishId { get; set; }
    public Dish Dish { get; set; } = null!;

    public int Quantity { get; set; }
}