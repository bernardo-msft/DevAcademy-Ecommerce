using Ecommerce.Application.Dtos;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Interfaces.Repositories;
using Ecommerce.Application.Interfaces.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Application.Services;

public class ShoppingCartService : IShoppingCartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IUnitOfWork _unitOfWork; // Still needed for product lookups
    private readonly ILogger<ShoppingCartService> _logger;

    public ShoppingCartService(ICartRepository cartRepository, IUnitOfWork unitOfWork, ILogger<ShoppingCartService> logger)
    {
        _cartRepository = cartRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Determines the correct Redis key for a cart.
    /// Uses the User ID for authenticated users, or the anonymous cart ID for guests.
    /// </summary>
    private string GetCartKey(string? cartId, Guid? userId)
    {
        if (userId.HasValue)
        {
            return userId.Value.ToString();
        }
        // If cartId is null, a new one is generated for a new anonymous user.
        return cartId ?? Guid.NewGuid().ToString();
    }

    public async Task<ShoppingCartDto?> GetCartAsync(string? cartId, Guid? userId)
    {
        var cartKey = GetCartKey(cartId, userId);

        // Avoids a pointless lookup for a newly generated key that can't exist yet.
        if (string.IsNullOrEmpty(cartId) && !userId.HasValue)
        {
            return null;
        }
        return await _cartRepository.GetByIdAsync(cartKey);
    }

    public async Task<ShoppingCartDto?> AddItemToCartAsync(string? cartId, Guid? userId, AddCartItemDto itemDto)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(itemDto.ProductId);
        if (product == null || product.StockQuantity < itemDto.Quantity)
        {
            _logger.LogWarning("Product not found or not enough stock for ProductId: {ProductId}", itemDto.ProductId);
            return null;
        }

        var cartKey = GetCartKey(cartId, userId);
        var cart = await _cartRepository.GetByIdAsync(cartKey) ?? new ShoppingCartDto { Id = Guid.Parse(cartKey) };

        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == product.Id);
        if (existingItem != null)
        {
            existingItem.Quantity += itemDto.Quantity;
        }
        else
        {
            cart.Items.Add(new CartItemDto
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Price = product.Price,
                Quantity = itemDto.Quantity
            });
        }

        return await _cartRepository.UpdateAsync(cartKey, cart);
    }

    public async Task<ShoppingCartDto?> UpdateCartItemAsync(string cartId, Guid productId, UpdateCartItemDto itemDto)
    {
        var cart = await _cartRepository.GetByIdAsync(cartId);
        if (cart == null) return null;

        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item == null) return null;

        var product = await _unitOfWork.Products.GetByIdAsync(productId);
        if (product == null || product.StockQuantity < itemDto.Quantity) return null;

        if (itemDto.Quantity <= 0)
        {
            cart.Items.Remove(item);
        }
        else
        {
            item.Quantity = itemDto.Quantity;
        }

        return await _cartRepository.UpdateAsync(cartId, cart);
    }

    public async Task<ShoppingCartDto?> RemoveItemFromCartAsync(string cartId, Guid productId)
    {
        var cart = await _cartRepository.GetByIdAsync(cartId);
        if (cart == null) return null;

        var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            cart.Items.Remove(item);
            await _cartRepository.UpdateAsync(cartId, cart);
        }
        return cart;
    }

    public async Task MergeCartsOnLoginAsync(string anonymousCartId, Guid userId)
    {
        var userCartKey = userId.ToString();
        var userCart = await _cartRepository.GetByIdAsync(userCartKey);
        var anonymousCart = await _cartRepository.GetByIdAsync(anonymousCartId);

        if (anonymousCart == null) return; // Nothing to merge

        if (userCart == null)
        {
            // Easiest case: just rename the anonymous cart key to the user's ID.
            await _cartRepository.RenameAsync(anonymousCartId, userCartKey);
            _logger.LogInformation("Transferred anonymous cart {AnonymousCartId} to user cart {UserCartKey}", anonymousCartId, userCartKey);
        }
        else
        {
            // Merge items from the anonymous cart into the user's existing cart.
            foreach (var anonItem in anonymousCart.Items)
            {
                var existingItem = userCart.Items.FirstOrDefault(i => i.ProductId == anonItem.ProductId);
                if (existingItem != null)
                {
                    existingItem.Quantity += anonItem.Quantity; // Combine quantities
                }
                else
                {
                    userCart.Items.Add(anonItem);
                }
            }
            await _cartRepository.UpdateAsync(userCartKey, userCart);
            await _cartRepository.DeleteAsync(anonymousCartId); // Clean up the old anonymous cart
            _logger.LogInformation("Merged anonymous cart {AnonymousCartId} into user cart {UserCartKey}", anonymousCartId, userCartKey);
        }
    }
}