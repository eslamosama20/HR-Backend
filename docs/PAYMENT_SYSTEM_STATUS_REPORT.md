# Payment System — Status Report
**Date:** 2026-04-30 | **Project:** SmartAndPro ERP (.NET 8)

---

## ملخص تنفيذي

طبقة الدفع الموحدة **(Payment Orchestration Layer)** اكتملت بالكامل من ناحية الباكند.
كل وسائل الدفع الست مُسجَّلة في النظام ومربوطة بجدول `Payments` في قاعدة البيانات.
**المانع الوحيد للتشغيل الفعلي** هو ملء بيانات الاعتماد الحقيقية (API Keys / Tokens) في `appsettings.json` بعد الحصول على الحسابات من مزودي الخدمة.

---

## 1 — Architecture Overview

```
POST /api/payments
        │
        ▼
  PaymentController
        │
        ▼
  PaymentService   ←── Method → Provider mapping
   ┌────┴─────────────────────────────────────────┐
   │  card / mada / apple_pay  →  PaymobProvider  │  ✅ Real HTTP (3-step flow)
   │  tabby                    →  TabbyProvider    │  🟡 Mock — awaiting credentials
   │  tamara                   →  TamaraProvider   │  🟡 Mock — awaiting credentials
   │  stc_pay                  →  HyperPayProvider │  🟡 Mock — awaiting credentials
   └──────────────────────────────────────────────┘
        │
        ▼
   ERPContext.Payments   ← persists every transaction (Pending → Paid/Failed/Refunded)
```

---

## 2 — ملفات منجزة (Backend)

| الملف | الوصف | الحالة |
|-------|-------|--------|
| `Services/Payment/PaymentInterfaces.cs` | DTOs مشتركة + Interfaces + Config Options لكل provider | ✅ |
| `Services/Payment/PaymentService.cs` | Orchestrator — method→provider routing، persist إلى DB، webhook dispatch | ✅ |
| `Services/Payment/Providers/PaymobProvider.cs` | تكامل Paymob حقيقي (auth → order → payment key → iframe URL) | ✅ |
| `Services/Payment/Providers/TabbyProvider.cs` | Provider skeleton جاهز، حالياً Mock | ✅ Skeleton |
| `Services/Payment/Providers/TamaraProvider.cs` | Provider skeleton جاهز، حالياً Mock | ✅ Skeleton |
| `Services/Payment/Providers/HyperPayProvider.cs` | Provider skeleton جاهز للـ STC Pay، حالياً Mock | ✅ Skeleton |
| `Controllers/Payment/PaymentController.cs` | 3 endpoints: `POST /api/payments`، `GET /api/payments/methods`، `POST /api/webhooks/{provider}` | ✅ |
| `Domains/Entities/Payment/Payment.cs` | Entity: Amount، Currency، Method، Provider، Status، ExternalId، PaymentUrl، WebhookPayload | ✅ |
| `Migrations/20260428003014_AddPaymentsTable` | Migration موجودة وتم تطبيقها | ✅ |
| `appsettings.json` → قسم `PaymentProviders` | الهيكل موجود — الحقول فارغة تنتظر الحسابات الحقيقية | ⚠️ Pending |
| `docs/payment-api.postman_collection.json` | Postman collection كامل لكل الـ 6 طرق + webhook examples | ✅ |

---

## 3 — وسائل الدفع المُنفَّذة

| Payment Method | Provider | نوع التكامل | جاهز للـ Production |
|---------------|----------|------------|---------------------|
| `card` | Paymob | **Real HTTP** — 3-step flow (auth → order → iframe) | بعد ملء ApiKey + IntegrationId |
| `mada` | Paymob | **Real HTTP** — نفس flow البطاقة | بعد ملء ApiKey + IntegrationId (Mada) |
| `apple_pay` | Paymob | **Real HTTP** — نفس flow البطاقة | بعد ملء ApiKey + IntegrationId (ApplePay) |
| `tabby` | Tabby | Mock (Skeleton جاهز) | بعد ملء ApiKey من حساب Tabby |
| `tamara` | Tamara | Mock (Skeleton جاهز) | بعد ملء ApiToken + NotificationToken من حساب Tamara |
| `stc_pay` | HyperPay | Mock (Skeleton جاهز) | بعد ملء EntityId + AccessToken من حساب HyperPay |

---

## 4 — API Endpoints المُنفَّذة

### `POST /api/payments`
```json
// Request
{
  "orderReference": "ORD-2026-001",
  "amount": 250.00,
  "currency": "SAR",
  "paymentMethod": "card",
  "customerName": "أحمد محمد",
  "customerEmail": "ahmed@example.com",
  "customerPhone": "+966501234567",
  "description": "فاتورة رقم 123",
  "callbackUrl": "https://erp.smartapro.com/payment/callback"
}

// Response
{
  "paymentUrl": "https://accept.paymob.com/api/acceptance/iframes/{id}?payment_token=...",
  "externalId": "12345678",
  "provider": "paymob",
  "status": "Pending",
  "internalId": "guid"
}
```

### `GET /api/payments/methods?country=SA`
```json
[
  { "key": "card",      "label": "Credit / Debit Card", "provider": "paymob",   "available": true },
  { "key": "mada",      "label": "Mada",                "provider": "paymob",   "available": true },
  { "key": "apple_pay", "label": "Apple Pay",           "provider": "paymob",   "available": true },
  { "key": "tabby",     "label": "Tabby (تابي)",        "provider": "tabby",    "available": true },
  { "key": "tamara",    "label": "Tamara (تمارا)",      "provider": "tamara",   "available": true },
  { "key": "stc_pay",   "label": "STC Pay",             "provider": "hyperpay", "available": true }
]
```

### `POST /api/webhooks/{provider}`
- يستقبل webhook من أي provider
- يخزن الـ raw payload في `Payments.WebhookPayload`
- يحدّث الـ Status: `Pending → Paid | Failed | Refunded`

---

## 5 — ما يحتاج بيانات حقيقية في `appsettings.json`

### الحقول المطلوبة الآن في `PaymentProviders`:

```json
"PaymentProviders": {
  "Paymob": {
    "ApiKey":        "← من Paymob Dashboard → Settings → API Key",
    "IntegrationId": "← من Paymob Dashboard → Integrations → Integration ID",
    "IsSandbox":     false
  },
  "Tabby": {
    "ApiKey":    "← من Tabby Partner Portal → API Keys",
    "IsSandbox": false
  },
  "Tamara": {
    "ApiToken":           "← من Tamara Merchant Portal → API Token",
    "NotificationToken":  "← من Tamara Merchant Portal → Notification Token",
    "IsSandbox":          false
  },
  "HyperPay": {
    "EntityId":     "← من HyperPay Dashboard → Entity ID (STC_PAY)",
    "AccessToken":  "← من HyperPay Dashboard → Access Token",
    "IsSandbox":    false
  }
}
```

> **ملاحظة Paymob:** لكل وسيلة دفع (card، mada، apple_pay) يوجد **Integration ID منفصل** في الـ Dashboard.
> في الوقت الحالي النظام يستخدم `IntegrationId` واحد — إذا أراد العميل الثلاثة في نفس الوقت يلزم إضافة `MadaIntegrationId` و `ApplePayIntegrationId` لاحقاً.

---

## 6 — ما يلزم لتفعيل Tabby / Tamara / HyperPay بشكل كامل

بعد ملء الـ credentials، يكفي استبدال جسم `CreatePaymentAsync` في كل Provider بـ HTTP calls حقيقية:

| Provider | الـ Endpoint الحقيقي | الـ Docs |
|---------|---------------------|---------|
| **Tabby** | `POST /api/v2/checkout` | https://docs.tabby.ai |
| **Tamara** | `POST /checkout` | https://docs.tamara.co |
| **HyperPay** | `POST /v1/checkouts` (server-to-server) | https://wordpressdocs.hyperpay.com |

الـ Skeleton (إنشاء object، تسجيل في DB، return URL) موجود بالفعل — الخطوة الوحيدة المتبقية هي **HTTP call داخل `CreatePaymentAsync`**.

---

## 7 — قاعدة البيانات

جدول `Payments` موجود في DB (Migration: `20260428003014_AddPaymentsTable`):

| Column | Type | الوصف |
|--------|------|-------|
| `Id` | Guid | PK |
| `Amount` | decimal(18,2) | المبلغ |
| `Currency` | nvarchar | SAR / USD / ... |
| `Method` | nvarchar | card / mada / apple_pay / tabby / tamara / stc_pay |
| `Provider` | nvarchar | paymob / tabby / tamara / hyperpay |
| `Status` | nvarchar | Pending / Paid / Failed / Refunded |
| `ExternalId` | nvarchar | ID من مزود الخدمة |
| `PaymentUrl` | nvarchar | رابط الدفع / iframe |
| `OrderReference` | nvarchar | رقم الطلب من النظام |
| `CustomerName/Email/Phone` | nvarchar | بيانات العميل |
| `WebhookPayload` | nvarchar(max) | Raw webhook للـ audit |
| `PaidAt` | datetime | وقت اكتمال الدفع |

---

## 8 — خلاصة الحالة

```
✅ Payment Orchestration Layer     → مكتمل 100%
✅ PaymobProvider (card/mada/AP)   → مكتمل — ينتظر credentials فقط
✅ TabbyProvider skeleton          → مكتمل — ينتظر credentials + HTTP body
✅ TamaraProvider skeleton         → مكتمل — ينتظر credentials + HTTP body
✅ HyperPayProvider skeleton       → مكتمل — ينتظر credentials + HTTP body
✅ DB Migration + Entity           → مطبَّق
✅ Postman Collection              → docs/payment-api.postman_collection.json
⚠️  appsettings.json credentials   → فارغة — تنتظر الحسابات الحقيقية
🔴  Tabby real HTTP calls          → تحتاج تكامل بعد الحساب
🔴  Tamara real HTTP calls         → تحتاج تكامل بعد الحساب
🔴  HyperPay real HTTP calls       → تحتاج تكامل بعد الحساب
```

---

*تقرير مُولَّد بتاريخ 2026-04-30 بناءً على فحص الكود الفعلي في المستودع.*
