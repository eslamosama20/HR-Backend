namespace SmartAndPro.ERP.Application.Common.Dtos.Reports.TopProducts;

/// <summary>
/// DTO للمنتجات الأكثر مبيعاً أو طلباً
/// </summary>
public class TopProductDto
{
    /// <summary>
    /// معرف المنتج
    /// </summary>
    public Guid ProductId { get; set; }
    
    /// <summary>
    /// كود المنتج
    /// </summary>
    public string ProductCode { get; set; } = string.Empty;
    
    /// <summary>
    /// اسم المنتج بالعربي
    /// </summary>
    public string ProductNameAr { get; set; } = string.Empty;
    
    /// <summary>
    /// اسم المنتج بالإنجليزي
    /// </summary>
    public string ProductNameEn { get; set; } = string.Empty;
    
    /// <summary>
    /// اسم الوحدة
    /// </summary>
    public string UnitName { get; set; } = string.Empty;
    
    /// <summary>
    /// عدد مرات البيع/الطلب
    /// </summary>
    public int TransactionCount { get; set; }
    
    /// <summary>
    /// إجمالي الكمية المباعة/المطلوبة
    /// </summary>
    public decimal TotalQuantity { get; set; }
    
    /// <summary>
    /// إجمالي المبلغ
    /// </summary>
    public decimal TotalAmount { get; set; }
    
    /// <summary>
    /// تاريخ آخر عملية
    /// </summary>
    public DateTime? LastTransactionDate { get; set; }
    
    /// <summary>
    /// متوسط سعر الوحدة
    /// </summary>
    public decimal AverageUnitPrice { get; set; }
    
    /// <summary>
    /// الترتيب
    /// </summary>
    public int Rank { get; set; }
}

/// <summary>
/// فلاتر البحث في المنتجات الأكثر مبيعاً/طلباً
/// </summary>
public class TopProductsFilterDto
{
    /// <summary>
    /// تاريخ البداية
    /// </summary>
    public DateTime? FromDate { get; set; }
    
    /// <summary>
    /// تاريخ النهاية
    /// </summary>
    public DateTime? ToDate { get; set; }
    
    /// <summary>
    /// عدد المنتجات المطلوب عرضها (الأكثر)
    /// </summary>
    public int? Top { get; set; } = 100;
    
    /// <summary>
    /// البحث بالاسم أو الكود
    /// </summary>
    public string? SearchTerm { get; set; }
    
    /// <summary>
    /// معرف المخزن (للفلترة حسب المخزن)
    /// </summary>
    public Guid? StoreId { get; set; }
}

/// <summary>
/// DTO لمشاركة التقرير
/// </summary>
public class TopProductsShareRequestDto
{
    public string Method { get; set; } = "email"; // email or whatsapp
    public string Receiver { get; set; } = string.Empty;
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
