namespace GroupPreOrder.Api.Models;

public class Dish
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    public decimal Price { get; set; }
}