using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Persistence.Repositories;

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(EcommerceDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Product>> GetAllWithCategoryAsync(Guid? categoryId = null)
    {
        var query = _context.Products.Include(p => p.Category).AsQueryable();

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<Product?> GetByIdWithCategoryAsync(Guid id)
    {
        return await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm, Guid? categoryId = null)
    {
        var lowerCaseSearchTerm = searchTerm.Trim().ToLower();
        var query = _context.Products
            .Include(p => p.Category)
            .Where(p => p.Name.ToLower().Contains(lowerCaseSearchTerm) ||
                        p.Description.ToLower().Contains(lowerCaseSearchTerm));

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId.Value);
        }

        return await query.ToListAsync();
    }
}