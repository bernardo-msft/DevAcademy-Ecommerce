namespace Ecommerce.Application.Dtos;

public class MonthlySalesDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalSales { get; set; }
}

public class PopularProductDto
{
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public int TotalQuantitySold { get; set; }
}

public class TopCustomerDto
{
    public Guid UserId { get; set; }
    public string? CustomerName { get; set; }
    public decimal TotalPurchaseAmount { get; set; }
}
