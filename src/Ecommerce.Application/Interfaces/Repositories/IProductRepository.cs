using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Interfaces.Repositories;

public interface IProductRepository : IGenericRepository<Product>
{
    Task<IEnumerable<Product>> GetAllWithCategoryAsync(Guid? categoryId = null);
    Task<Product?> GetByIdWithCategoryAsync(Guid id);
    Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm, Guid? categoryId = null);
}