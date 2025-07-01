using Ecommerce.Application.Dtos;

namespace Ecommerce.Application.Interfaces.Repositories;

public interface ICartRepository
{
    Task<ShoppingCartDto?> GetByIdAsync(string cartId);
    Task<ShoppingCartDto> UpdateAsync(string cartId, ShoppingCartDto cart);
    Task<bool> DeleteAsync(string cartId);
    Task<bool> RenameAsync(string oldCartId, string newCartId);
}