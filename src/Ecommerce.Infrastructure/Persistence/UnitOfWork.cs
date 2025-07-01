using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Infrastructure.Persistence.Repositories;

namespace Ecommerce.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly EcommerceDbContext _context;
    private IUserRepository? _userRepository;
    private IProductRepository? _productRepository;
    private ICategoryRepository? _categoryRepository;
    private IOrderRepository? _orderRepository;
    private IOrderItemRepository? _orderItemRepository;

    public UnitOfWork(EcommerceDbContext context)
    {
        _context = context;
    }

    public IUserRepository Users => _userRepository ??= new UserRepository(_context);

    public IProductRepository Products => _productRepository ??= new ProductRepository(_context);

    public ICategoryRepository Categories => _categoryRepository ??= new CategoryRepository(_context);

    public IOrderRepository Orders => _orderRepository ??= new OrderRepository(_context);

    public IOrderItemRepository OrderItems => _orderItemRepository ??= new OrderItemRepository(_context);

    public async Task<int> CompleteAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}