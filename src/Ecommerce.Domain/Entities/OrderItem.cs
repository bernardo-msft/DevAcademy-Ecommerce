namespace Ecommerce.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; } // Price at the time of purchase

    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;
}