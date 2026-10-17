using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartAndPro.ERP.Application.IServices;
using SmartAndPro.ERP.Application.DTOs.Pharmacy;
using SmartAndPro.ERP.Domains.Entities.Pharmacy;
using SmartAndPro.ERP.Domains.Entities.Inventory.PurchaseClassesManagment;
using SmartAndPro.ERP.Domains.Entities.Inventory.PermitManagement;
using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.Infrastructure.Repositories;

namespace SmartAndPro.ERP.Application.Services.Pharmacy
{
    // ──────────────────────────────────────────────────────────────────────────
    // PharmacyItemService
    // ──────────────────────────────────────────────────────────────────────────
    public class PharmacyItemService : IPharmacyItemService
    {
        private readonly RepositoriesFactory _repos;
        private readonly ILogger<PharmacyItemService> _logger;

        public PharmacyItemService(RepositoriesFactory repos, ILogger<PharmacyItemService> logger)
        {
            _repos = repos;
            _logger = logger;
        }

        /// <summary>
        /// Generates a deterministic, stable int ID from an item code using FNV-1a hash.
        /// This allows the frontend to use int IDs without adding a DB column.
        /// </summary>
        private static int ItemCodeToIntId(string itemCode)
        {
            unchecked
            {
                uint hash = 2166136261u;
                foreach (char c in itemCode)
                {
                    hash ^= (uint)c;
                    hash *= 16777619u;
                }
                return (int)(hash & 0x7FFFFFFF);
            }
        }

        private DbContext GetDbContext() =>
            _repos.GetRepository<PharmacyBatch>().GetDbContext();

        public async Task<ItemLookupResponseDto> SearchItemsAsync(ItemLookupQueryDto query)
        {
            try
            {
                var db = GetDbContext();

                var term = (query.SearchTerm ?? "").Trim().ToLower();
                Guid? storeGuid = null;
                if (!string.IsNullOrWhiteSpace(query.StoreId) && Guid.TryParse(query.StoreId, out var parsedStoreGuid))
                {
                    storeGuid = parsedStoreGuid;
                }

                var response = new ItemLookupResponseDto();

                if (storeGuid.HasValue)
                {
                    // 1. Direct, fast query on store's permits and master items
                    var permitEntries = await (
                        from pe in db.Set<PermitEntry>().AsNoTracking()
                        join ip in db.Set<InventoryPermit>().AsNoTracking() on pe.InventoryPermitId equals ip.Id
                        join m in db.Set<MainPurchaseClass>().AsNoTracking() on pe.MainPurchaseClassId equals m.Id
                        where pe.IsActive && ip.IsActive && ip.StoreDetailsId == storeGuid.Value && m.IsActive
                        select new
                        {
                            pe.MainPurchaseClassId,
                            m.ReferenceCode,
                            m.ArabicName,
                            m.EnglishName,
                            m.IsControlled,
                            m.NeedsBatch,
                            pe.BatchNo,
                            pe.ExpiryDate,
                            pe.Quantity,
                            pe.RetailPrice,
                            pe.WholesalePrice,
                            pe.BulkPrice,
                            ip.PermitType,
                            pe.CreatedAt
                        }
                    ).ToListAsync();

                    // Group by MainPurchaseClassId and BatchNo to display each batch as a distinct selectable row
                    var groupedItems = permitEntries
                        .GroupBy(x => new { x.MainPurchaseClassId, BatchNo = string.IsNullOrWhiteSpace(x.BatchNo) ? "1" : x.BatchNo.Trim() })
                        .Select(g =>
                        {
                            var first = g.First();
                            var latestRetailEntry = g.Where(x => x.RetailPrice > 0).OrderByDescending(x => x.CreatedAt).FirstOrDefault();
                            var latestWholesaleEntry = g.Where(x => x.WholesalePrice > 0).OrderByDescending(x => x.CreatedAt).FirstOrDefault();
                            var latestBulkEntry = g.Where(x => x.BulkPrice > 0).OrderByDescending(x => x.CreatedAt).FirstOrDefault();

                            var retail = latestRetailEntry?.RetailPrice ?? 0m;
                            var wholesale = latestWholesaleEntry?.WholesalePrice ?? 0m;
                            var bulk = latestBulkEntry?.BulkPrice ?? 0m;
                            var price = retail > 0 ? retail : (wholesale > 0 ? wholesale : bulk);

                            var latestBatchEntry = g.OrderByDescending(x => x.CreatedAt).FirstOrDefault();
                            var stock = g.Sum(x => x.PermitType == PermitType.Entry ? x.Quantity : -x.Quantity);

                            var batchNo = g.Key.BatchNo;
                            var expiry = latestBatchEntry?.ExpiryDate.HasValue == true
                                ? latestBatchEntry.ExpiryDate.Value.ToString("yyyy-MM-dd")
                                : (latestBatchEntry?.CreatedAt ?? DateTime.UtcNow).AddYears(2).ToString("yyyy-MM-dd");

                            return new
                            {
                                MainPurchaseClassId = g.Key.MainPurchaseClassId,
                                first.ReferenceCode,
                                first.ArabicName,
                                first.EnglishName,
                                first.IsControlled,
                                first.NeedsBatch,
                                Stock = Math.Max(0m, stock),
                                UnitPrice = price,
                                RetailPrice = retail > 0 ? retail : (wholesale > 0 ? wholesale : bulk),
                                WholesalePrice = wholesale > 0 ? wholesale : (retail > 0 ? retail : bulk),
                                BulkPrice = bulk > 0 ? bulk : (wholesale > 0 ? wholesale : retail),
                                BatchNo = batchNo,
                                ExpiryDate = expiry
                            };
                        })
                        .Where(x => x.Stock > 0);

                    if (!string.IsNullOrWhiteSpace(term))
                    {
                        groupedItems = groupedItems.Where(x =>
                            (x.ReferenceCode != null && x.ReferenceCode.ToLower().Contains(term)) ||
                            (x.ArabicName != null && x.ArabicName.ToLower().Contains(term)) ||
                            (x.EnglishName != null && x.EnglishName.ToLower().Contains(term)));
                    }

                    foreach (var item in groupedItems.OrderByDescending(x => x.Stock).Take(query.MaxResults))
                    {
                        response.Items.Add(new ItemLookupDto
                        {
                            ItemId = ItemCodeToIntId(item.ReferenceCode),
                            ItemCode = item.ReferenceCode,
                            TradeName = item.EnglishName,
                            ScientificName = item.ArabicName,
                            UnitPrice = item.UnitPrice,
                            RetailPrice = item.RetailPrice,
                            WholesalePrice = item.WholesalePrice,
                            BulkPrice = item.BulkPrice,
                            Stock = item.Stock,
                            AvailabilityStatus = item.Stock > 10 ? "available" : "low-stock",
                            IsControlled = item.IsControlled,
                            RequiresBatch = item.NeedsBatch,
                            AllowNegative = false,
                            MainPurchaseClassId = item.MainPurchaseClassId,
                            DefaultBatchNo = item.BatchNo,
                            DefaultExpiryDate = item.ExpiryDate
                        });
                    }

                    return response;
                }
                else
                {
                    var classQ = db.Set<MainPurchaseClass>()
                        .AsNoTracking()
                        .Where(m => m.IsActive);

                    if (!string.IsNullOrWhiteSpace(term))
                    {
                        classQ = classQ.Where(m =>
                            m.ReferenceCode.ToLower().Contains(term) ||
                            m.ArabicName.ToLower().Contains(term) ||
                            m.EnglishName.ToLower().Contains(term));
                    }

                    var masterItems = await classQ.Take(query.MaxResults).ToListAsync();
                    foreach (var m in masterItems)
                    {
                        response.Items.Add(new ItemLookupDto
                        {
                            ItemId = ItemCodeToIntId(m.ReferenceCode),
                            ItemCode = m.ReferenceCode,
                            TradeName = m.EnglishName,
                            ScientificName = m.ArabicName,
                            UnitPrice = 0,
                            Stock = 0,
                            AvailabilityStatus = "out-of-stock",
                            IsControlled = m.IsControlled,
                            RequiresBatch = m.NeedsBatch,
                            AllowNegative = false,
                            MainPurchaseClassId = m.Id
                        });
                    }

                    return response;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching pharmacy items for term '{Term}'", query.SearchTerm);
                throw;
            }
        }

        public async Task<BatchLookupResponseDto> GetItemBatchesAsync(BatchLookupQueryDto query)
        {
            try
            {
                var db = GetDbContext();
                var utcNow = DateTime.UtcNow;
                var threeMonths = utcNow.AddMonths(3);

                Guid? storeGuid = null;
                if (!string.IsNullOrWhiteSpace(query.StoreId) && Guid.TryParse(query.StoreId, out var parsedStoreGuid))
                {
                    storeGuid = parsedStoreGuid;
                }

                var allMasters = await db.Set<MainPurchaseClass>()
                    .AsNoTracking()
                    .Where(m => m.IsActive)
                    .ToListAsync();
                var master = allMasters.FirstOrDefault(m => ItemCodeToIntId(m.ReferenceCode) == query.ItemId);

                if (master != null)
                {
                    var permitBatches = await (
                        from pe in db.Set<PermitEntry>().AsNoTracking()
                        join ip in db.Set<InventoryPermit>().AsNoTracking() on pe.InventoryPermitId equals ip.Id
                        where pe.IsActive && ip.IsActive && pe.MainPurchaseClassId == master.Id
                        where !storeGuid.HasValue || ip.StoreDetailsId == storeGuid.Value
                        select new
                        {
                            pe.Id,
                            pe.BatchNo,
                            pe.ExpiryDate,
                            pe.Quantity,
                            pe.RetailPrice,
                            ip.PermitType,
                            pe.CreatedAt
                        }
                    ).ToListAsync();

                    var grouped = permitBatches
                        .GroupBy(x => new
                        {
                            BatchNo = string.IsNullOrWhiteSpace(x.BatchNo) ? "1" : x.BatchNo.Trim(),
                            Expiry = x.ExpiryDate.HasValue ? x.ExpiryDate.Value.ToString("yyyy-MM-dd") : x.CreatedAt.AddYears(2).ToString("yyyy-MM-dd")
                        })
                        .Select(g =>
                        {
                            var expDate = DateTime.TryParse(g.Key.Expiry, out var dt) ? dt : utcNow.AddYears(2);
                            var availQty = g.Sum(x => x.PermitType == PermitType.Entry ? x.Quantity : -x.Quantity);
                            var price = g.Where(x => x.RetailPrice > 0).OrderByDescending(x => x.CreatedAt).Select(x => x.RetailPrice).FirstOrDefault();

                            return new BatchLookupDto
                            {
                                BatchId = g.Key.BatchNo,
                                BatchNo = g.Key.BatchNo,
                                ExpiryDate = expDate,
                                AvailableQuantity = Math.Max(0m, availQty),
                                UnitPrice = price,
                                IsExpired = expDate < utcNow,
                                IsExpiringSoon = expDate > utcNow && expDate < threeMonths,
                                Status = "active"
                            };
                        })
                        .Where(x => !query.ActiveOnly || (x.AvailableQuantity > 0 && !x.IsExpired))
                        .OrderBy(x => x.BatchNo)
                        .ToList();

                    return new BatchLookupResponseDto
                    {
                        ItemId = query.ItemId,
                        ItemName = master.EnglishName,
                        Batches = grouped
                    };
                }

                return new BatchLookupResponseDto
                {
                    ItemId = query.ItemId,
                    Batches = new List<BatchLookupDto>()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting item batches for item {ItemId}", query.ItemId);
                throw;
            }
        }

        public async Task<StockCheckResponse> CheckStockAsync(StockCheckRequest request)
        {
            try
            {
                var db = GetDbContext();
                var now = DateTime.UtcNow;

                Guid? storeGuid = null;
                if (!string.IsNullOrWhiteSpace(request.StoreId) && Guid.TryParse(request.StoreId, out var parsedStoreGuid))
                    storeGuid = parsedStoreGuid;

                // 1. Check PharmacyBatch
                var batchQ = db.Set<PharmacyBatch>()
                    .AsNoTracking()
                    .Where(b => b.IsActive && b.Quantity > 0 && b.ExpiryDate > now);

                if (storeGuid.HasValue)
                    batchQ = batchQ.Where(b => b.StoreDetailsId == storeGuid.Value);

                var allBatches = await batchQ.ToListAsync();
                var availableQty = allBatches
                    .Where(b => ItemCodeToIntId(b.ItemCode) == request.ItemId)
                    .Sum(b => b.Quantity);

                // 2. If availableQty == 0, check PermitEntry in store
                if (availableQty <= 0)
                {
                    var allMasters = await db.Set<MainPurchaseClass>()
                        .AsNoTracking()
                        .Where(m => m.IsActive)
                        .ToListAsync();
                    var master = allMasters.FirstOrDefault(m => ItemCodeToIntId(m.ReferenceCode) == request.ItemId);

                    if (master != null)
                    {
                        var permitQ = from pe in db.Set<PermitEntry>().AsNoTracking()
                                      join ip in db.Set<InventoryPermit>().AsNoTracking() on pe.InventoryPermitId equals ip.Id
                                      where pe.IsActive && ip.IsActive && pe.MainPurchaseClassId == master.Id
                                      select new { pe.Quantity, ip.PermitType, ip.StoreDetailsId };

                        if (storeGuid.HasValue)
                            permitQ = permitQ.Where(x => x.StoreDetailsId == storeGuid.Value);

                        var permits = await permitQ.ToListAsync();
                        var stock = permits.Sum(x => x.PermitType == PermitType.Entry ? x.Quantity : -x.Quantity);
                        availableQty = Math.Max(0m, stock);
                    }
                }

                string status;
                if (availableQty <= 0)
                    status = "unavailable";
                else if (availableQty >= request.RequestedQty)
                    status = "full";
                else
                    status = "partial";

                return new StockCheckResponse
                {
                    IsAvailable = availableQty > 0,
                    AvailableQty = availableQty,
                    AvailabilityStatus = status
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking stock for item ID {ItemId}", request.ItemId);
                throw;
            }
        }
    }

    // ──────────────────────────────────────────────────────────────────────────
    // PharmacyInsuranceService
    // ──────────────────────────────────────────────────────────────────────────
    public class PharmacyInsuranceService : IPharmacyInsuranceService
    {
        private readonly RepositoriesFactory _repos;
        private readonly ILogger<PharmacyInsuranceService> _logger;

        public PharmacyInsuranceService(RepositoriesFactory repos, ILogger<PharmacyInsuranceService> logger)
        {
            _repos = repos;
            _logger = logger;
        }

        private DbContext GetDbContext() =>
            _repos.GetRepository<PharmacyInsuranceCompany>().GetDbContext();

        public async Task<InsuranceCheckResponse> CheckInsuranceCoverageAsync(InsuranceCheckRequest request)
        {
            try
            {
                var db = GetDbContext();
                var company = await db.Set<PharmacyInsuranceCompany>()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == request.InsuranceCompanyId);

                if (company == null)
                {
                    return new InsuranceCheckResponse
                    {
                        IsActive = false,
                        RejectionReason = "Insurance company not found"
                    };
                }

                var coveragePct = company.DefaultCoveragePercentage;
                var patientSharePct = 100m - coveragePct;
                var totalAmount = request.Items.Sum(i => i.Quantity * i.UnitPrice);
                var coveredAmount = Math.Round(totalAmount * coveragePct / 100m, 2);
                var patientPayable = Math.Round(totalAmount - coveredAmount, 2);

                var itemCoverages = request.Items
                    .Select(i => new InsuranceItemCoverageDto
                    {
                        ItemId = i.ItemId,
                        IsCovered = true,
                        CoveragePercentage = coveragePct,
                        AmountCovered = Math.Round(i.Quantity * i.UnitPrice * coveragePct / 100m, 2)
                    }).ToList();

                return new InsuranceCheckResponse
                {
                    IsActive = true,
                    CoveragePercentage = coveragePct,
                    PatientSharePercentage = patientSharePct,
                    TotalAmount = totalAmount,
                    CoveredAmount = coveredAmount,
                    PatientPayableAmount = patientPayable,
                    ItemsCoverage = itemCoverages
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking insurance coverage for company {CompanyId}", request.InsuranceCompanyId);
                throw;
            }
        }

        public async Task<InsuranceApprovalResponse> SubmitInsuranceApprovalAsync(InsuranceApprovalRequest request)
        {
            try
            {
                var db = GetDbContext();
                var prescription = await db.Set<PharmacyPrescription>()
                    .FirstOrDefaultAsync(p => p.Id == request.PrescriptionId);

                if (prescription == null)
                {
                    return new InsuranceApprovalResponse
                    {
                        Success = false,
                        Message = "Prescription not found",
                        Status = "failed"
                    };
                }

                prescription.InsuranceApprovalNo = request.ApprovalNo;
                prescription.InsuranceStatus = "approved";
                await db.SaveChangesAsync();

                return new InsuranceApprovalResponse
                {
                    Success = true,
                    Message = "Approval submitted successfully",
                    Status = "approved"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting insurance approval for prescription {PrescriptionId}", request.PrescriptionId);
                throw;
            }
        }
    }
}
