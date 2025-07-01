using Ecommerce.Application.Dtos;
using Ecommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Ecommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public async Task<IActionResult> PlaceOrder()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Invalid user identifier.");
        }

        var (order, errorMessage) = await _orderService.PlaceOrderAsync(userId);

        if (errorMessage != null)
        {
            return BadRequest(new { Message = errorMessage });
        }

        return Ok(order);
    }

    [HttpGet]
    public async Task<IActionResult> GetUserOrders()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized("Invalid user identifier.");
        }

        var orders = await _orderService.GetUserOrdersAsync(userId);
        return Ok(orders);
    }

    [HttpGet("all")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> GetAllOrders()
    {
        var orders = await _orderService.GetAllOrdersAsync();
        return Ok(orders);
    }

    [HttpPut("{orderId}/status")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateOrderStatus(Guid orderId, [FromBody] UpdateOrderStatusDto statusDto)
    {
        var (order, errorMessage) = await _orderService.UpdateOrderStatusAsync(orderId, statusDto);

        if (errorMessage != null)
        {
            return BadRequest(new { Message = errorMessage });
        }

        return Ok(order);
    }
}