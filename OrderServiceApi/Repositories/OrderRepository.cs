using M01.OrderPaymentSystem.OrderServiceApi.Models;
using M01.RepositoryPattern.Data;
using Microsoft.EntityFrameworkCore;

namespace M01.OrderPaymentSystem.OrderServiceApi.Repositories;

public class OrderRepository(AppDbContext context) : IOrderRepository
{
    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task AddAsync(Order order, CancellationToken cancellationToken = default)
    {
        await context.Orders.AddAsync(order, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Order order, CancellationToken cancellationToken = default)
    {
        context.Orders.Update(order);

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(Order order, CancellationToken cancellationToken = default)
    {
        context.Orders.Remove(order);

        await context.SaveChangesAsync(cancellationToken);
    }
}