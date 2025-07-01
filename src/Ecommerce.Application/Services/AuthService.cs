using Ecommerce.Application.Dtos;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Interfaces.Services;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;

namespace Ecommerce.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, ITokenService tokenService)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<(UserDto? User, string? ErrorMessage)> RegisterAsync(UserRegistrationDto registrationDto)
    {
        var existingUser = await _unitOfWork.Users.GetByEmailAsync(registrationDto.Email);
        if (existingUser != null)
        {
            return (null, "Email is already registered.");
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = registrationDto.Name,
            Email = registrationDto.Email,
            PasswordHash = _passwordHasher.HashPassword(registrationDto.Password),
            Role = Role.User // Default role, can be changed based on logic
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.CompleteAsync();

        var userDto = new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role
        };

        return (userDto, null);
    }

    public async Task<(UserDto? User, TokenDto? Token, string? ErrorMessage)> LoginAsync(UserLoginDto loginDto)
    {
        var user = await _unitOfWork.Users.GetByEmailAsync(loginDto.Email);
        if (user == null || !_passwordHasher.VerifyPassword(user.PasswordHash, loginDto.Password))
        {
            return (null, null, "Invalid email or password.");
        }

        var tokenDto = _tokenService.GenerateToken(user);
        var userDto = new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role
        };

        return (userDto, tokenDto, null);
    }
}