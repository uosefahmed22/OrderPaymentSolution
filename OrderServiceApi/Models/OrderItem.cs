namespace M01.OrderPaymentSystem.OrderServiceApi.Models;

public class OrderItem
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Total => Quantity * UnitPrice;

    private OrderItem() { } // For EF

    public OrderItem(Guid productId, int quantity, decimal unitPrice)
    {
        if (quantity <= 0) throw new ArgumentException("Quantity must be > 0");
        if (unitPrice <= 0) throw new ArgumentException("Price must be > 0");

        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
}