using System.Globalization;
using M01.OrderPaymentSystem.OrderServiceApi.Models;
using M01.OrderPaymentSystem.OrderServiceApi.Repositories;
using M01.OrderPaymentSystem.OrderServiceApi.Requests;
using M01.OrderPaymentSystem.OrderServiceApi.Responses;

namespace M01.OrderPaymentSystem.OrderServiceApi.Services;

public class OrderService(IOrderRepository repository,
                          HttpClient paymentHttpClient,
                          ILogger<OrderService> logger) : IOrderService
{
    public async Task<OrderResponse?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await repository.GetByIdAsync(orderId, cancellationToken);

        if (order is null)
        {
            logger.LogWarning("Order not found. OrderId: {OrderId}", orderId);

            return null;
        }

        return OrderResponse.FromModel(order);
    }

    public async Task<OrderResponse> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        var items = request.Items.Select(i =>
            new OrderItem(i.ProductId, i.Quantity, i.UnitPrice)).ToList();

        var order = new Order(request.CustomerId, items);

        await repository.AddAsync(order, cancellationToken);

        logger.LogInformation("Created new order. OrderId: {OrderId}, CustomerId: {CustomerId}, ItemsCount: {ItemCount}",
            order.Id, order.CustomerId, order.Items.Count);

        return OrderResponse.FromModel(order);
    }

    public async Task PayAsync(Guid orderId, PaymentRequest request, CancellationToken cancellationToken = default)
    {
        var order = await repository.GetByIdAsync(orderId, cancellationToken);

        if (order is null)
        {
            logger.LogWarning("Order not found. OrderId: {OrderId}", orderId);

            throw new KeyNotFoundException($"Order {orderId} not found");
        }

        if (order.PaidAt.HasValue)
        {
            logger.LogWarning("Attempted to pay already-paid order. OrderId: {OrderId}", orderId);

            throw new InvalidOperationException("Order has already been paid.");
        }

        var payload = new Dictionary<string, string?>
        {
            { "OrderId", orderId.ToString() },
            { "Amount", order.TotalAmount.ToString(CultureInfo.InvariantCulture) },
            { "Currency", "USD" },
            { "PaymentMethod", request.PaymentMethod.ToString() },
            { "CardNumber", request.CardNumber },
            { "CardHolderName", request.CardHolderName },
        };

        HttpResponseMessage response;
        try
        {
            response = await paymentHttpClient.PostAsJsonAsync("Payment/process", payload, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "HTTP request to payment gateway failed. OrderId: {OrderId}", orderId);

            throw;
        }

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();

            logger.LogError("Payment failed. Status: {StatusCode}, Body: {Body}, OrderId: {OrderId}",
             (int)response.StatusCode, body, orderId);

            throw new InvalidOperationException($"Payment failed with status: {(int)response.StatusCode}, body: {body}");
        }

        var paymentResult = await response.Content.ReadFromJsonAsync<PaymentResponse>(cancellationToken);

        if (paymentResult is null)
        {
            var raw = await response.Content.ReadAsStringAsync();

            // email: ab******@y****.com
            logger.LogError("Deserialization failed. Raw response: {Raw}, OrderId: {OrderId}", raw, orderId);

            throw new InvalidOperationException($"Deserialization failed. Raw response: {raw}");
        }

        if (!paymentResult.Success)
        {
            logger.LogWarning("Payment declined by gateway. OrderId: {OrderId}, TransactionId: {TransactionId}",
            orderId, paymentResult.TransactionId);

            throw new InvalidOperationException("Payment was declined");
        }

        order.PaidAt = DateTime.UtcNow;
        order.PaymentReference = paymentResult.TransactionId;

        await repository.UpdateAsync(order, cancellationToken);

        logger.LogInformation("Payment successful for OrderId: {OrderId}, TransactionId: {TransactionId}",
           orderId, paymentResult.TransactionId);
    }


    public async Task CancelAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Attempting to cancel order. OrderId: {OrderId}", orderId);

        var order = await repository.GetByIdAsync(orderId, cancellationToken);

        if (order is null)
        {
            logger.LogWarning("Order not found. OrderId: {OrderId}", orderId);

            throw new KeyNotFoundException($"Order {orderId} not found");
        }

        if (order.PaidAt.HasValue)
        {
            logger.LogWarning("Tried to cancel a paid order. OrderId: {OrderId}", orderId);

            throw new InvalidOperationException("Paid invoice cannot be cancelled.");
        }

        await repository.RemoveAsync(order, cancellationToken);

        logger.LogInformation("Order cancelled successfully. OrderId: {OrderId}", orderId);
    }

}
