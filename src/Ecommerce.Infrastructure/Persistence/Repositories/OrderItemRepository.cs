using Ecommerce.Application.Dtos;
using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Persistence.Repositories;

public class OrderItemRepository : GenericRepository<OrderItem>, IOrderItemRepository
{
    public OrderItemRepository(EcommerceDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<PopularProductDto>> GetMostPopularProductsAsync(int count)
    {
        return await _context.OrderItems
            .Include(oi => oi.Product)
            .GroupBy(oi => new { oi.ProductId, oi.Product!.Name })
            .Select(g => new PopularProductDto
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.Name,
                TotalQuantitySold = g.Sum(oi => oi.Quantity)
            })
            .OrderByDescending(p => p.TotalQuantitySold)
            .Take(count)
            .ToListAsync();
    }
}