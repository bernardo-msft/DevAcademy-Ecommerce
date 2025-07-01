using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ecommerce.API.Controllers;
using Ecommerce.Application.Interfaces.Services;
using Ecommerce.Application.Dtos;
using Microsoft.AspNetCore.Http;

namespace Ecommerce.API.Tests;

public class ProductsControllerTests
{
    private readonly Mock<IProductService> _mockProductService;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _mockProductService = new Mock<IProductService>();
        _controller = new ProductsController(_mockProductService.Object);
    }

    [Fact]
    public async Task GetAll_WhenProductsExist_ReturnsOkWithProducts()
    {
        // Arrange
        var products = new List<ProductDto>
        {
            new ProductDto { Id = Guid.NewGuid(), Name = "Laptop" },
            new ProductDto { Id = Guid.NewGuid(), Name = "Mouse" }
        };
        _mockProductService.Setup(s => s.GetAllProductsAsync(null)).ReturnsAsync(products);

        // Act
        var result = await _controller.GetAll(null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedProducts = Assert.IsAssignableFrom<IEnumerable<ProductDto>>(okResult.Value);
        Assert.Equal(2, new List<ProductDto>(returnedProducts).Count);
    }

    [Fact]
    public async Task Search_WithValidTerm_ReturnsOkWithProducts()
    {
        // Arrange
        var searchTerm = "Lap";
        var products = new List<ProductDto> { new ProductDto { Id = Guid.NewGuid(), Name = "Laptop" } };
        _mockProductService.Setup(s => s.SearchProductsAsync(searchTerm, null)).ReturnsAsync(products);

        // Act
        var result = await _controller.Search(searchTerm, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedProducts = Assert.IsAssignableFrom<IEnumerable<ProductDto>>(okResult.Value);
        Assert.Single(returnedProducts);
    }

    [Fact]
    public async Task Search_WithEmptyTerm_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.Search(" ", null);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetById_WhenProductExists_ReturnsOkWithProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var productDto = new ProductDto { Id = productId, Name = "Laptop" };
        _mockProductService.Setup(s => s.GetProductByIdAsync(productId)).ReturnsAsync(productDto);

        // Act
        var result = await _controller.GetById(productId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedProduct = Assert.IsType<ProductDto>(okResult.Value);
        Assert.Equal(productId, returnedProduct.Id);
    }

    [Fact]
    public async Task GetById_WhenProductDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        _mockProductService.Setup(s => s.GetProductByIdAsync(It.IsAny<Guid>())).ReturnsAsync((ProductDto?)null);

        // Act
        var result = await _controller.GetById(Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_WithValidData_ReturnsCreatedAtAction()
    {
        // Arrange
        var createDto = new CreateProductDto { Name = "New Product", CategoryId = Guid.NewGuid(), Price = 100, StockQuantity = 10 };
        var createdProduct = new ProductDto { Id = Guid.NewGuid(), Name = createDto.Name };
        _mockProductService.Setup(s => s.CreateProductAsync(createDto)).ReturnsAsync((createdProduct, null));

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(ProductsController.GetById), createdAtActionResult.ActionName);
        var returnedProduct = Assert.IsType<ProductDto>(createdAtActionResult.Value);
        Assert.Equal(createdProduct.Name, returnedProduct.Name);
    }

    [Fact]
    public async Task Create_WhenServiceReturnsError_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateProductDto { Name = "New Product" };
        var errorMessage = "Category not found.";
        _mockProductService.Setup(s => s.CreateProductAsync(createDto)).ReturnsAsync((null, errorMessage));

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task Update_WhenProductExists_ReturnsOkWithUpdatedProduct()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var updateDto = new UpdateProductDto { Name = "Updated Name" };
        var updatedProduct = new ProductDto { Id = productId, Name = updateDto.Name };
        _mockProductService.Setup(s => s.UpdateProductAsync(productId, updateDto)).ReturnsAsync((updatedProduct, null));

        // Act
        var result = await _controller.Update(productId, updateDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedProduct = Assert.IsType<ProductDto>(okResult.Value);
        Assert.Equal(updateDto.Name, returnedProduct.Name);
    }

    [Fact]
    public async Task Update_WhenProductDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var productId = Guid.NewGuid();
        var updateDto = new UpdateProductDto { Name = "Updated Name" };
        _mockProductService.Setup(s => s.UpdateProductAsync(productId, updateDto)).ReturnsAsync((null, null));

        // Act
        var result = await _controller.Update(productId, updateDto);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_WhenProductExists_ReturnsNoContent()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _mockProductService.Setup(s => s.DeleteProductAsync(productId)).ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(productId);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_WhenProductDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var productId = Guid.NewGuid();
        _mockProductService.Setup(s => s.DeleteProductAsync(productId)).ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(productId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}
