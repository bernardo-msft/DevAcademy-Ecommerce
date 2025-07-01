using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Infrastructure.Persistence.Repositories;

public class CategoryRepository : GenericRepository<Category>, ICategoryRepository
{
    public CategoryRepository(EcommerceDbContext context) : base(context)
    {
    }
}