using Ecommerce.Application.Dtos;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Interfaces.Services;

namespace Ecommerce.Application.Services;

public class ReportService : IReportService
{
    private readonly IUnitOfWork _unitOfWork;

    public ReportService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<MonthlySalesDto>> GetMonthlySalesAsync(int year)
    {
        return await _unitOfWork.Orders.GetMonthlySalesAsync(year);
    }

    public async Task<IEnumerable<PopularProductDto>> GetMostPopularProductsAsync(int count)
    {
        return await _unitOfWork.OrderItems.GetMostPopularProductsAsync(count);
    }

    public async Task<IEnumerable<TopCustomerDto>> GetTopCustomersAsync(int count)
    {
        return await _unitOfWork.Orders.GetTopCustomersAsync(count);
    }
}