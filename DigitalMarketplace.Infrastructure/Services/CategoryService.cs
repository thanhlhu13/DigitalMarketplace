using DigitalMarketplace.Application.DTOs.Categories;
using DigitalMarketplace.Application.Interfaces;
using DigitalMarketplace.Domain.Entities;
using DigitalMarketplace.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DigitalMarketplace.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly ApplicationDbContext _context;

    public CategoryService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all categories.
    /// AsNoTracking is used because this operation only reads data
    /// and does not require EF Core to track the entities.
    /// </summary>
    public async Task<List<CategoryResponse>> GetAllAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new CategoryResponse
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug,
                Description = x.Description,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();
    }

    /// <summary>
    /// Retrieves a category by its ID.
    /// Throws KeyNotFoundException when the category does not exist,
    /// which is handled by the global exception middleware.
    /// </summary>
    public async Task<CategoryResponse> GetByIdAsync(int id)
    {
        var category = await _context.Categories
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new CategoryResponse
            {
                Id = x.Id,
                Name = x.Name,
                Slug = x.Slug,
                Description = x.Description,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync();

        if (category == null)
        {
            throw new KeyNotFoundException(
                "Category does not exist.");
        }

        return category;
    }

    /// <summary>
    /// Creates a new category.
    /// Category name and slug must be unique to avoid duplicate data.
    /// </summary>
    public async Task<CategoryResponse> CreateAsync(
        CreateCategoryRequest request)
    {
        // Check whether another category already uses the same name.
        var nameExists = await _context.Categories
            .AnyAsync(x => x.Name == request.Name);

        if (nameExists)
        {
            throw new InvalidOperationException(
                "Category name is already registered.");
        }

        // Check whether another category already uses the same slug.
        var slugExists = await _context.Categories
            .AnyAsync(x => x.Slug == request.Slug);

        if (slugExists)
        {
            throw new InvalidOperationException(
                "Category slug is already registered.");
        }

        // Create the entity from the validated request.
        var category = new Category
        {
            Name = request.Name,
            Slug = request.Slug,
            Description = request.Description,

            // New categories are active by default.
            IsActive = true,

            CreatedAt = DateTime.UtcNow
        };

        _context.Categories.Add(category);

        // SaveChangesAsync also generates the database identity ID.
        await _context.SaveChangesAsync();

        // Return a DTO instead of exposing the EF Core entity directly.
        return new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            Description = category.Description,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
    }

    /// <summary>
    /// Updates an existing category.
    /// The current category is excluded when checking for duplicate
    /// name and slug values.
    /// </summary>
    public async Task<CategoryResponse> UpdateAsync(
        int id,
        UpdateCategoryRequest request)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(x => x.Id == id);

        if (category == null)
        {
            throw new KeyNotFoundException(
                "Category does not exist.");
        }

        // Check for duplicate name while excluding the current category.
        var nameExists = await _context.Categories
            .AnyAsync(x =>
                x.Id != id &&
                x.Name == request.Name);

        if (nameExists)
        {
            throw new InvalidOperationException(
                "Category name is already registered.");
        }

        // Check for duplicate slug while excluding the current category.
        var slugExists = await _context.Categories
            .AnyAsync(x =>
                x.Id != id &&
                x.Slug == request.Slug);

        if (slugExists)
        {
            throw new InvalidOperationException(
                "Category slug is already registered.");
        }

        // Update the entity with the new values.
        category.Name = request.Name;
        category.Slug = request.Slug;
        category.Description = request.Description;
        category.IsActive = request.IsActive;
        category.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Return the updated data as a DTO.
        return new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            Slug = category.Slug,
            Description = category.Description,
            IsActive = category.IsActive,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
    }

    /// <summary>
    /// Deletes a category.
    /// A category cannot be deleted if it is currently used by
    /// one or more products.
    /// </summary>
    public async Task DeleteAsync(int id)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(x => x.Id == id);

        if (category == null)
        {
            throw new KeyNotFoundException(
                "Category does not exist.");
        }

        // Prevent deleting a category that is still referenced by products.
        // This protects the product-category relationship.
        var hasProducts = await _context.Products
            .AnyAsync(x => x.CategoryId == id);

        if (hasProducts)
        {
            throw new InvalidOperationException(
                "Cannot delete category because it has products.");
        }

        _context.Categories.Remove(category);

        await _context.SaveChangesAsync();
    }
}