using System.ComponentModel.DataAnnotations;

namespace DigitalMarketplace.Application.DTOs.Categories;

public class UpdateCategoryRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = null!;

    [Required]
    [MaxLength(150)]
    public string Slug { get; set; } = null!;

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsActive { get; set; }
}