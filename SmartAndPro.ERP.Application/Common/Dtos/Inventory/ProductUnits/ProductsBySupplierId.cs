namespace SmartAndPro.ERP.Application.Common.Dtos.Inventory.ProductUnits
{
    public class ProductsBySupplierIdDto
    {
        public Guid ProductId { get; set; }
        public string ProductNameAr { get; set; }
        public string ProductNameEn { get; set; }
        public decimal UnitPrice { get; set; }
        public Guid StoreId { get; set; }
        public string StoreNameAr { get; set; }
        public string StoreNameEn { get; set; }
    }
}
