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

public class ReportsControllerTests
{
    private readonly Mock<IReportService> _mockReportService;
    private readonly ReportsController _controller;

    public ReportsControllerTests()
    {
        _mockReportService = new Mock<IReportService>();
        _controller = new ReportsController(_mockReportService.Object);
    }

    [Fact]
    public async Task GetMonthlySales_WhenCalled_ReturnsOkWithSalesData()
    {
        // Arrange
        var year = 2023;
        var salesData = new List<MonthlySalesDto>
        {
            new MonthlySalesDto { Month = 1, TotalSales = 1000 },
            new MonthlySalesDto { Month = 2, TotalSales = 1500 }
        };
        _mockReportService.Setup(s => s.GetMonthlySalesAsync(year)).ReturnsAsync(salesData);

        // Act
        var result = await _controller.GetMonthlySales(year);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedData = Assert.IsAssignableFrom<IEnumerable<MonthlySalesDto>>(okResult.Value);
        Assert.Equal(2, new List<MonthlySalesDto>(returnedData).Count);
    }

    [Fact]
    public async Task GetPopularProducts_WhenCalled_ReturnsOkWithProducts()
    {
        // Arrange
        var count = 5;
        var popularProducts = new List<PopularProductDto>
        {
            new PopularProductDto { ProductId = Guid.NewGuid(), ProductName = "Laptop", TotalQuantitySold = 50 },
            new PopularProductDto { ProductId = Guid.NewGuid(), ProductName = "Mouse", TotalQuantitySold = 100 }
        };
        _mockReportService.Setup(s => s.GetMostPopularProductsAsync(count)).ReturnsAsync(popularProducts);

        // Act
        var result = await _controller.GetPopularProducts(count);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedProducts = Assert.IsAssignableFrom<IEnumerable<PopularProductDto>>(okResult.Value);
        Assert.Equal(2, new List<PopularProductDto>(returnedProducts).Count);
    }

    [Fact]
    public async Task GetTopCustomers_WhenCalled_ReturnsOkWithCustomers()
    {
        // Arrange
        var count = 3;
        var topCustomers = new List<TopCustomerDto>
        {
            new TopCustomerDto { UserId = Guid.NewGuid(), CustomerName = "John Doe", TotalPurchaseAmount = 5000 },
            new TopCustomerDto { UserId = Guid.NewGuid(), CustomerName = "Jane Smith", TotalPurchaseAmount = 4500 }
        };
        _mockReportService.Setup(s => s.GetTopCustomersAsync(count)).ReturnsAsync(topCustomers);

        // Act
        var result = await _controller.GetTopCustomers(count);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedCustomers = Assert.IsAssignableFrom<IEnumerable<TopCustomerDto>>(okResult.Value);
        Assert.Equal(2, new List<TopCustomerDto>(returnedCustomers).Count);
    }
}