using DigitalMarketplace.Application.DTOs.Carts;

namespace DigitalMarketplace.Application.Interfaces;

public interface ICartService
{
    Task<CartResponse> GetMyCartAsync(long userId);

    Task<CartResponse> AddItemAsync(
        long userId,
        AddCartItemRequest request);

    Task<CartResponse> UpdateItemAsync(
        long userId,
        long cartItemId,
        UpdateCartItemRequest request);

    Task RemoveItemAsync(
        long userId,
        long cartItemId);

    Task ClearCartAsync(long userId);
}