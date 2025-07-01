using Ecommerce.Application.Dtos;

namespace Ecommerce.Application.Interfaces.Services;

public interface IOrderService
{
    Task<(OrderDto? Order, string? ErrorMessage)> PlaceOrderAsync(Guid userId);
    Task<IEnumerable<OrderDto>> GetUserOrdersAsync(Guid userId);
    Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
    Task<(OrderDto? Order, string? ErrorMessage)> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusDto statusDto);
}