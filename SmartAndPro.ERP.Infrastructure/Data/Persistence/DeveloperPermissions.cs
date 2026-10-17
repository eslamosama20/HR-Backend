using System.Collections.Generic;

namespace SmartAndPro.ERP.Infrastructure.Data.Persistence;

public static class DeveloperPermissions
{
    public const string SeedTemplates = "System.DevOnly.SeedTemplates";
    public const string ViewReservationCompanies = "System.DevOnly.ViewReservationCompanies";
    public const string ViewReservationSalesClasses = "System.DevOnly.ViewReservationSalesClasses";
    public const string SyncLegacyReservationCompanies = "System.DevOnly.SyncLegacyReservationCompanies";

    public static readonly Dictionary<string, string> ArabicNames = new()
    {
        { SeedTemplates, "توليد قوالب طباعة الحجوزات الافتراضية" },
        { ViewReservationCompanies, "عرض وتصفية شركات الحجز للمطور" },
        { ViewReservationSalesClasses, "عرض وتصفية أصناف الحجز للمطور" },
        { SyncLegacyReservationCompanies, "مزامنة شركات الحجز القديمة للمطور" }
    };

    public static readonly List<string> All = new()
    {
        SeedTemplates,
        ViewReservationCompanies,
        ViewReservationSalesClasses,
        SyncLegacyReservationCompanies
    };
}
