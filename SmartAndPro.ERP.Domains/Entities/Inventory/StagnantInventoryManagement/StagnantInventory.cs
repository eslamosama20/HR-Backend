using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartAndPro.ERP.Domains.Common.Models;
using SmartAndPro.ERP.Domains.Entities.Inventory.PurchaseClassesManagment;
using SmartAndPro.ERP.Domains.Entities.Inventory.SalesClassesManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.StoreManagement;

namespace SmartAndPro.ERP.Domains.Entities.Inventory.StagnantInventoryManagement;

/// <summary>
/// الرواكد - المنتجات الراكدة التي لم يتم تحريكها لفترة طويلة
/// </summary>
public class StagnantInventory : Entity, IEntityTypeConfiguration<StagnantInventory>
{
    /// <summary>
    /// صنف المشتريات (اختياري)
    /// </summary>
    public Guid? MainPurchaseClassId { get; set; }
    public MainPurchaseClass? MainPurchaseClass { get; set; }
    
    /// <summary>
    /// صنف المبيعات (اختياري)
    /// </summary>
    public Guid? MainSalesClassId { get; set; }
    public MainSalesClass? MainSalesClass { get; set; }
    
    /// <summary>
    /// الكمية الراكدة
    /// </summary>
    public decimal Quantity { get; set; }
    
    /// <summary>
    /// تاريخ آخر حركة على المنتج
    /// </summary>
    public DateTime? LastMovementDate { get; set; }
    
    /// <summary>
    /// تاريخ إضافته للرواكد
    /// </summary>
    public DateTime AddedToStagnantDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// سبب الإضافة للرواكد
    /// </summary>
    public StagnantReason Reason { get; set; }
    
    /// <summary>
    /// مدة الركود بالأشهر (للإضافة التلقائية)
    /// </summary>
    public int? StagnantPeriodMonths { get; set; }
    
    /// <summary>
    /// هل تم الإضافة تلقائياً أم يدوياً
    /// </summary>
    public bool IsAutoAdded { get; set; }
    
    /// <summary>
    /// ملاحظات إضافية
    /// </summary>
    public string? Notes { get; set; }
    
    /// <summary>
    /// من أضاف المنتج للرواكد
    /// </summary>
    public string? AddedBy { get; set; }
    
    /// <summary>
    /// المخزن
    /// </summary>
    public Guid? StoreDetailsId { get; set; }
    public StoreDetails? StoreDetails { get; set; }
    
    /// <summary>
    /// هل تم التخلص من المنتج
    /// </summary>
    public bool IsDisposed { get; set; }
    
    /// <summary>
    /// تاريخ التخلص
    /// </summary>
    public DateTime? DisposedDate { get; set; }
    
    /// <summary>
    /// طريقة التخلص
    /// </summary>
    public string? DisposalMethod { get; set; }

    public void Configure(EntityTypeBuilder<StagnantInventory> builder)
    {
        builder
            .ToTable("StagnantInventory")
            .HasKey(x => x.Id);
        
        builder.Property(x => x.Quantity)
            .IsRequired()
            .HasColumnType("decimal(18,4)");
        
        builder.Property(x => x.AddedToStagnantDate)
            .IsRequired();
        
        builder.Property(x => x.Notes)
            .HasMaxLength(1000);
        
        builder.Property(x => x.AddedBy)
            .HasMaxLength(200);
        
        builder.Property(x => x.DisposalMethod)
            .HasMaxLength(500);

        builder.HasOne(x => x.MainPurchaseClass)
            .WithMany()
            .HasForeignKey(x => x.MainPurchaseClassId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(x => x.MainSalesClass)
            .WithMany()
            .HasForeignKey(x => x.MainSalesClassId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(x => x.StoreDetails)
            .WithMany()
            .HasForeignKey(x => x.StoreDetailsId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

/// <summary>
/// سبب الإضافة للرواكد
/// </summary>
public enum StagnantReason
{
    /// <summary>
    /// يدوي - تم التحويل يدوياً من صفحة المخزون
    /// </summary>
    Manual = 0,
    
    /// <summary>
    /// تلقائي - لم يحصل على عمليات خلال المدة المحددة
    /// </summary>
    AutoByPeriod = 1,
    
    /// <summary>
    /// عند إذن الدخول - تم تحديد المدة عند إنشاء إذن الدخول
    /// </summary>
    OnEntryPermit = 2
}
