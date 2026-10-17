using SmartAndPro.ERP.Application.Common.Dtos.Bills.Sale;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Bills.Sales;

namespace SmartAndPro.ERP.Application.Services.Bills.Sale
{
    public interface IBillOfSaleService : IServiceBase
        <
            BillOfSales,
            BillOfSalePostDto,
            BillOfSalePutDto,
            BillOfSaleResponseDto,
            BillOfSaleFilterDto
        >
    {
    }
}
