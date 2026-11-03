namespace GroupPreOrder.Api.Models;

public class OrderItem
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public Guid DishId { get; set; }
    public Dish Dish { get; set; } = null!;

    public int Quantity { get; set; }
}