using DigitalMarketplace.Application.DTOs.Products;
using DigitalMarketplace.Application.Interfaces;
using DigitalMarketplace.Domain.Entities;
using DigitalMarketplace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DigitalMarketplace.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;

    public ProductService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all products with their category name.
    /// AsNoTracking is used because this operation only reads data.
    /// </summary>
    public async Task<List<ProductResponse>> GetAllAsync()
    {
        return await _context.Products
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new ProductResponse
            {
                Id = x.Id,
                CategoryId = x.CategoryId,
                CategoryName = x.Category.Name,
                Name = x.Name,
                Slug = x.Slug,
                Description = x.Description,
                Price = x.Price,
                StockQuantity = x.StockQuantity,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves a product by ID.
    /// Throws KeyNotFoundException when the product does not exist.
    /// </summary>
    public async Task<ProductResponse> GetByIdAsync(long id)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ProductResponse
            {
                Id = x.Id,
                CategoryId = x.CategoryId,
                CategoryName = x.Category.Name,
                Name = x.Name,
                Slug = x.Slug,
                Description = x.Description,
                Price = x.Price,
                StockQuantity = x.StockQuantity,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (product == null)
        {
            throw new KeyNotFoundException(
                "Product does not exist.");
        }

        return product;
    }

    /// <summary>
    /// Creates a new product.
    /// The category must exist and product name/slug must be unique.
    /// </summary>
    public async Task<ProductResponse> CreateAsync(
        CreateProductRequest request)
    {
        // Verify that the selected category exists.
        var categoryExists = await _context.Categories
            .AnyAsync(x => x.Id == request.CategoryId);

        if (!categoryExists)
        {
            throw new KeyNotFoundException(
                "Category does not exist.");
        }

        // Check whether another product already uses the same name.
        var nameExists = await _context.Products
            .AnyAsync(x => x.Name == request.Name);

        if (nameExists)
        {
            throw new InvalidOperationException(
                "Product name is already registered.");
        }

        // Check whether another product already uses the same slug.
        var slugExists = await _context.Products
            .AnyAsync(x => x.Slug == request.Slug);

        if (slugExists)
        {
            throw new InvalidOperationException(
                "Product slug is already registered.");
        }

        var product = new Product
        {
            CategoryId = request.CategoryId,
            Name = request.Name,
            Slug = request.Slug,
            Description = request.Description,
            Price = request.Price,
            StockQuantity = request.StockQuantity,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Products.Add(product);

        await _context.SaveChangesAsync();

        // Load the category name for the response DTO.
        var categoryName = await _context.Categories
            .Where(x => x.Id == product.CategoryId)
            .Select(x => x.Name)
            .FirstAsync();

        return new ProductResponse
        {
            Id = product.Id,
            CategoryId = product.CategoryId,
            CategoryName = categoryName,
            Name = product.Name,
            Slug = product.Slug,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }

    /// <summary>
    /// Updates an existing product.
    /// The category must exist and name/slug must remain unique.
    /// </summary>
    public async Task<ProductResponse> UpdateAsync(
        long id,
        UpdateProductRequest request)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(x => x.Id == id);

        if (product == null)
        {
            throw new KeyNotFoundException(
                "Product does not exist.");
        }

        // Verify that the new category exists.
        var categoryExists = await _context.Categories
            .AnyAsync(x => x.Id == request.CategoryId);

        if (!categoryExists)
        {
            throw new KeyNotFoundException(
                "Category does not exist.");
        }

        // Exclude the current product when checking for duplicate names.
        var nameExists = await _context.Products
            .AnyAsync(x =>
                x.Id != id &&
                x.Name == request.Name);

        if (nameExists)
        {
            throw new InvalidOperationException(
                "Product name is already registered.");
        }

        // Exclude the current product when checking for duplicate slugs.
        var slugExists = await _context.Products
            .AnyAsync(x =>
                x.Id != id &&
                x.Slug == request.Slug);

        if (slugExists)
        {
            throw new InvalidOperationException(
                "Product slug is already registered.");
        }

        product.CategoryId = request.CategoryId;
        product.Name = request.Name;
        product.Slug = request.Slug;
        product.Description = request.Description;
        product.Price = request.Price;
        product.StockQuantity = request.StockQuantity;
        product.IsActive = request.IsActive;
        product.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Get the category name for the response DTO.
        var categoryName = await _context.Categories
            .Where(x => x.Id == product.CategoryId)
            .Select(x => x.Name)
            .FirstAsync();

        return new ProductResponse
        {
            Id = product.Id,
            CategoryId = product.CategoryId,
            CategoryName = categoryName,
            Name = product.Name,
            Slug = product.Slug,
            Description = product.Description,
            Price = product.Price,
            StockQuantity = product.StockQuantity,
            IsActive = product.IsActive,
            CreatedAt = product.CreatedAt,
            UpdatedAt = product.UpdatedAt
        };
    }

    /// <summary>
    /// Deletes a product.
    /// A product cannot be deleted if it has already been used
    /// in an order.
    /// </summary>
    public async Task DeleteAsync(long id)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(x => x.Id == id);

        if (product == null)
        {
            throw new KeyNotFoundException(
                "Product does not exist.");
        }

        // Prevent deleting a product that is referenced by order items.
        var hasOrderItems = await _context.OrderItems
            .AnyAsync(x => x.ProductId == id);

        if (hasOrderItems)
        {
            throw new InvalidOperationException(
                "Cannot delete product because it has been used in an order.");
        }

        _context.Products.Remove(product);

        await _context.SaveChangesAsync();
    }
}