namespace SmartAndPro.ERP.Application.Common.Dtos.Bills.Accounts
{
    public class BillAccountsPutDto
    {
        public Guid Id { get; set; }
        public Guid SupplierId { get; set; }
        public string SupplierBankAccountNumber { get; set; }
        public string? Description { get; set; }
        public decimal NetPrice { get; set; }
        public decimal Tax { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
