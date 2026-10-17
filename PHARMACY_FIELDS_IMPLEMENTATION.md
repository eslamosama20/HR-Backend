# Pharmacy Fields Implementation Summary

## Overview
تنفيذ التعديلات المطلوبة من الفرونت على الباك إند — إضافة حقول الصيدلية لـ Item Master و Purchase Invoice Items و Store.

---

## 1. Enums جديدة

### `SmartAndPro.ERP.Domains/Enums/ItemType.cs`
```csharp
public enum ItemType
{
    General = 0,
    Medication = 1,
    Service = 2
}
```

### `SmartAndPro.ERP.Domains/Enums/StoreCategory.cs`
```csharp
public enum StoreCategory
{
    General = 0,
    Pharmacy = 1,
    Warehouse = 2,
    Retail = 3
}
```

---

## 2. Entity Changes

### `MainPurchaseClass.cs` — حقول الصنف (Item Master)
| الحقل | النوع | الوصف |
|-------|-------|--------|
| `ItemType` | `ItemType` (enum) | نوع الصنف: General / Medication / Service — مخزون كـ string |
| `NeedsBatch` | `bool` | هل يحتاج رقم تشغيلة عند الإضافة؟ |
| `NeedsExpiry` | `bool` | هل يحتاج تاريخ صلاحية عند الإضافة؟ |
| `IsPharmacyItem` | `bool` | هل هو صنف صيدلاني؟ |
| `SfdaCode` | `string?` | كود هيئة الدواء (SFDA) — max 50 |

### `PurchaseItem.cs` — بنود فاتورة المشتريات
| الحقل | النوع | الوصف |
|-------|-------|--------|
| `BatchNo` | `string?` | رقم التشغيلة — max 100 |
| `ExpiryDate` | `DateTime?` | تاريخ الصلاحية |

### `StoreDetails.cs` — المخزن
| الحقل | النوع | الوصف |
|-------|-------|--------|
| `StoreCategory` | `StoreCategory` (enum) | تصنيف المخزن: General / Pharmacy / Warehouse / Retail — مخزون كـ string |

---

## 3. DTOs المُحدَّثة

### MainPurchaseClass
- `MainPurchaseClassPostDto` ✅ — أُضيف: ItemType, NeedsBatch, NeedsExpiry, IsPharmacyItem, SfdaCode
- `MainPurchaseClassPutDto` ✅ — أُضيف: ItemType, NeedsBatch, NeedsExpiry, IsPharmacyItem, SfdaCode
- `MainPurchaseClassResponseDto` ✅ — أُضيف: ItemType, NeedsBatch, NeedsExpiry, IsPharmacyItem, SfdaCode

### PurchaseItem
- `PurchaseItemPostDto` ✅ — أُضيف: BatchNo, ExpiryDate
- `PurchaseItemDto` (Response) ✅ — أُضيف: BatchNo, ExpiryDate

### StoreDetails
- `StoreDetailsPostDto` ✅ — أُضيف: StoreCategory
- `StoreDetailsPutDto` ✅ — أُضيف: StoreCategory
- `StoreDetailsResponseDto` ✅ — أُضيف: StoreCategory

---

## 4. Validation Logic

**الملف:** `PurchaseInvoiceService.cs`

تم إضافة validation في `AddAsync` و `EditAsync`:
- إذا كان `NeedsBatch = true` للصنف → `BatchNo` مطلوب وإلا يرجع خطأ عربي
- إذا كان `NeedsExpiry = true` للصنف → `ExpiryDate` مطلوب وإلا يرجع خطأ عربي
- يرمي `BusinessRuleViolationException` بأسماء الحقول ورسائل عربية

**مثال على الخطأ:**
```
"الصنف 'بنادول' يتطلب إدخال رقم التشغيلة (Batch No)."
"الصنف 'بنادول' يتطلب إدخال تاريخ الصلاحية (Expiry Date)."
```

---

## 5. EF Core Migration

**اسم الـ Migration:** `AddPharmacyFieldsToItemPurchaseStore`  
**الملف:** `SmartAndPro.ERP.Infrastructure/Migrations/20260418143118_AddPharmacyFieldsToItemPurchaseStore.cs`

### الأعمدة المُضافة في قاعدة البيانات:
| الجدول | العمود | النوع | القيمة الافتراضية |
|---------|--------|-------|------------------|
| `StoreDetails` | `StoreCategory` | nvarchar(max) | `'General'` |
| `PurchaseItems` | `BatchNo` | nvarchar(100) | NULL |
| `PurchaseItems` | `ExpiryDate` | datetime2 | NULL |
| `MainPurchaseClass` | `ItemType` | nvarchar(max) | `'General'` |
| `MainPurchaseClass` | `NeedsBatch` | bit | `0` |
| `MainPurchaseClass` | `NeedsExpiry` | bit | `0` |
| `MainPurchaseClass` | `IsPharmacyItem` | bit | `0` |
| `MainPurchaseClass` | `SfdaCode` | nvarchar(50) | NULL |

لتطبيق الـ migration على قاعدة البيانات:
```bash
dotnet ef database update --project SmartAndPro.ERP.Infrastructure --startup-project SmartAndPro.ERP.WebAPI
```

---

## 6. Build Status
✅ **0 Errors** — البيلد نظيف بعد كل التعديلات
