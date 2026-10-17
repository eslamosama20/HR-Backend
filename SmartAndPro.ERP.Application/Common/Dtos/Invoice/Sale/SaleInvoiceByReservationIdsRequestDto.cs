namespace SmartAndPro.ERP.Application.Common.Dtos.Invoice.Sale;

public class SaleInvoiceByReservationIdsRequestDto
{
    public List<Guid> ReservationIds { get; set; } = new();
}
