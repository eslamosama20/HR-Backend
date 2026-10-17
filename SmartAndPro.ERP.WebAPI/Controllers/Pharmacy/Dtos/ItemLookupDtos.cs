using System.ComponentModel.DataAnnotations;

namespace SmartAndPro.ERP.WebAPI.Controllers.Pharmacy.Dtos
{
    // ================== Drug Lookup DTOs ==================
    
    public class ItemLookupQueryDto
    {
        public string? SearchTerm { get; set; } = string.Empty;
        
        [Required]
        public string StoreId { get; set; } = string.Empty;
        
        public bool IsControlled { get; set; } = false;
        
        public int MaxResults { get; set; } = 20;
    }

    public class ItemLookupDto
    {
        public int ItemId { get; set; }
        public string ItemCode { get; set; } = string.Empty;
        public string TradeName { get; set; } = string.Empty;
        public string? ScientificName { get; set; }
        public string? Strength { get; set; }
        public string? Manufacturer { get; set; }
        public string? DosageForm { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal RetailPrice { get; set; }
        public decimal WholesalePrice { get; set; }
        public decimal BulkPrice { get; set; }
        public decimal Stock { get; set; }
        public string AvailabilityStatus { get; set; } = string.Empty; // available|low-stock|out-of-stock
        public bool IsControlled { get; set; }
        public bool RequiresBatch { get; set; }
        public bool AllowNegative { get; set; }
        public string? Barcode { get; set; }
    }

    public class ItemLookupResponseDto
    {
        public List<ItemLookupDto> Items { get; set; } = new();
    }

    // ================== Batch Lookup DTOs ==================
    
    public class BatchLookupQueryDto
    {
        [Required]
        public int ItemId { get; set; }
        
        [Required]
        public string StoreId { get; set; } = string.Empty;
        
        public bool ActiveOnly { get; set; } = true;
    }

    public class BatchLookupDto
    {
        public string BatchId { get; set; } = string.Empty;
        public string BatchNo { get; set; } = string.Empty;
        public DateTime? ExpiryDate { get; set; }
        public decimal AvailableQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string? SupplierName { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public bool IsExpired { get; set; }
        public bool IsExpiringSoon { get; set; } // within 3 months
        public string Status { get; set; } = "active"; // active|expired|recalled
    }

    public class BatchLookupResponseDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public List<BatchLookupDto> Batches { get; set; } = new();
    }

    // ================== Stock Check DTOs ==================
    
    public class StockCheckRequest
    {
        [Required]
        public int ItemId { get; set; }
        
        [Required]
        public string StoreId { get; set; } = string.Empty;
        
        [Required]
        public decimal RequestedQty { get; set; }
    }

    public class StockCheckResponse
    {
        public bool IsAvailable { get; set; }
        public decimal AvailableQty { get; set; }
        public string AvailabilityStatus { get; set; } = string.Empty; // full|partial|unavailable
        public List<AlternativeItemDto> Alternatives { get; set; } = new();
    }

    public class AlternativeItemDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string ScientificName { get; set; } = string.Empty;
        public decimal AvailableQty { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
