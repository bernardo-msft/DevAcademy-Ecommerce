using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Ecommerce.Application.Dtos;
using Ecommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ITokenBlacklistService _tokenBlacklistService;
    private readonly IShoppingCartService _shoppingCartService;
    private const string CartIdCookieName = "AnonymousCartId";

    public AuthController(IAuthService authService, ITokenBlacklistService tokenBlacklistService, IShoppingCartService shoppingCartService)
    {
        _authService = authService;
        _tokenBlacklistService = tokenBlacklistService;
        _shoppingCartService = shoppingCartService;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register(UserRegistrationDto registrationDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var (user, errorMessage) = await _authService.RegisterAsync(registrationDto);

        if (errorMessage != null)
        {
            return BadRequest(new { Message = errorMessage });
        }

        // It's common to return the created user or a success message.
        // For security, avoid returning the token directly on registration.
        // User should login to get a token.
        return Ok(user);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(UserLoginDto loginDto)
    {
        var (user, token, errorMessage) = await _authService.LoginAsync(loginDto);

        if (errorMessage != null)
        {
            return Unauthorized(new { Message = errorMessage });
        }

        // After successful login, check for an anonymous cart and merge it.
        if (Request.Cookies.TryGetValue(CartIdCookieName, out var anonymousCartId) && !string.IsNullOrEmpty(anonymousCartId))
        {
            await _shoppingCartService.MergeCartsOnLoginAsync(anonymousCartId, user!.Id);
            // Clear the cookie after merging
            Response.Cookies.Delete(CartIdCookieName);
        }

        return Ok(new { User = user, Token = token });
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout() // Make it async
    {
        var jti = User.FindFirstValue(JwtRegisteredClaimNames.Jti);
        var expClaim = User.FindFirstValue(JwtRegisteredClaimNames.Exp);

        if (!string.IsNullOrEmpty(jti) && !string.IsNullOrEmpty(expClaim) && long.TryParse(expClaim, out var expUnixTime))
        {
            var expiresAtUtc = DateTimeOffset.FromUnixTimeSeconds(expUnixTime);
            await _tokenBlacklistService.BlacklistTokenAsync(jti, expiresAtUtc);
        }
        // Even if blacklisting fails or claims are missing, proceed with client-side logout message.
        return Ok(new { Message = "Logout successful. Please clear your token." });
    }

    // Example of a protected endpoint
    [HttpGet("me")]
    [Authorize] // Requires any authenticated user
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userEmail = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        var userName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value;
        var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;


        if (userId == null)
        {
            return Unauthorized();
        }
        // In a real app, you might fetch fresh user data from the service/repository
        return Ok(new { Id = userId, Email = userEmail, Name = userName, Role = userRole });
    }

    [HttpGet("admin-only")]
    [Authorize(Policy = "AdminOnly")] // Requires Admin role
    public IActionResult AdminOnlyEndpoint()
    {
        return Ok(new { Message = "Welcome, Admin!" });
    }
}