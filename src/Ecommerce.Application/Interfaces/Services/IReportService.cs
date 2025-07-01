using Ecommerce.Application.Dtos;

namespace Ecommerce.Application.Interfaces.Services;

public interface IReportService
{
    Task<IEnumerable<MonthlySalesDto>> GetMonthlySalesAsync(int year);
    Task<IEnumerable<PopularProductDto>> GetMostPopularProductsAsync(int count);
    Task<IEnumerable<TopCustomerDto>> GetTopCustomersAsync(int count);
}