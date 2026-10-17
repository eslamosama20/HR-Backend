using SmartAndPro.ERP.Domains.Entities.Invoice.Purchase;
using SmartAndPro.ERP.Domains.Entities.PurchaseQuotations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmartAndPro.ERP.Application.Common.Dtos.PurchaseQuotations;

namespace SmartAndPro.ERP.Application.Services.PurchaseQuotations
{
    public interface IPurchaseQuotationService
    {
        Task<OperationResponse<object>> UpdateAsync(int id, PurchaseQuotationPutDto dto);
        public  Task<OperationResponse<object>> GetAllFrontalAsync(int? pageNumber = null, int? pageSize = null);
        public Task<PurchaseQuotation> MakeFrontalAsync(int purchaseId);
        public  Task<string> GetLastInvoiceNo();
        public Task<OperationResponse<object>> GetAllAsync(int? pageNumber = null, int? pageSize = null);
        public Task<PurchaseQuotation> GetFirstAsync();
        public Task<PurchaseQuotation> GetByIdAsync(int id);
        public  Task<int> AddAsync(PurchaseQuotation invoice);
        //public  Task<int> UpdateTransaction(int? invoiceId, Guid? transactionId);
        //public  Task<int> CreateTransactionAndAddProductUnits(int id);
        public Task<int> EditAsync(PurchaseQuotation invoice, List<PurchaseQuotationItem> oldpurchaseItems);
        public Task<int> RemoveAsync(PurchaseQuotation quotation);

        public Task<OperationResponse<object>> ConvertToInvoice(int quotationId, Guid storeDetailsId);

        public Task<decimal> GetAverageItemPriceAsync(Guid mainPurchaseClassId, int days);
        public Task<decimal> GetLastItemPriceAsync(Guid mainPurchaseClassId);



    }
}
