using DigitalMarketplace.Application.Interfaces;
using DigitalMarketplace.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DigitalMarketplace.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services)
    {
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IWalletService, WalletService>();
        services.AddScoped<IDepositRequestService, DepositRequestService>();

        return services;
    }
}