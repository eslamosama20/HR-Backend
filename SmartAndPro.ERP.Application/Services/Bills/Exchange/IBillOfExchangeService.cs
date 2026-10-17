using SmartAndPro.ERP.Application.Common.Dtos.Bills.Exchange;
using SmartAndPro.ERP.Application.Services.@base;
using BillOfExchanges = SmartAndPro.ERP.Domains.Entities.Bills.Exchange.BillOfExchange;

namespace SmartAndPro.ERP.Application.Services.Bills.Exchange
{
    public interface IBillOfExchangeService : IServiceBase
        <
        BillOfExchanges,
        BillOfExchangePostDto,
        BillOfExchangePutDto,
        BillOfExchangeResponseDto,
        BillOfExchangeFilterDto
        >
    {
    }
}
