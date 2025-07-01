using Ecommerce.Application.Interfaces.Repositories;

namespace Ecommerce.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IProductRepository Products { get; }
    ICategoryRepository Categories { get; }
    IOrderRepository Orders { get; }
    IOrderItemRepository OrderItems { get; }

    Task<int> CompleteAsync();
}