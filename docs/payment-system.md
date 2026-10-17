# Payment System — Architecture & API Guide

## Architecture Overview

```
PaymentController
      │
      ▼
IPaymentService (PaymentService)
      │
      ├─ Method → Provider mapping
      │       card / mada / apple_pay  →  PaymobProvider   (real HTTP)
      │       tabby                    →  TabbyProvider     (mock)
      │       tamara                   →  TamaraProvider    (mock)
      │       stc_pay                  →  HyperPayProvider  (mock)
      │
      ├─ Persist Payment record (ERPContext → Payments table)
      │
      └─ IPaymentProvider
              ├── CreatePaymentAsync(OrderDto)  → PaymentResponse
              ├── HandleWebhookAsync(payload)
              └── RefundAsync(externalId, amount)
```

### File Structure
```
SmartAndPro.ERP.Domains/Entities/Payment/
  └── Payment.cs                          ← EF entity

SmartAndPro.ERP.WebAPI/
  ├── Controllers/Payment/
  │   └── PaymentController.cs            ← 3 endpoints
  └── Services/Payment/
      ├── PaymentInterfaces.cs            ← IPaymentProvider, IPaymentService, all DTOs, Options
      ├── PaymentService.cs               ← orchestration + DB persistence
      └── Providers/
          ├── PaymobProvider.cs           ← real integration (3-step Paymob flow)
          ├── TabbyProvider.cs            ← mock
          ├── TamaraProvider.cs           ← mock
          └── HyperPayProvider.cs         ← mock (STC Pay)
```

---

## Payment Method Mapping

| Method Key   | Provider   | Real / Mock |
|-------------|------------|-------------|
| `card`      | Paymob     | Real        |
| `mada`      | Paymob     | Real        |
| `apple_pay` | Paymob     | Real        |
| `tabby`     | Tabby      | Mock        |
| `tamara`    | Tamara     | Mock        |
| `stc_pay`   | HyperPay   | Mock        |

---

## Providers

### Paymob (card / mada / apple_pay)
Real 3-step integration:
1. **Auth** — `POST /auth/tokens` → get `authToken`
2. **Order** — `POST /ecommerce/orders` → get `orderId`
3. **Payment Key** — `POST /acceptance/payment_keys` → get iframe `paymentUrl`

**To go live:** Set `IsSandbox: false` and fill `ApiKey` + `IntegrationId` in `appsettings.json`.

### Tabby (BNPL — Buy Now Pay Later)
Mock implementation. Returns a fake checkout URL.  
**To go live:** Replace `CreatePaymentAsync` in `TabbyProvider.cs` with real Tabby v2 API call and fill `ApiKey`.

### Tamara (BNPL)
Mock implementation. Returns a fake order redirect URL.  
**To go live:** Replace `CreatePaymentAsync` in `TamaraProvider.cs` with real Tamara API call and fill `ApiToken`.

### HyperPay — STC Pay
Mock implementation. Returns a fake checkout ID.  
**To go live:** Replace `CreatePaymentAsync` in `HyperPayProvider.cs` with real HyperPay checkout API and fill `EntityId` + `AccessToken`.

---

## Database Model

Table: `Payments`

| Column           | Type             | Notes                             |
|-----------------|-----------------|-----------------------------------|
| `Id`            | `uniqueidentifier` | PK                              |
| `Amount`        | `decimal(18,2)`  |                                   |
| `Currency`      | `nvarchar(10)`   | default `SAR`                     |
| `Method`        | `nvarchar(50)`   | card / mada / stc_pay …           |
| `Provider`      | `nvarchar(50)`   | paymob / tabby / tamara / hyperpay|
| `Status`        | `nvarchar(30)`   | Pending / Paid / Failed / Refunded|
| `ExternalId`    | `nvarchar(200)`  | Provider's transaction ID         |
| `PaymentUrl`    | `nvarchar(1000)` | Checkout / iframe URL             |
| `OrderReference`| `nvarchar(100)`  | Your internal order ID            |
| `CustomerName`  | `nvarchar(200)`  |                                   |
| `CustomerEmail` | `nvarchar(200)`  |                                   |
| `CustomerPhone` | `nvarchar(50)`   |                                   |
| `WebhookPayload`| `nvarchar(max)`  | Raw webhook body (audit)          |
| `PaidAt`        | `datetime2`      | Set when status → Paid            |
| `CreatedAt`     | `datetime2`      |                                   |
| `IsActive`      | `bit`            | Soft delete                       |

---

## API Endpoints

### 1. Create Payment
```
POST /api/payments
Authorization: Bearer {token}

{
  "orderReference": "ORD-2026-001",
  "amount": 250.00,
  "currency": "SAR",
  "paymentMethod": "card",
  "customerName": "أحمد محمد",
  "customerEmail": "ahmed@example.com",
  "customerPhone": "+966500000000",
  "description": "طلب رقم ORD-2026-001",
  "callbackUrl": "https://yourapp.com/payment/callback"
}
```

**Response 200:**
```json
{
  "paymentUrl": "https://accept.paymob.com/api/acceptance/iframes/...",
  "externalId": "12345678",
  "provider": "paymob",
  "status": "Pending",
  "internalId": "guid"
}
```

---

### 2. Get Payment Methods
```
GET /api/payments/methods?country=SA
```

**Response 200:**
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

---

### 3. Webhook
```
POST /api/webhooks/{provider}
```
- No `Authorization` header — must be whitelisted in firewall by provider IP range.
- Always returns `200 OK` (providers retry on non-200).
- Auto-updates `Payment.Status` based on payload parsing.

| Provider  | Webhook Path                  |
|-----------|-------------------------------|
| Paymob    | `/api/webhooks/paymob`        |
| Tabby     | `/api/webhooks/tabby`         |
| Tamara    | `/api/webhooks/tamara`        |
| HyperPay  | `/api/webhooks/hyperpay`      |

---

## Configuration — appsettings.json

```json
"PaymentProviders": {
  "Paymob": {
    "ApiKey": "your_paymob_api_key",
    "IntegrationId": "your_integration_id",
    "IsSandbox": true
  },
  "Tabby": {
    "ApiKey": "your_tabby_api_key",
    "IsSandbox": true
  },
  "Tamara": {
    "ApiToken": "your_tamara_token",
    "NotificationToken": "your_notification_token",
    "IsSandbox": true
  },
  "HyperPay": {
    "EntityId": "your_entity_id",
    "AccessToken": "your_access_token",
    "IsSandbox": true
  }
}
```

### Switch Sandbox → Production
1. Set `"IsSandbox": false` for the desired provider.
2. Fill in the production credentials from your provider dashboard.
3. No code changes needed — the provider classes read `IsSandbox` automatically.

---

## How to Add a New Provider

1. Create `YourProvider.cs` in `Services/Payment/Providers/` implementing `IPaymentProvider`.
2. Add an options class `YourOptions` to `PaymentInterfaces.cs` and register it in `PaymentProvidersOptions`.
3. Add the method → provider entry in `PaymentService.MethodProviderMap`.
4. Add a `PaymentMethodDto` to `PaymentService.AllMethods`.
5. Register in `Program.cs`: `builder.Services.AddScoped<YourProvider>();`
6. Add the `ResolveProvider` case in `PaymentService`.

---

## Webhook Flow

```
Provider                    Your Server
   │                              │
   │  POST /api/webhooks/paymob   │
   │ ─────────────────────────▶  │
   │                              │  1. Read raw body
   │                              │  2. Log provider + IP
   │                              │  3. Call IPaymentProvider.HandleWebhookAsync()
   │                              │  4. Parse ExternalId + Status from payload
   │                              │  5. UPDATE Payments SET Status = 'Paid' WHERE ExternalId = ...
   │  ◀──────────── 200 OK ──────│
```
