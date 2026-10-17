namespace SmartAndPro.ERP.Domains.Enums;

/// <summary>
/// نوع التذكير بتاريخ الصلاحية
/// </summary>
public enum ReminderType
{
    /// <summary>
    /// تذكير يومي
    /// </summary>
    Daily = 1,
    
    /// <summary>
    /// تذكير أسبوعي
    /// </summary>
    Weekly = 7,
    
    /// <summary>
    /// تذكير شهري
    /// </summary>
    Monthly = 30,
    
    /// <summary>
    /// تذكير مخصص (عدد أيام يحدده المستخدم)
    /// </summary>
    Custom = 0
}
