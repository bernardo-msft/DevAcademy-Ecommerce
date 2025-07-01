using Ecommerce.Application.Dtos;
using Ecommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll([FromQuery] Guid? categoryId)
    {
        var products = await _productService.GetAllProductsAsync(categoryId);
        return Ok(products);
    }

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] Guid? categoryId)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest("Search term cannot be empty.");
        }
        var products = await _productService.SearchProductsAsync(q, categoryId);
        return Ok(products);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null) return NotFound();
        return Ok(product);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Create([FromForm] CreateProductDto createProductDto)
    {
        var (product, errorMessage) = await _productService.CreateProductAsync(createProductDto);
        if (errorMessage != null) return BadRequest(new { Message = errorMessage });
        return CreatedAtAction(nameof(GetById), new { id = product!.Id }, product);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdateProductDto updateProductDto)
    {
        var (product, errorMessage) = await _productService.UpdateProductAsync(id, updateProductDto);
        if (errorMessage != null) return BadRequest(new { Message = errorMessage });
        if (product == null) return NotFound();
        return Ok(product);
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _productService.DeleteProductAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }
}