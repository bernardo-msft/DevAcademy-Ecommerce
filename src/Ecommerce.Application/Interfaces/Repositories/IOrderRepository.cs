using Ecommerce.Application.Dtos;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Interfaces.Repositories;

public interface IOrderRepository : IGenericRepository<Order>
{
    Task<IEnumerable<Order>> GetOrdersByUserIdAsync(Guid userId);
    Task<IEnumerable<Order>> GetAllOrdersAsync();
    Task<IEnumerable<MonthlySalesDto>> GetMonthlySalesAsync(int year);
    Task<IEnumerable<TopCustomerDto>> GetTopCustomersAsync(int count);
}