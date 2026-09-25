using DigitalMarketplace.Application.DTOs.Products;
using DigitalMarketplace.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DigitalMarketplace.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>
    /// Retrieves all products.
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<ProductResponse>>> GetAll()
    {
        var products = await _productService.GetAllAsync();

        return Ok(products);
    }

    /// <summary>
    /// Retrieves a product by ID.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{id:long}")]
    public async Task<ActionResult<ProductResponse>> GetById(long id)
    {
        var product = await _productService.GetByIdAsync(id);

        return Ok(product);
    }

    /// <summary>
    /// Creates a new product.
    /// Only Admin users are allowed to create products.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ProductResponse>> Create(
        CreateProductRequest request)
    {
        var product = await _productService.CreateAsync(request);

        return CreatedAtAction(
            nameof(GetById),
            new { id = product.Id },
            product);
    }

    /// <summary>
    /// Updates an existing product.
    /// Only Admin users are allowed to update products.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:long}")]
    public async Task<ActionResult<ProductResponse>> Update(
        long id,
        UpdateProductRequest request)
    {
        var product = await _productService.UpdateAsync(id, request);

        return Ok(product);
    }

    /// <summary>
    /// Deletes a product.
    /// Only Admin users are allowed to delete products.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        await _productService.DeleteAsync(id);

        return NoContent();
    }
}