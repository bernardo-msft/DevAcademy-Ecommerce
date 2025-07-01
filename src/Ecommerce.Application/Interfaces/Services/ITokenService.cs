using Ecommerce.Domain.Entities;
using Ecommerce.Application.Dtos;

namespace Ecommerce.Application.Interfaces.Services;

public interface ITokenService
{
    TokenDto GenerateToken(User user);
}