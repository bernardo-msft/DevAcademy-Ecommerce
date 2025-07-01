using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ecommerce.API.Controllers;
using Ecommerce.Application.Interfaces.Services;
using Ecommerce.Application.Dtos;

namespace Ecommerce.API.Tests;

public class CategoriesControllerTests
{
    private readonly Mock<ICategoryService> _mockCategoryService;
    private readonly CategoriesController _controller;

    public CategoriesControllerTests()
    {
        _mockCategoryService = new Mock<ICategoryService>();
        _controller = new CategoriesController(_mockCategoryService.Object);
    }

    [Fact]
    public async Task GetAll_WhenCategoriesExist_ReturnsOkWithCategories()
    {
        // Arrange
        var categories = new List<CategoryDto>
        {
            new CategoryDto { Id = Guid.NewGuid(), Name = "Electronics" },
            new CategoryDto { Id = Guid.NewGuid(), Name = "Books" }
        };
        _mockCategoryService.Setup(s => s.GetAllCategoriesAsync()).ReturnsAsync(categories);

        // Act
        var result = await _controller.GetAll();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedCategories = Assert.IsAssignableFrom<IEnumerable<CategoryDto>>(okResult.Value);
        Assert.Equal(2, new List<CategoryDto>(returnedCategories).Count);
    }

    [Fact]
    public async Task GetById_WhenCategoryExists_ReturnsOkWithCategory()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var categoryDto = new CategoryDto { Id = categoryId, Name = "Electronics" };
        _mockCategoryService.Setup(s => s.GetCategoryByIdAsync(categoryId)).ReturnsAsync(categoryDto);

        // Act
        var result = await _controller.GetById(categoryId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedCategory = Assert.IsType<CategoryDto>(okResult.Value);
        Assert.Equal(categoryId, returnedCategory.Id);
    }

    [Fact]
    public async Task GetById_WhenCategoryDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        _mockCategoryService.Setup(s => s.GetCategoryByIdAsync(It.IsAny<Guid>())).ReturnsAsync((CategoryDto?)null);

        // Act
        var result = await _controller.GetById(Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Create_WithValidData_ReturnsCreatedAtAction()
    {
        // Arrange
        var createDto = new CreateCategoryDto { Name = "New Category" };
        var createdCategory = new CategoryDto { Id = Guid.NewGuid(), Name = createDto.Name };
        _mockCategoryService.Setup(s => s.CreateCategoryAsync(createDto)).ReturnsAsync((createdCategory, null));

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(nameof(CategoriesController.GetById), createdAtActionResult.ActionName);
        var returnedCategory = Assert.IsType<CategoryDto>(createdAtActionResult.Value);
        Assert.Equal(createdCategory.Name, returnedCategory.Name);
    }

    [Fact]
    public async Task Create_WhenServiceReturnsError_ReturnsBadRequest()
    {
        // Arrange
        var createDto = new CreateCategoryDto { Name = "Existing Category" };
        var errorMessage = "Category with this name already exists.";
        _mockCategoryService.Setup(s => s.CreateCategoryAsync(createDto)).ReturnsAsync((null, errorMessage));

        // Act
        var result = await _controller.Create(createDto);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
    }

    [Fact]
    public async Task Update_WhenCategoryExists_ReturnsOkWithUpdatedCategory()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var updateDto = new UpdateCategoryDto { Name = "Updated Name" };
        var updatedCategory = new CategoryDto { Id = categoryId, Name = updateDto.Name };
        _mockCategoryService.Setup(s => s.UpdateCategoryAsync(categoryId, updateDto)).ReturnsAsync((updatedCategory, null));

        // Act
        var result = await _controller.Update(categoryId, updateDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedCategory = Assert.IsType<CategoryDto>(okResult.Value);
        Assert.Equal(updateDto.Name, returnedCategory.Name);
    }

    [Fact]
    public async Task Update_WhenCategoryDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var updateDto = new UpdateCategoryDto { Name = "Updated Name" };
        _mockCategoryService.Setup(s => s.UpdateCategoryAsync(categoryId, updateDto)).ReturnsAsync((null, null));

        // Act
        var result = await _controller.Update(categoryId, updateDto);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_WhenCategoryExists_ReturnsNoContent()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _mockCategoryService.Setup(s => s.DeleteCategoryAsync(categoryId)).ReturnsAsync(true);

        // Act
        var result = await _controller.Delete(categoryId);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_WhenCategoryDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _mockCategoryService.Setup(s => s.DeleteCategoryAsync(categoryId)).ReturnsAsync(false);

        // Act
        var result = await _controller.Delete(categoryId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
}