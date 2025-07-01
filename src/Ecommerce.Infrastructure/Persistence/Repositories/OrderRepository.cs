using Ecommerce.Application.Dtos;
using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Persistence.Repositories;

public class OrderRepository : GenericRepository<Order>, IOrderRepository
{
    public OrderRepository(EcommerceDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(Guid userId)
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<Order>> GetAllOrdersAsync()
    {
        return await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .Include(o => o.User)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<MonthlySalesDto>> GetMonthlySalesAsync(int year)
    {
        return await _context.Orders
            .Where(o => o.OrderDate.Year == year)
            .GroupBy(o => o.OrderDate.Month)
            .Select(g => new MonthlySalesDto
            {
                Year = year,
                Month = g.Key,
                TotalSales = g.Sum(o => o.TotalPrice)
            })
            .OrderBy(s => s.Month)
            .ToListAsync();
    }

    public async Task<IEnumerable<TopCustomerDto>> GetTopCustomersAsync(int count)
    {
        return await _context.Orders
            .Include(o => o.User)
            .GroupBy(o => new { o.UserId, o.User!.Name })
            .Select(g => new TopCustomerDto
            {
                UserId = g.Key.UserId,
                CustomerName = g.Key.Name,
                TotalPurchaseAmount = g.Sum(o => o.TotalPrice)
            })
            .OrderByDescending(c => c.TotalPurchaseAmount)
            .Take(count)
            .ToListAsync();
    }
}