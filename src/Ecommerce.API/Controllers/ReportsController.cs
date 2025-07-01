using Ecommerce.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpGet("sales/{year}")]
    public async Task<IActionResult> GetMonthlySales(int year)
    {
        var salesData = await _reportService.GetMonthlySalesAsync(year);
        return Ok(salesData);
    }

    [HttpGet("popular-products")]
    public async Task<IActionResult> GetPopularProducts([FromQuery] int count = 10)
    {
        var popularProducts = await _reportService.GetMostPopularProductsAsync(count);
        return Ok(popularProducts);
    }

    [HttpGet("top-customers")]
    public async Task<IActionResult> GetTopCustomers([FromQuery] int count = 10)
    {
        var topCustomers = await _reportService.GetTopCustomersAsync(count);
        return Ok(topCustomers);
    }
}