namespace M01.OrderPaymentSystem.OrderServiceApi.Models;

public class Order
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? PaymentReference { get; set; }
    public decimal TotalAmount => Items.Sum(i => i.Total);
    public List<OrderItem> Items { get; set; } = [];

    private Order() { }

    public Order(Guid customerId, IEnumerable<OrderItem> items)
    {
        if (!items.Any()) throw new ArgumentException("Order must have at least one item");

        CustomerId = customerId;
        CreatedAt = DateTime.UtcNow;
        Items = items.ToList();
    }
}
