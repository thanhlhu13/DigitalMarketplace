using System.ComponentModel.DataAnnotations;

namespace DigitalMarketplace.Application.DTOs.Products;

public class ProductQueryRequest
{
    public string? Search { get; set; }

    public int? CategoryId { get; set; }

    public bool? IsActive { get; set; }

    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
    public int PageSize { get; set; } = 10;

    public string? SortBy { get; set; } = "createdAt";

    public string? SortOrder { get; set; } = "desc";
}