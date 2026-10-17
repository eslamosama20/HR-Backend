namespace SmartAndPro.ERP.Domains.Enums;

public enum InvoicePrintType
{
    /// <summary>
    /// A4 Portrait orientation - الطباعة العادية A4 عمودي
    /// </summary>
    A4Portrait = 0,
    
    /// <summary>
    /// A4 Landscape orientation - الطباعة العادية A4 أفقي
    /// </summary>
    A4Landscape = 1,
    
    /// <summary>
    /// Thermal printer 80mm width - طباعة حرارية 80 ملم
    /// </summary>
    Thermal80mm = 2,
    
    /// <summary>
    /// Thermal printer 58mm width - طباعة حرارية 58 ملم
    /// </summary>
    Thermal58mm = 3,

    /// <summary>
    /// Server-side replica of the frontend Print 1 (format 1) A4 template - auto orientation (items > 8 -> landscape)
    /// نسخة سيرفر من طباعة 1 (الفرونت) — اتجاه تلقائي
    /// </summary>
    Format1Server = 4
}
