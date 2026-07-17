namespace M01.OrderPaymentSystem.PaymentServiceApi.Models;

public class Payment
{
    public Guid OrderId { get; set; }

    public string? PaymentReference { get; set; }

    public DateTime ProcessedAt { get; set; }

    public decimal Amount { get; set; }
}