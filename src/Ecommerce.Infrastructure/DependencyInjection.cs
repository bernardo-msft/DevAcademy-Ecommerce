using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Application.Interfaces.Services;
using Ecommerce.Infrastructure.Persistence;
using Ecommerce.Infrastructure.Persistence.Repositories;
using Ecommerce.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Ecommerce.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<EcommerceDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("EcommerceConnection"),
                b => b.MigrationsAssembly(typeof(EcommerceDbContext).Assembly.FullName)));

        // Redis Connection
        var redisConnectionString = configuration.GetConnectionString("RedisConnection");
        if (string.IsNullOrEmpty(redisConnectionString))
        {
            throw new InvalidOperationException("Redis connection string 'RedisConnection' is not configured.");
        }

        services.AddSingleton<IConnectionMultiplexer>(sp => // Use factory to get logger if needed for connection events
        {
            var logger = sp.GetRequiredService<ILogger<IConnectionMultiplexer>>(); // Logger for this registration class
            try
            {
                var options = ConfigurationOptions.Parse(redisConnectionString);
                // Example: Modify some options
                options.SyncTimeout = 5000; // Milliseconds for synchronous operations
                options.ConnectTimeout = 10000; // Milliseconds to wait for connection
                options.ConnectRetry = 3; // Number of times to retry connection
                options.DefaultDatabase = 0; // Explicitly set default DB if needed

                logger.LogInformation("Attempting to connect to Redis with SyncTimeout: {SyncTimeout}, ConnectTimeout: {ConnectTimeout}",
                    options.SyncTimeout, options.ConnectTimeout);

                var multiplexer = ConnectionMultiplexer.Connect(options);

                multiplexer.ConnectionFailed += (sender, args) =>
                {
                    logger.LogError(args.Exception, "Redis connection failed. Endpoint: {Endpoint}, FailureType: {FailureType}", args.EndPoint, args.FailureType);
                };
                multiplexer.ConnectionRestored += (sender, args) =>
                {
                    logger.LogInformation("Redis connection restored. Endpoint: {Endpoint}, FailureType: {FailureType}", args.EndPoint, args.FailureType);
                };
                multiplexer.ErrorMessage += (sender, args) =>
                {
                    logger.LogError("Redis error message: {Message}", args.Message);
                };
                multiplexer.InternalError += (sender, args) =>
                {
                    logger.LogError(args.Exception, "Redis internal error. Origin: {Origin}", args.Origin);
                };

                logger.LogInformation("Successfully connected to Redis.");
                return multiplexer;
            }
            catch (RedisConnectionException ex)
            {
                logger.LogCritical(ex, "CRITICAL: Failed to connect to Redis during startup. Connection string: {RedisConnectionString}", redisConnectionString);
                // Depending on your application's requirements, you might:
                // 1. Rethrow to prevent application startup if Redis is essential.
                // 2. Return a null or a "dummy" multiplexer if the app can run in a degraded mode (not recommended for auth blacklist).
                throw; // Forcing app to fail startup if Redis connection fails initially.
            }
        });

        // Repositories
        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICartRepository, RedisCartRepository>();
        services.AddScoped<IFileStorageService, AzureBlobStorageService>();

        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services
        services.AddSingleton<ITokenService, TokenService>();
        services.AddSingleton<ITokenBlacklistService, RedisTokenBlacklistService>();

        return services;
    }
}