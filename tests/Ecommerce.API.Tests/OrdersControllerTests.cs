using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Ecommerce.API.Controllers;
using Ecommerce.Application.Dtos;
using Ecommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Ecommerce.Domain.Enums;

namespace Ecommerce.API.Tests;

public class OrdersControllerTests
{
    private readonly Mock<IOrderService> _mockOrderService;
    private readonly OrdersController _controller;

    public OrdersControllerTests()
    {
        _mockOrderService = new Mock<IOrderService>();
        _controller = new OrdersController(_mockOrderService.Object);
    }

    private void SetupUser(string userId)
    {
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId) };
        var identity = new ClaimsIdentity(claims);
        var claimsPrincipal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };
    }

    [Fact]
    public async Task PlaceOrder_WhenSuccessful_ReturnsOkWithOrder()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orderDto = new OrderDto { Id = Guid.NewGuid(), UserId = userId };
        SetupUser(userId.ToString());
        _mockOrderService.Setup(s => s.PlaceOrderAsync(userId)).ReturnsAsync((orderDto, null));

        // Act
        var result = await _controller.PlaceOrder();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedOrder = Assert.IsType<OrderDto>(okResult.Value);
        Assert.Equal(orderDto.Id, returnedOrder.Id);
    }

    [Fact]
    public async Task PlaceOrder_WhenCartIsEmpty_ReturnsBadRequest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var errorMessage = "Shopping cart is empty.";
        SetupUser(userId.ToString());
        _mockOrderService.Setup(s => s.PlaceOrderAsync(userId)).ReturnsAsync((null, errorMessage));

        // Act
        var result = await _controller.PlaceOrder();

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task GetUserOrders_WhenAuthenticated_ReturnsOkWithOrders()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var orders = new List<OrderDto> { new OrderDto { Id = Guid.NewGuid(), UserId = userId } };
        SetupUser(userId.ToString());
        _mockOrderService.Setup(s => s.GetUserOrdersAsync(userId)).ReturnsAsync(orders);

        // Act
        var result = await _controller.GetUserOrders();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedOrders = Assert.IsAssignableFrom<IEnumerable<OrderDto>>(okResult.Value);
        Assert.Single(returnedOrders);
    }

    [Fact]
    public async Task GetAllOrders_AsAdmin_ReturnsOkWithAllOrders()
    {
        // Arrange
        var orders = new List<OrderDto> { new OrderDto(), new OrderDto() };
        _mockOrderService.Setup(s => s.GetAllOrdersAsync()).ReturnsAsync(orders);

        // Act
        var result = await _controller.GetAllOrders();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedOrders = Assert.IsAssignableFrom<IEnumerable<OrderDto>>(okResult.Value);
        Assert.Equal(2, new List<OrderDto>(returnedOrders).Count);
    }

    [Fact]
    public async Task UpdateOrderStatus_WhenSuccessful_ReturnsOkWithUpdatedOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var statusDto = new UpdateOrderStatusDto { Status = OrderStatus.Shipped };
        var updatedOrder = new OrderDto { Id = orderId, Status = OrderStatus.Shipped };
        _mockOrderService.Setup(s => s.UpdateOrderStatusAsync(orderId, statusDto)).ReturnsAsync((updatedOrder, null));

        // Act
        var result = await _controller.UpdateOrderStatus(orderId, statusDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedOrder = Assert.IsType<OrderDto>(okResult.Value);
        Assert.Equal(OrderStatus.Shipped, returnedOrder.Status);
    }

    [Fact]
    public async Task UpdateOrderStatus_WhenOrderNotFound_ReturnsBadRequest()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var statusDto = new UpdateOrderStatusDto { Status = OrderStatus.Shipped };
        var errorMessage = "Order not found.";
        _mockOrderService.Setup(s => s.UpdateOrderStatusAsync(orderId, statusDto)).ReturnsAsync((null, errorMessage));

        // Act
        var result = await _controller.UpdateOrderStatus(orderId, statusDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }
}