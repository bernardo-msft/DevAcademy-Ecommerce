using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Ecommerce.API.Controllers;
using Ecommerce.Application.Interfaces.Services;
using Ecommerce.Application.Dtos;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Collections.Generic;

namespace Ecommerce.API.Tests;

public class ShoppingCartControllerTests
{
    private readonly Mock<IShoppingCartService> _mockShoppingCartService;
    private readonly ShoppingCartController _controller;
    private const string CartIdCookieName = "AnonymousCartId";

    public ShoppingCartControllerTests()
    {
        _mockShoppingCartService = new Mock<IShoppingCartService>();
        _controller = new ShoppingCartController(_mockShoppingCartService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    private void SetupAnonymousUser(string cartId)
    {
        var mockRequestCookies = new Mock<IRequestCookieCollection>();
        mockRequestCookies.Setup(c => c.TryGetValue(CartIdCookieName, out cartId!)).Returns(true);
        _controller.Request.Cookies = mockRequestCookies.Object;
    }

    private void SetupAuthenticatedUser(Guid userId)
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        }, "mock"));
        _controller.ControllerContext.HttpContext.User = user;
        _controller.Request.Cookies = new Mock<IRequestCookieCollection>().Object;
    }

    [Fact]
    public async Task GetCart_WhenCartExists_ReturnsOkWithCart()
    {
        // Arrange
        var cartId = Guid.NewGuid().ToString();
        var cartDto = new ShoppingCartDto { Id = Guid.Parse(cartId) };
        SetupAnonymousUser(cartId);
        _mockShoppingCartService.Setup(s => s.GetCartAsync(cartId, null)).ReturnsAsync(cartDto);

        // Act
        var result = await _controller.GetCart();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedCart = Assert.IsType<ShoppingCartDto>(okResult.Value);
        Assert.Equal(cartDto.Id, returnedCart.Id);
    }

    [Fact]
    public async Task GetCart_WhenCartDoesNotExist_ReturnsOkWithEmptyCart()
    {
        // Arrange
        SetupAnonymousUser(Guid.NewGuid().ToString());
        _mockShoppingCartService.Setup(s => s.GetCartAsync(It.IsAny<string>(), null)).ReturnsAsync((ShoppingCartDto?)null);

        // Act
        var result = await _controller.GetCart();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var cart = Assert.IsType<ShoppingCartDto>(okResult.Value);
        Assert.Empty(cart.Items);
    }

    [Fact]
    public async Task AddItemToCart_ForNewAnonymousUser_SetsCookieAndReturnsCart()
    {
        // Arrange
        var itemDto = new AddCartItemDto { ProductId = Guid.NewGuid(), Quantity = 1 };
        var newCartId = Guid.NewGuid();
        var cartDto = new ShoppingCartDto { Id = newCartId };
        
        var mockResponseCookies = new Mock<IResponseCookies>();
        var mockHttpResponse = new Mock<HttpResponse>();
        mockHttpResponse.SetupGet(r => r.Cookies).Returns(mockResponseCookies.Object);

        var mockHttpContext = new Mock<HttpContext>();
        mockHttpContext.SetupGet(c => c.Response).Returns(mockHttpResponse.Object);
        mockHttpContext.SetupGet(c => c.Request.Cookies).Returns(new Mock<IRequestCookieCollection>().Object); // No initial cookie
        mockHttpContext.SetupGet(c => c.User).Returns(new ClaimsPrincipal(new ClaimsIdentity())); // Set up a default user

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = mockHttpContext.Object
        };

        _mockShoppingCartService.Setup(s => s.AddItemToCartAsync(null, null, itemDto)).ReturnsAsync(cartDto);

        // Act
        var result = await _controller.AddItemToCart(itemDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<ShoppingCartDto>(okResult.Value);
        mockResponseCookies.Verify(c => c.Append(CartIdCookieName, newCartId.ToString(), It.IsAny<CookieOptions>()), Times.Once);
    }

    [Fact]
    public async Task AddItemToCart_WhenServiceFails_ReturnsBadRequest()
    {
        // Arrange
        var itemDto = new AddCartItemDto();
        SetupAnonymousUser(Guid.NewGuid().ToString());
        _mockShoppingCartService.Setup(s => s.AddItemToCartAsync(It.IsAny<string>(), null, itemDto)).ReturnsAsync((ShoppingCartDto?)null);

        // Act
        var result = await _controller.AddItemToCart(itemDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdateCartItem_WhenCartExists_ReturnsOkWithCart()
    {
        // Arrange
        var cartId = Guid.NewGuid().ToString();
        var productId = Guid.NewGuid();
        var itemDto = new UpdateCartItemDto { Quantity = 2 };
        var cartDto = new ShoppingCartDto { Id = Guid.Parse(cartId) };
        SetupAnonymousUser(cartId);
        _mockShoppingCartService.Setup(s => s.UpdateCartItemAsync(cartId, productId, itemDto)).ReturnsAsync(cartDto);

        // Act
        var result = await _controller.UpdateCartItem(productId, itemDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<ShoppingCartDto>(okResult.Value);
    }

    [Fact]
    public async Task UpdateCartItem_WhenCartOrItemNotFound_ReturnsNotFound()
    {
        // Arrange
        var cartId = Guid.NewGuid().ToString();
        var productId = Guid.NewGuid();
        var itemDto = new UpdateCartItemDto { Quantity = 2 };
        SetupAnonymousUser(cartId);
        _mockShoppingCartService.Setup(s => s.UpdateCartItemAsync(cartId, productId, itemDto)).ReturnsAsync((ShoppingCartDto?)null);

        // Act
        var result = await _controller.UpdateCartItem(productId, itemDto);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task RemoveItemFromCart_WhenSuccessful_ReturnsOkWithCart()
    {
        // Arrange
        var cartId = Guid.NewGuid().ToString();
        var productId = Guid.NewGuid();
        var cartDto = new ShoppingCartDto { Id = Guid.Parse(cartId) };
        SetupAnonymousUser(cartId);
        _mockShoppingCartService.Setup(s => s.RemoveItemFromCartAsync(cartId, productId)).ReturnsAsync(cartDto);

        // Act
        var result = await _controller.RemoveItemFromCart(productId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<ShoppingCartDto>(okResult.Value);
    }

    [Fact]
    public async Task RemoveItemFromCart_WhenCartIdentifierIsMissing_ReturnsBadRequest()
    {
        // Arrange
        _controller.Request.Cookies = new Mock<IRequestCookieCollection>().Object; // No cookie

        // Act
        var result = await _controller.RemoveItemFromCart(Guid.NewGuid());

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);
    }
}