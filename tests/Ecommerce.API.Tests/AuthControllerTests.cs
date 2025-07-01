using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Ecommerce.API.Controllers;
using Ecommerce.Application.Interfaces.Services;
using Ecommerce.Application.Dtos;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Ecommerce.Domain.Enums;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Collections.Generic;
using System.Reflection;

namespace Ecommerce.API.Tests;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly Mock<ITokenBlacklistService> _tokenBlacklistServiceMock;
    private readonly Mock<IShoppingCartService> _shoppingCartServiceMock;
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _authServiceMock = new Mock<IAuthService>();
        _tokenBlacklistServiceMock = new Mock<ITokenBlacklistService>();
        _shoppingCartServiceMock = new Mock<IShoppingCartService>();
        _controller = new AuthController(
            _authServiceMock.Object,
            _tokenBlacklistServiceMock.Object,
            _shoppingCartServiceMock.Object
        );
    }

    [Fact]
    public async Task Register_WithValidData_ReturnsOkWithUser()
    {
        // Arrange
        var registrationDto = new UserRegistrationDto { Name = "Test User", Email = "test@example.com", Password = "Password123!" };
        var userDto = new UserDto { Id = Guid.NewGuid(), Name = "Test User", Email = "test@example.com", Role = Role.User };
        _authServiceMock.Setup(s => s.RegisterAsync(registrationDto)).ReturnsAsync((userDto, null));

        // Act
        var result = await _controller.Register(registrationDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedUser = Assert.IsType<UserDto>(okResult.Value);
        Assert.Equal(userDto.Email, returnedUser.Email);
    }

    [Fact]
    public async Task Register_WhenUserExists_ReturnsBadRequest()
    {
        // Arrange
        var registrationDto = new UserRegistrationDto { Name = "Test User", Email = "test@example.com", Password = "Password123!" };
        var errorMessage = "User with this email already exists.";
        _authServiceMock.Setup(s => s.RegisterAsync(registrationDto)).ReturnsAsync((null, errorMessage));

        // Act
        var result = await _controller.Register(registrationDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var value = new AnonymousObject(badRequestResult.Value);
        var message = value.GetPropertyValue<string>("Message");
        Assert.Equal(errorMessage, message);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsOkWithUserAndToken()
    {
        // Arrange
        var loginDto = new UserLoginDto { Email = "test@example.com", Password = "Password123!" };
        var userDto = new UserDto { Id = Guid.NewGuid(), Name = "Test User", Email = "test@example.com", Role = Role.User };
        var tokenDto = new TokenDto { AccessToken = "fake-jwt-token" };
        _authServiceMock.Setup(s => s.LoginAsync(loginDto)).ReturnsAsync((userDto, tokenDto, null));
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        _controller.Request.Cookies = Mock.Of<IRequestCookieCollection>();

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = new AnonymousObject(okResult.Value);
        var returnedUser = value.GetPropertyValue<UserDto>("User");
        var returnedToken = value.GetPropertyValue<TokenDto>("Token");

        Assert.NotNull(returnedUser);
        Assert.NotNull(returnedToken);
        Assert.Equal(userDto.Email, returnedUser.Email);
        Assert.Equal(tokenDto.AccessToken, returnedToken.AccessToken);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        // Arrange
        var loginDto = new UserLoginDto { Email = "test@example.com", Password = "WrongPassword!" };
        var errorMessage = "Invalid credentials.";
        _authServiceMock.Setup(s => s.LoginAsync(loginDto)).ReturnsAsync((null, null, errorMessage));

        // Act
        var result = await _controller.Login(loginDto);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var value = new AnonymousObject(unauthorizedResult.Value);
        var message = value.GetPropertyValue<string>("Message");
        Assert.Equal(errorMessage, message);
    }

    [Fact]
    public async Task Logout_WhenCalled_BlacklistsTokenAndReturnsOk()
    {
        // Arrange
        var jti = Guid.NewGuid().ToString();
        var exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds().ToString();
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, jti),
            new Claim(JwtRegisteredClaimNames.Exp, exp)
        };
        var identity = new ClaimsIdentity(claims);
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        // Act
        var result = await _controller.Logout();

        // Assert
        _tokenBlacklistServiceMock.Verify(s => s.BlacklistTokenAsync(jti, It.IsAny<DateTimeOffset>()), Times.Once);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = new AnonymousObject(okResult.Value);
        var message = value.GetPropertyValue<string>("Message");
        Assert.Equal("Logout successful. Please clear your token.", message);
    }

    [Fact]
    public void GetCurrentUser_WhenAuthenticated_ReturnsOkWithUserDetails()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userEmail = "test@example.com";
        var userName = "Test User";
        var userRole = "User"; // Corrected from "Customer"

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, userEmail),
            new Claim(ClaimTypes.Name, userName),
            new Claim(ClaimTypes.Role, userRole)
        };
        var identity = new ClaimsIdentity(claims);
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        // Act
        var result = _controller.GetCurrentUser(); // Note: Not awaited

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var value = new AnonymousObject(okResult.Value);
        Assert.Equal(userId, value.GetPropertyValue<string>("Id"));
        Assert.Equal(userEmail, value.GetPropertyValue<string>("Email"));
        Assert.Equal(userName, value.GetPropertyValue<string>("Name"));
        Assert.Equal(userRole, value.GetPropertyValue<string>("Role"));
    }
}

// Helper class to inspect anonymous objects
public class AnonymousObject
{
    private readonly object? _obj;
    public AnonymousObject(object? obj) { _obj = obj; }
    public T? GetPropertyValue<T>(string name)
    {
        var prop = _obj?.GetType().GetProperty(name);
        return (T?)prop?.GetValue(_obj, null);
    }
}