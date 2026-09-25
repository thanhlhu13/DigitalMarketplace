using DigitalMarketplace.Application.DTOs.Products;

namespace DigitalMarketplace.Application.Interfaces;

public interface IProductService
{
    Task<List<ProductResponse>> GetAllAsync();

    Task<ProductResponse> GetByIdAsync(long id);

    Task<ProductResponse> CreateAsync(CreateProductRequest request);

    Task<ProductResponse> UpdateAsync(
        long id,
        UpdateProductRequest request);

    Task DeleteAsync(long id);
}