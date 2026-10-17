# Pharmacy Backend Alignment — Summary of Changes

## Overview

These changes were made to align the backend with the frontend contract as described in the "Backend Alignment & Remaining Gaps Report". All changes are in the pharmacy module.

---

## 1. Entity Layer (`SmartAndPro.ERP.Domains`)

### `PharmacyEntities.cs`
- `PharmacyPrescriptionItem.ItemId`: `int` → `Guid?`

### `PharmacyAllocation.cs`
- `PharmacyAllocationLine.MainPurchaseClassId`: `int` → `Guid`
- `PharmacyPickup.PickupStatus` comment updated to include all valid values:
  - `pending | ready_for_pickup | picked_up | checked_out | cancelled`

---

## 2. WebAPI Layer (`SmartAndPro.ERP.WebAPI`)

### `Controllers/Pharmacy/Dtos/PrescriptionDtos.cs`
All `int` IDs changed to `Guid`:
| Property | Before | After |
|---|---|---|
| `PrescriptionListItemDto.Id` | `int` | `Guid` |
| `PrescriptionDetailsDto.Id` | `int` | `Guid` |
| `PatientDto.Id` | `int` | `Guid` |
| `DoctorDto.Id` | `int` | `Guid` |
| `PrescriptionItemDto.LineId` | `int` | `Guid` |
| `PrescriptionItemDto.ItemId` | `int` | `Guid?` |
| `InsuranceDto.InsuranceCompanyId` | `int?` | `Guid?` |
| `CreatePrescriptionRequest.PatientId` | `int` | `Guid` |
| `CreatePrescriptionRequest.DoctorId` | `int?` | `Guid?` |
| `CreatePrescriptionItemRequest.ItemId` | `int` | `Guid` |
| `CreatePrescriptionResponse.Id` | `int` | `Guid` |
| `UpdatePrescriptionRequest.PatientId` | `int` | `Guid` |
| `UpdatePrescriptionRequest.DoctorId` | `int?` | `Guid?` |
| `UpdatePrescriptionResponse.Id` | `int` | `Guid` |

### `Controllers/Pharmacy/Dtos/HighPriorityDtos.cs`
| Property | Before | After |
|---|---|---|
| `PatientListDto.Id` | `int` | `Guid` |
| `PatientDetailsDto.Id` | `int` | `Guid` |
| `DoctorListDto.Id` | `int` | `Guid` |
| `InsuranceItemDto.ItemId` | `int` | `Guid` |
| `InsuranceItemCoverageDto.ItemId` | `int` | `Guid` |
| `InsuranceApprovalRequest.PrescriptionId` | `int` | `Guid` |
| `UpdatePrescriptionStatusResponse.Id` | `int` | `Guid` |

### `Controllers/Pharmacy/Dtos/AdditionalDtos.cs`
| Property | Before | After |
|---|---|---|
| `MarkReadyResponse.Id` | `int` | `Guid` |
| `SendToPosResponse.Id` | `int` | `Guid` |
| `SendToPosResponse.SalesInvoiceId` | `int?` | `Guid?` |
| `CancelPrescriptionResponse.Id` | `int` | `Guid` |

### `Controllers/Pharmacy/PharmacyPrescriptionController.cs`
- Pagination parameter renamed: `size` → `pageSize` on all list endpoints:
  - `GET /prescriptions`
  - `GET /patients`
  - `GET /doctors`
  - `GET /insurance-companies`
- Added 3 new prescription item CRUD endpoints:
  - `POST api/v1/pharmacy/prescriptions/{id}/items` — add item to prescription
  - `PUT api/v1/pharmacy/prescriptions/{id}/items/{itemId}` — update item
  - `DELETE api/v1/pharmacy/prescriptions/{id}/items/{itemId}` — soft-delete item

### `Services/Pharmacy/PharmacyPrescriptionService.cs`
- Interface updated: `size` → `pageSize`, added `AddItemAsync`, `UpdateItemAsync`, `DeleteItemAsync`
- All pagination responses updated: `size` → `pageSize` in returned objects
- Item CRUD methods implemented (recalculates totals after each change)

### `Services/Pharmacy/PharmacyDispenseService.cs`
- Fixed invalid Guid cast: `(Guid)(object)line.ItemId` → `line.ItemId` (direct access after type fix)
- `AllocateAndDispense` response enriched — now includes `lines[]` array with per-line data:
  ```json
  {
    "lines": [
      {
        "prescriptionItemId": "guid",
        "itemId": "guid",
        "storeId": "guid",
        "batchNo": "string",
        "expiryDate": "datetime",
        "requestedQuantity": 0,
        "committedQuantity": 0,
        "availableQuantityAtCommit": 0
      }
    ]
  }
  ```

---

## 3. Application Layer (`SmartAndPro.ERP.Application`)

### `DTOs/Pharmacy/PrescriptionDtos.cs`
Same int → Guid changes as WebAPI DTOs (Application layer has its own separate DTO set):
- `PrescriptionListItemDto.Id`, `PrescriptionDetailsDto.Id`, `PatientDto.Id`, `DoctorDto.Id`
- `InsuranceDto.InsuranceCompanyId`, `PrescriptionItemDto.LineId`, `PrescriptionItemDto.ItemId`
- `CreatePrescriptionRequest.PatientId/DoctorId`, `CreatePrescriptionItemRequest.ItemId`
- `CreatePrescriptionResponse.Id`, `UpdatePrescriptionRequest.PatientId/DoctorId`, `UpdatePrescriptionResponse.Id`

### `DTOs/Pharmacy/HighPriorityDtos.cs`
- `InsuranceCheckRequest.InsuranceCompanyId`: `int` → `Guid`
- `InsuranceItemDto.ItemId`: `int` → `Guid`
- `InsuranceItemCoverageDto.ItemId`: `int` → `Guid`
- `InsuranceApprovalRequest.PrescriptionId`: `int` → `Guid`
- `PatientListDto.Id`, `PatientDetailsDto.Id`: `int` → `Guid`
- `PatientInsuranceDto.InsuranceCompanyId`: `int` → `Guid`
- `CreatePatientResponse.Id`: `int` → `Guid`
- `DoctorListDto.Id`: `int` → `Guid`
- `UpdatePrescriptionStatusResponse.Id`: `int` → `Guid`

### `DTOs/Pharmacy/AdditionalDtos.cs`
- `UpdateInsuranceRequest.InsuranceCompanyId`: `int?` → `Guid?`
- `InsuranceDataDto.InsuranceCompanyId`: `int?` → `Guid?`
- `MarkReadyResponse.Id`: `int` → `Guid`
- `SendToPosResponse.Id`: `int` → `Guid`
- `SendToPosResponse.SalesInvoiceId`: `int?` → `Guid?`
- `CancelPrescriptionResponse.Id`: `int` → `Guid`
- `ReadyPrescriptionItemDto.PrescriptionId`: `int` → `Guid`
- `InsuranceCompanyLookupDto.Id`: `int` → `Guid`

### `Services/Pharmacy/PharmacyPrescriptionService.cs`
Fixed all `prescription.SeqNo` → `prescription.Id` assignments in response mappings:
- `UpdatePrescriptionStatusResponse.Id`
- `MarkReadyResponse.Id`
- `SendToPosResponse.Id`
- `CancelPrescriptionResponse.Id`
- Insurance company lookup changed from `c.SeqNo == id` → `c.Id == id`
- Guard condition changed from `InsuranceCompanyId > 0` → `InsuranceCompanyId != Guid.Empty`

### `Services/Pharmacy/PharmacyLookupService.cs`
- `InsuranceCompanyLookupDto.Id = c.Id` (was `c.SeqNo`)

---

## 4. Infrastructure Layer (`SmartAndPro.ERP.Infrastructure`)

### Migration: `20260421102421_FixPharmacyIdTypesToGuid`
- Changes `PharmacyAllocationLine.MainPurchaseClassId` column from `int` → `uniqueidentifier`
- Changes `PharmacyPrescriptionItem.ItemId` column from `int` → `uniqueidentifier`

> ⚠️ **Warning**: This migration drops and recreates columns — existing data in these columns will be lost. Run on a non-production environment first.

### `ERPContextModelSnapshot.cs` — Conflict Resolution
Resolved 3 git merge conflicts:
| Conflict | Resolution |
|---|---|
| `ItemType` default value (`"General"` vs `"general"`) | Kept `"General"` — matches `ItemType.General.ToString()` enum value |
| `SfdaCode` max length (`50` vs `100`) | Kept `50` — matches entity's `HasMaxLength(50)` configuration |
| `MainPurchaseClassId` type (`int` vs `Guid`) | Kept `Guid` — our intentional type fix |

---

## 5. Build Status

```
Build succeeded.
0 Error(s)
```
