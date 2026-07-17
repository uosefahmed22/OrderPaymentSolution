using M01.OrderPaymentSystem.OrderServiceApi.Requests;
using M01.OrderPaymentSystem.OrderServiceApi.Responses;

namespace M01.OrderPaymentSystem.OrderServiceApi.Services;

public interface IOrderService
{
    Task<OrderResponse?> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<OrderResponse> CreateAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);
    Task PayAsync(Guid orderId, PaymentRequest request, CancellationToken cancellationToken = default);
    Task CancelAsync(Guid orderId, CancellationToken cancellationToken = default);
}
