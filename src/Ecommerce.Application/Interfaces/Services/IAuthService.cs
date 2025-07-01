using Ecommerce.Application.Dtos;

namespace Ecommerce.Application.Interfaces.Services;

public interface IAuthService
{
    Task<(UserDto? User, string? ErrorMessage)> RegisterAsync(UserRegistrationDto registrationDto);
    Task<(UserDto? User, TokenDto? Token, string? ErrorMessage)> LoginAsync(UserLoginDto loginDto);
}