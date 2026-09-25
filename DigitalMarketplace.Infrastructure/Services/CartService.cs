using DigitalMarketplace.Application.DTOs.Carts;
using DigitalMarketplace.Application.Interfaces;
using DigitalMarketplace.Domain.Entities;
using DigitalMarketplace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DigitalMarketplace.Infrastructure.Services;

public class CartService : ICartService
{
    private readonly ApplicationDbContext _context;

    public CartService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves the current user's cart.
    /// If the user does not have a cart yet, a new empty cart is created.
    /// </summary>
    public async Task<CartResponse> GetMyCartAsync(long userId)
    {
        var cart = await GetOrCreateCartAsync(userId);

        return await BuildCartResponseAsync(cart.Id, userId);
    }

    /// <summary>
    /// Adds a product to the current user's cart.
    /// If the product already exists in the cart, its quantity is increased.
    /// </summary>
    public async Task<CartResponse> AddItemAsync(
        long userId,
        AddCartItemRequest request)
    {
        // Product must exist and be active before it can be added to the cart.
        var product = await _context.Products
            .FirstOrDefaultAsync(x =>
                x.Id == request.ProductId &&
                x.IsActive);

        if (product == null)
        {
            throw new KeyNotFoundException(
                "Product does not exist or is inactive.");
        }

        // The requested quantity cannot exceed the available stock.
        if (request.Quantity > product.StockQuantity)
        {
            throw new InvalidOperationException(
                "Requested quantity exceeds available stock.");
        }

        var cart = await GetOrCreateCartAsync(userId);

        // Check whether this product is already in the cart.
        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(x =>
                x.CartId == cart.Id &&
                x.ProductId == request.ProductId);

        if (cartItem == null)
        {
            // Create a new cart item when the product is not in the cart.
            cartItem = new CartItem
            {
                CartId = cart.Id,
                ProductId = request.ProductId,
                Quantity = request.Quantity,
                CreatedAt = DateTime.UtcNow
            };

            _context.CartItems.Add(cartItem);
        }
        else
        {
            // Increase the existing quantity instead of creating
            // a duplicate cart item for the same product.
            var newQuantity = cartItem.Quantity + request.Quantity;

            if (newQuantity > product.StockQuantity)
            {
                throw new InvalidOperationException(
                    "Requested quantity exceeds available stock.");
            }

            cartItem.Quantity = newQuantity;
            cartItem.UpdatedAt = DateTime.UtcNow;
        }

        cart.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await BuildCartResponseAsync(cart.Id, userId);
    }

    /// <summary>
    /// Updates the quantity of an existing cart item.
    /// </summary>
    public async Task<CartResponse> UpdateItemAsync(
    long userId,
    long cartItemId,
    UpdateCartItemRequest request)
    {
        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(x => x.Id == cartItemId);

        if (cartItem == null)
        {
            throw new KeyNotFoundException(
                "Cart item does not exist.");
        }

        var cart = await _context.Carts
            .FirstOrDefaultAsync(x =>
                x.Id == cartItem.CartId &&
                x.UserId == userId);

        if (cart == null)
        {
            throw new KeyNotFoundException(
                "Cart item does not belong to the current user.");
        }

        var product = await _context.Products
            .FirstOrDefaultAsync(x => x.Id == cartItem.ProductId);

        if (product == null || !product.IsActive)
        {
            throw new KeyNotFoundException(
                "Product does not exist or is inactive.");
        }

        if (request.Quantity > product.StockQuantity)
        {
            throw new InvalidOperationException(
                "Requested quantity exceeds available stock.");
        }

        cartItem.Quantity = request.Quantity;
        cartItem.UpdatedAt = DateTime.UtcNow;

        cart.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await BuildCartResponseAsync(cart.Id, userId);
    }

    /// <summary>
    /// Removes one item from the current user's cart.
    /// </summary>
    public async Task RemoveItemAsync(
    long userId,
    long cartItemId)
    {
        var cartItem = await _context.CartItems
            .FirstOrDefaultAsync(x => x.Id == cartItemId);

        if (cartItem == null)
        {
            throw new KeyNotFoundException(
                "Cart item does not exist.");
        }

        var cart = await _context.Carts
            .FirstOrDefaultAsync(x =>
                x.Id == cartItem.CartId &&
                x.UserId == userId);

        if (cart == null)
        {
            throw new KeyNotFoundException(
                "Cart item does not belong to the current user.");
        }

        _context.CartItems.Remove(cartItem);

        cart.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Removes all items from the current user's cart.
    /// The cart itself is kept for future use.
    /// </summary>
    public async Task ClearCartAsync(long userId)
    {
        var cart = await _context.Carts
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (cart == null)
        {
            return;
        }

        var cartItems = await _context.CartItems
            .Where(x => x.CartId == cart.Id)
            .ToListAsync();

        if (cartItems.Count == 0)
        {
            return;
        }

        _context.CartItems.RemoveRange(cartItems);

        cart.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Gets the existing cart or creates a new empty cart.
    /// </summary>
    private async Task<Cart> GetOrCreateCartAsync(long userId)
    {
        var cart = await _context.Carts
            .FirstOrDefaultAsync(x => x.UserId == userId);

        if (cart != null)
        {
            return cart;
        }

        cart = new Cart
        {
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Carts.Add(cart);

        await _context.SaveChangesAsync();

        return cart;
    }

    /// <summary>
    /// Builds the response returned to the API client.
    /// Product information is loaded through a projection instead of
    /// exposing EF Core entities directly.
    /// </summary>
    private async Task<CartResponse> BuildCartResponseAsync(
        long cartId,
        long userId)
    {
        var cart = await _context.Carts
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.Id == cartId &&
                x.UserId == userId);

        if (cart == null)
        {
            throw new KeyNotFoundException(
                "Cart does not exist.");
        }

        var items = await (
            from cartItem in _context.CartItems.AsNoTracking()
            join product in _context.Products.AsNoTracking()
                on cartItem.ProductId equals product.Id
            where cartItem.CartId == cartId
            select new CartItemResponse
            {
                Id = cartItem.Id,
                ProductId = product.Id,
                ProductName = product.Name,
                ProductSlug = product.Slug,
                UnitPrice = product.Price,
                Quantity = cartItem.Quantity,
                TotalPrice = product.Price * cartItem.Quantity
            }
        ).ToListAsync();

        return new CartResponse
        {
            Id = cart.Id,
            UserId = cart.UserId,
            TotalAmount = items.Sum(x => x.TotalPrice),
            Items = items,
            CreatedAt = cart.CreatedAt,
            UpdatedAt = cart.UpdatedAt
        };
    }
}