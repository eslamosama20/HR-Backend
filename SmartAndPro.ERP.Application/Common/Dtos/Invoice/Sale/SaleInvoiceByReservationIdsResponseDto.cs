namespace SmartAndPro.ERP.Application.Common.Dtos.Invoice.Sale;

public class SaleInvoiceByReservationIdsResponseDto
{
    public Dictionary<Guid, SaleInvoiceDto> Invoices { get; set; } = new();
}
