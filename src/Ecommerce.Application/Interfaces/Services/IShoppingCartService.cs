using Ecommerce.Application.Dtos;
using System;
using System.Threading.Tasks;

namespace Ecommerce.Application.Interfaces.Services;

public interface IShoppingCartService
{
    Task<ShoppingCartDto?> GetCartAsync(string? cartId, Guid? userId);
    Task<ShoppingCartDto?> AddItemToCartAsync(string? cartId, Guid? userId, AddCartItemDto itemDto);
    Task<ShoppingCartDto?> UpdateCartItemAsync(string cartId, Guid productId, UpdateCartItemDto itemDto);
    Task<ShoppingCartDto?> RemoveItemFromCartAsync(string cartId, Guid productId);
    Task MergeCartsOnLoginAsync(string anonymousCartId, Guid userId);
}