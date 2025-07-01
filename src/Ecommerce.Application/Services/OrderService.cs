using Ecommerce.Application.Dtos;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Application.Interfaces.Services;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Application.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICartRepository _cartRepository;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IUnitOfWork unitOfWork, ICartRepository cartRepository, ILogger<OrderService> logger)
    {
        _unitOfWork = unitOfWork;
        _cartRepository = cartRepository;
        _logger = logger;
    }

    public async Task<(OrderDto? Order, string? ErrorMessage)> PlaceOrderAsync(Guid userId)
    {
        var cart = await _cartRepository.GetByIdAsync(userId.ToString());
        if (cart == null || !cart.Items.Any())
        {
            return (null, "Shopping cart is empty.");
        }

        var orderItems = new List<OrderItem>();
        decimal totalPrice = 0;

        foreach (var item in cart.Items)
        {
            var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
            if (product == null || product.StockQuantity < item.Quantity)
            {
                return (null, $"Product '{item.ProductName}' is out of stock or does not exist.");
            }

            product.StockQuantity -= item.Quantity;
            _unitOfWork.Products.Update(product);

            orderItems.Add(new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = item.Price
            });
            totalPrice += item.Price * item.Quantity;
        }

        var order = new Order
        {
            UserId = userId,
            OrderDate = DateTime.UtcNow,
            TotalPrice = totalPrice,
            Status = OrderStatus.Pending,
            OrderItems = orderItems
        };

        await _unitOfWork.Orders.AddAsync(order);
        await _unitOfWork.CompleteAsync();
        await _cartRepository.DeleteAsync(userId.ToString());

        _logger.LogInformation("Order {OrderId} placed successfully for user {UserId}", order.Id, userId);

        return (MapOrderToDto(order), null);
    }

    public async Task<IEnumerable<OrderDto>> GetUserOrdersAsync(Guid userId)
    {
        var orders = await _unitOfWork.Orders.GetOrdersByUserIdAsync(userId);
        return orders.Select(MapOrderToDto);
    }

    public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
    {
        var orders = await _unitOfWork.Orders.GetAllOrdersAsync();
        return orders.Select(MapOrderToDto);
    }

    public async Task<(OrderDto? Order, string? ErrorMessage)> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusDto statusDto)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId);

        if (order == null)
        {
            return (null, "Order not found.");
        }

        order.Status = statusDto.Status;
        _unitOfWork.Orders.Update(order);
        await _unitOfWork.CompleteAsync();

        _logger.LogInformation("Order {OrderId} status updated to {Status}", order.Id, order.Status);

        var updatedOrder = await _unitOfWork.Orders.GetOrdersByUserIdAsync(order.UserId); // Re-fetch to get relations
        return (MapOrderToDto(updatedOrder.First(o => o.Id == orderId)), null);
    }

    private static OrderDto MapOrderToDto(Order order)
    {
        return new OrderDto
        {
            Id = order.Id,
            UserId = order.UserId,
            OrderDate = order.OrderDate,
            TotalPrice = order.TotalPrice,
            Status = order.Status,
            OrderItems = order.OrderItems.Select(oi => new OrderItemDto
            {
                ProductId = oi.ProductId,
                ProductName = oi.Product?.Name ?? "N/A",
                Quantity = oi.Quantity,
                Price = oi.Price
            }).ToList()
        };
    }
}