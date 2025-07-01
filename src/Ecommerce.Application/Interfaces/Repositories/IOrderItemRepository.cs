using Ecommerce.Application.Dtos;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Interfaces.Repositories;

public interface IOrderItemRepository : IGenericRepository<OrderItem>
{
    Task<IEnumerable<PopularProductDto>> GetMostPopularProductsAsync(int count);
}