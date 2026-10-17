# Pharmacy Items & Insurance — تقرير التنفيذ
**التاريخ:** 2026-05-04 | **المشروع:** SmartAndPro ERP (.NET 8)

---

## المشكلة التي كانت موجودة

صفحتان في الفرونت إند كانتا تُرجعان **500 Internal Server Error** عند كل طلب:

| الصفحة | السبب |
|--------|-------|
| صفحة البحث عن الأدوية واستعراض الدُفعات | `PharmacyItemController` يعتمد على `IPharmacyItemService` في الـ DI — لا يوجد Implementation مُسجَّل |
| صفحة التحقق من التأمين وإرسال الموافقة | `PharmacyInsuranceController` يعتمد على `IPharmacyInsuranceService` في الـ DI — لا يوجد Implementation مُسجَّل |

الـ Controllers كانت موجودة والـ Interfaces كانت معرَّفة، لكن:
- ملف `Services/Pharmacy/PharmacyServices.cs` كان **فارغاً تماماً**
- السطران المفقودان في `Program.cs` (تسجيل الـ Services في DI Container)

---

## ما الذي تم عمله

### 1 — تنفيذ `PharmacyItemService`

**الملف:** `SmartAndPro.ERP.Application/Services/Pharmacy/PharmacyServices.cs`

#### `SearchItemsAsync`
- يستعلم من `MainPurchaseClass` حيث `IsPharmacyItem = true` و `IsActive = true`
- يدعم الفلترة بـ `SearchTerm` على الحقول: `ReferenceCode`، `ArabicName`، `EnglishName`
- يدعم الفلترة بـ `StoreId` (Guid)
- يدعم `IsControlled = true` لإظهار الأدوية المضبوطة فقط (`ItemType == Medication`)
- يجلب المخزون الفعلي من `PharmacyBatch` (غير منتهي، كمية > 0) ويحسب:
  - `available` إذا الكمية > 10
  - `low-stock` إذا الكمية بين 1 و 10
  - `out-of-stock` إذا لا يوجد مخزون

#### `GetItemBatchesAsync`
- يجلب كل دُفعات صنف معين من `PharmacyBatch` مع فلترة المخزن
- يحسب `IsExpired` و `IsExpiringSoon` (الدُفعات التي ستنتهي خلال 3 أشهر)
- يُرتّب النتائج حسب تاريخ الانتهاء (الأقرب انتهاءً أولاً)
- `activeOnly = true` يُخفي الدُفعات المنتهية أو الكمية = 0

#### `CheckStockAsync`
- يجمع كل الكمية المتاحة من `PharmacyBatch` لصنف ومخزن محددين
- يُرجع ثلاثة أوضاع: `full` | `partial` | `unavailable`

#### حل مشكلة `int ItemId` مقابل `Guid`
الـ DTOs تستخدم `int ItemId` لكن الـ Database تستخدم `Guid` في `MainPurchaseClass.Id` والـ `PharmacyBatch` يربط الأصناف بـ `ItemCode` (string).

الحل: **FNV-1a hash** — دالة تحوّل `ItemCode` إلى `int` بطريقة ثابتة وقابلة للتكرار:
```
"MED-001" → hash → 123456789 (دائماً نفس الرقم)
```
هكذا الفرونت إند يحصل على `int ItemId` ويرسله في طلبات الدُفعات والمخزون، ويُحوَّل للبحث في `PharmacyBatch` بنفس المنطق.

---

### 2 — تنفيذ `PharmacyInsuranceService`

**الملف:** `SmartAndPro.ERP.Application/Services/Pharmacy/PharmacyServices.cs` (نفس الملف)

#### `CheckInsuranceCoverageAsync`
- يستعلم عن `PharmacyInsuranceCompany` بالـ `InsuranceCompanyId`
- إذا لم تُوجد الشركة: يُرجع `IsActive = false` مع سبب الرفض
- إذا وُجدت: يحسب من `DefaultCoveragePercentage`:
  - `TotalAmount` = مجموع (كمية × سعر) لكل صنف
  - `CoveredAmount` = TotalAmount × نسبة التغطية / 100
  - `PatientPayableAmount` = TotalAmount - CoveredAmount
  - تفاصيل كل صنف في `ItemsCoverage`

#### `SubmitInsuranceApprovalAsync`
- يجد `PharmacyPrescription` بالـ `PrescriptionId`
- إذا لم تُوجد الوصفة: يُرجع `Success = false`
- إذا وُجدت: يُحدِّث حقلين في الجدول:
  - `InsuranceApprovalNo` = رقم الموافقة المُرسَل
  - `InsuranceStatus` = `"approved"`
- يحفظ التغيير ويُرجع `Success = true`

---

### 3 — تسجيل الـ Services في `Program.cs`

**الملف:** `SmartAndPro.ERP.WebAPI/Program.cs`

أُضيف سطران بعد تسجيلات الـ Pharmacy الأخرى:

```csharp
builder.Services.AddScoped<SmartAndPro.ERP.Application.IServices.IPharmacyItemService,
    SmartAndPro.ERP.Application.Services.Pharmacy.PharmacyItemService>();
builder.Services.AddScoped<SmartAndPro.ERP.Application.IServices.IPharmacyInsuranceService,
    SmartAndPro.ERP.Application.Services.Pharmacy.PharmacyInsuranceService>();
```

---

## الـ Endpoints الجديدة (5 Endpoints)

| # | Method | Route | الوظيفة |
|---|--------|-------|---------|
| 1 | `GET` | `/api/v1/pharmacy/items/lookup` | بحث في كتالوج الأدوية |
| 2 | `GET` | `/api/v1/pharmacy/items/{itemId}/batches` | دُفعات صنف في مخزن |
| 3 | `POST` | `/api/v1/pharmacy/items/check-stock` | فحص توافر الكمية |
| 4 | `POST` | `/api/v1/pharmacy/insurance/verify` | التحقق من التغطية التأمينية |
| 5 | `POST` | `/api/v1/pharmacy/insurance/submit-approval` | إرسال رقم الموافقة |

كل الـ Endpoints تتطلب **JWT Bearer Token** (`[Authorize]`).

---

## الجداول المستخدمة

| الجدول | الاستخدام |
|--------|----------|
| `MainPurchaseClass` | كتالوج الأصناف (`IsPharmacyItem=true`) |
| `PharmacyBatch` | المخزون الفعلي مع الدُفعات وتواريخ الانتهاء |
| `PharmacyInsuranceCompany` | شركات التأمين مع نسبة التغطية الافتراضية |
| `PharmacyPrescription` | لتحديث رقم الموافقة التأمينية |

---

## ملف الـ Postman Collection

**الملف:** `docs/pharmacy-items-insurance.postman_collection.json`

لاستخدامه:
1. افتح Postman → Import → اختر الملف
2. غيّر متغير `base_url` لعنوان السيرفر
3. غيّر متغير `token` بـ JWT الخاص بك (من `/api/auth/login`)
4. غيّر متغير `store_id` بـ Guid المخزن المطلوب
5. شغّل الـ Requests

---

## ملاحظات

- `unitPrice` في نتائج البحث والدُفعات حالياً `0` — جدول `PharmacyBatch` لا يحتوي على سعر، يمكن إضافته لاحقاً عند ربط فاتورة الشراء
- `alternatives` في `CheckStockAsync` فارغة حالياً — يمكن تفعيلها لاحقاً بالبحث عن أصناف بديلة بنفس الـ Scientific Name
- نسبة التغطية التأمينية مُوحَّدة من `DefaultCoveragePercentage` — يمكن توسيعها لاحقاً لتدعم نسب مختلفة لكل صنف
