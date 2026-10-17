namespace SmartAndPro.ERP.Application.Common.Interfaces
{
    public interface IInvoiceService
    {
        public Task<string> GetLastInvoiceNo();
    }
}
