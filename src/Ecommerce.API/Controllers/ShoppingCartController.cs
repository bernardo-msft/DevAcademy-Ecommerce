using Ecommerce.API.Middleware;
using Ecommerce.Application.Dtos;
using Ecommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;

namespace Ecommerce.API.Controllers;

[ApiController]
[Route("api/cart")]
public class ShoppingCartController : ControllerBase
{
    private readonly IShoppingCartService _shoppingCartService;
    private const string CartIdCookieName = "AnonymousCartId";

    public ShoppingCartController(IShoppingCartService shoppingCartService)
    {
        _shoppingCartService = shoppingCartService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetCart()
    {
        var (cartId, userId) = GetCartIdentifiers();
        var cart = await _shoppingCartService.GetCartAsync(cartId, userId);
        if (cart == null)
        {
            return Ok(new ShoppingCartDto()); // Return empty cart
        }
        return Ok(cart);
    }

    [HttpPost("items")]
    [AllowAnonymous]
    public async Task<IActionResult> AddItemToCart(AddCartItemDto itemDto)
    {
        var (cartId, userId) = GetCartIdentifiers();
        var cart = await _shoppingCartService.AddItemToCartAsync(cartId, userId, itemDto);

        if (cart == null)
        {
            return BadRequest("Could not add item to cart. Product may be out of stock or invalid.");
        }

        // If it was an anonymous user and a new cart was created, set the cookie.
        if (!userId.HasValue && string.IsNullOrEmpty(cartId))
        {
            SetCartIdCookie(cart.Id.ToString());
        }

        return Ok(cart);
    }

    [HttpPut("items/{productId}")]
    [AllowAnonymous]
    public async Task<IActionResult> UpdateCartItem(Guid productId, UpdateCartItemDto itemDto)
    {
        var (cartId, userId) = GetCartIdentifiers();
        var cartKey = userId?.ToString() ?? cartId;

        if (string.IsNullOrEmpty(cartKey))
        {
            return BadRequest("Cart not found.");
        }
        var cart = await _shoppingCartService.UpdateCartItemAsync(cartKey, productId, itemDto);
        return cart != null ? Ok(cart) : NotFound();
    }

    [HttpDelete("items/{productId}")]
    [AllowAnonymous]
    public async Task<IActionResult> RemoveItemFromCart(Guid productId)
    {
        var (cartId, userId) = GetCartIdentifiers();
        var cartKey = userId?.ToString() ?? cartId;

        if (string.IsNullOrEmpty(cartKey))
        {
            // This is the likely cause of your error.
            // The anonymous cart ID cookie is missing from the request.
            var error = new ApiErrorResponse(400, "Cart identifier is missing. Ensure the cart cookie is sent correctly for anonymous users.");
            return new ObjectResult(error) { StatusCode = 400 };
        }

        var cart = await _shoppingCartService.RemoveItemFromCartAsync(cartKey, productId);

        if (cart == null)
        {
            var error = new ApiErrorResponse(404, "The requested cart or item could not be found.");
            return new ObjectResult(error) { StatusCode = 404 };
        }

        return Ok(cart);
    }

    private (string? cartId, Guid? userId) GetCartIdentifiers()
    {
        Guid? userId = null;
        if (User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdClaim, out var id))
            {
                userId = id;
            }
        }

        Request.Cookies.TryGetValue(CartIdCookieName, out var cartId);
        return (cartId, userId);
    }

    private void SetCartIdCookie(string cartId)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddDays(7),
            IsEssential = true, // Important for GDPR compliance
            SameSite = SameSiteMode.Lax
        };
        Response.Cookies.Append(CartIdCookieName, cartId, cookieOptions);
    }
}