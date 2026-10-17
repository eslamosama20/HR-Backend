using System.Collections.Generic;

namespace SmartAndPro.ERP.WebAPI.Services.Cashier;

public interface ICashierRuntimeStore
{
    int GetOrCreateUserNumericId(Guid userGuid);
    Guid? TryResolveUserGuid(int userId);

    int GetOrCreateProductId(Guid mainSalesClassId, string? batchNo = null);
    Guid? TryResolveProductGuid(int productId);

    CashierSessionState? GetCurrentSession(Guid userGuid);
    CashierSessionState? GetSession(int sessionId);
    void SyncSessionIdCounter(int maxSessionId);
    void RestoreSession(CashierSessionState session);
    CashierSessionState OpenSession(Guid userGuid, string cashierUserName, decimal openingBalance, int? treasuryId, Guid? storeDetailsId, string? sessionNumber = null, int? stockAlertLimit = null);
    CashierSessionState? CloseSession(int sessionId, decimal actualBalance, string? notes, Guid? closedByUserGuid = null, string? closedByUserName = null);
    IReadOnlyList<CashierSessionState> GetSessions(int pageNumber, int pageSize);
    int GetSessionsCount();
    Guid? GetSessionStoreDetailsId(int sessionId);
    CashierSessionSummaryState BuildSessionSummary(int sessionId);

    void TrackInvoice(
        int invoiceId,
        int? sessionId,
        Guid userGuid,
        string userName,
        decimal totalAmount,
        DateTime createdAtUtc,
        Guid? storeDetailsId,
        IReadOnlyDictionary<Guid, decimal>? reservedByClass = null,
        int? tableId = null);
    IReadOnlyList<int> GetSessionInvoiceIds(int sessionId);
    IReadOnlyList<int> GetTableInvoiceIds(int tableId);
    CashierInvoiceState? GetInvoiceState(int invoiceId);
    IReadOnlyDictionary<Guid, decimal> GetReservedQuantitiesByClass(Guid storeDetailsId, IReadOnlyCollection<Guid> classIds, int? excludeInvoiceId = null);
    bool MarkInvoicePaid(int invoiceId);

    CashierPaymentState RecordPayment(
        int invoiceId,
        int paymentMethod,
        decimal paidAmount,
        decimal changeAmount,
        decimal cashAmount,
        decimal cardAmount,
        decimal creditAmount);

    CashierPaymentState? GetPayment(int invoiceId);
    bool MarkInvoiceRefunded(int invoiceId, string? reason);

    DailySalesReportState BuildDailySalesReport(DateTime date);
    CashierPerformanceReportState BuildPerformanceReport(Guid userGuid, DateTime fromUtc, DateTime toUtc);
}

public sealed class CashierRuntimeStore : ICashierRuntimeStore
{
    private readonly object _sync = new();
    private readonly Dictionary<int, CashierSessionState> _sessions = new();
    private readonly Dictionary<int, CashierInvoiceState> _invoices = new();
    private readonly Dictionary<int, CashierPaymentState> _payments = new();
    private readonly Dictionary<Guid, int> _userIdsByGuid = new();
    private readonly Dictionary<int, Guid> _userGuidsById = new();
    private readonly Dictionary<Guid, int> _productIdsByGuid = new();
    private readonly Dictionary<int, Guid> _productGuidsById = new();

    private int _nextSessionId;
    private int _nextUserId;
    private int _nextProductId = 1000;

    public int GetOrCreateUserNumericId(Guid userGuid)
    {
        lock (_sync)
        {
            if (_userIdsByGuid.TryGetValue(userGuid, out var numericId))
            {
                return numericId;
            }

            var next = ++_nextUserId;
            _userIdsByGuid[userGuid] = next;
            _userGuidsById[next] = userGuid;
            return next;
        }
    }

    public Guid? TryResolveUserGuid(int userId)
    {
        lock (_sync)
        {
            return _userGuidsById.TryGetValue(userId, out var guid) ? guid : null;
        }
    }

    private readonly Dictionary<string, int> _batchProductIds = new();

    public int GetOrCreateProductId(Guid mainSalesClassId, string? batchNo = null)
    {
        lock (_sync)
        {
            var key = string.IsNullOrWhiteSpace(batchNo) ? mainSalesClassId.ToString() : $"{mainSalesClassId}_{batchNo.Trim()}";
            if (_batchProductIds.TryGetValue(key, out var numericId))
            {
                return numericId;
            }

            if (string.IsNullOrWhiteSpace(batchNo) && _productIdsByGuid.TryGetValue(mainSalesClassId, out var existingNumericId))
            {
                _batchProductIds[key] = existingNumericId;
                return existingNumericId;
            }

            var next = ++_nextProductId;
            _batchProductIds[key] = next;
            _productGuidsById[next] = mainSalesClassId;
            if (!_productIdsByGuid.ContainsKey(mainSalesClassId))
            {
                _productIdsByGuid[mainSalesClassId] = next;
            }
            return next;
        }
    }

    public int GetOrCreateProductId(Guid mainSalesClassId) => GetOrCreateProductId(mainSalesClassId, null);

    public Guid? TryResolveProductGuid(int productId)
    {
        lock (_sync)
        {
            return _productGuidsById.TryGetValue(productId, out var guid) ? guid : null;
        }
    }

    public CashierSessionState? GetCurrentSession(Guid userGuid)
    {
        lock (_sync)
        {
            var current = _sessions.Values
                .Where(x => x.CashierUserGuid == userGuid && x.Status == 0)
                .OrderByDescending(x => x.OpeningDateUtc)
                .FirstOrDefault();

            return current is null ? null : CloneSession(current);
        }
    }

    public CashierSessionState? GetSession(int sessionId)
    {
        lock (_sync)
        {
            if (!_sessions.TryGetValue(sessionId, out var session))
            {
                return null;
            }

            return CloneSession(session);
        }
    }

    public void SyncSessionIdCounter(int maxSessionId)
    {
        lock (_sync)
        {
            if (maxSessionId > _nextSessionId)
            {
                _nextSessionId = maxSessionId;
            }
        }
    }

    public void RestoreSession(CashierSessionState session)
    {
        lock (_sync)
        {
            // Already in store — nothing to do
            if (_sessions.ContainsKey(session.Id))
                return;

            // Re-hydrate user GUID ↔ numeric ID mapping
            if (!_userIdsByGuid.ContainsKey(session.CashierUserGuid))
            {
                var numericId = ++_nextUserId;
                _userIdsByGuid[session.CashierUserGuid] = numericId;
                _userGuidsById[numericId] = session.CashierUserGuid;
            }

            // Keep session-ID counter in sync
            if (session.Id > _nextSessionId)
                _nextSessionId = session.Id;

            _sessions[session.Id] = CloneSession(session);
        }
    }

    public CashierSessionState OpenSession(Guid userGuid, string cashierUserName, decimal openingBalance, int? treasuryId, Guid? storeDetailsId, string? sessionNumber = null, int? stockAlertLimit = null)
    {
        lock (_sync)
        {
            var existingOpen = _sessions.Values
                .Where(x => x.CashierUserGuid == userGuid && x.Status == 0)
                .OrderByDescending(x => x.OpeningDateUtc)
                .FirstOrDefault();

            if (existingOpen is not null)
            {
                return CloneSession(existingOpen);
            }

            var numericUserId = GetOrCreateUserNumericId(userGuid);
            var id = ++_nextSessionId;
            var openedAt = DateTime.UtcNow;

            var session = new CashierSessionState
            {
                Id = id,
                SessionNumber = !string.IsNullOrWhiteSpace(sessionNumber) ? sessionNumber.Trim() : $"{id:D5}",
                CashierUserId = numericUserId,
                CashierUserGuid = userGuid,
                CashierUserName = string.IsNullOrWhiteSpace(cashierUserName) ? "Cashier" : cashierUserName.Trim(),
                OpeningDateUtc = openedAt,
                OpeningBalance = openingBalance,
                Status = 0,
                TreasuryId = treasuryId,
                StoreDetailsId = storeDetailsId.HasValue && storeDetailsId.Value != Guid.Empty ? storeDetailsId : null,
                TotalSales = 0m,
                TotalRefunds = 0m,
                TotalCash = 0m,
                TotalCard = 0m,
                TotalCredit = 0m,
                TransactionsCount = 0,
                StockAlertLimit = stockAlertLimit
            };

            _sessions[id] = session;
            return CloneSession(session);
        }
    }

    public CashierSessionState? CloseSession(int sessionId, decimal actualBalance, string? notes, Guid? closedByUserGuid = null, string? closedByUserName = null)
    {
        lock (_sync)
        {
            if (!_sessions.TryGetValue(sessionId, out var session))
            {
                return null;
            }

            if (session.Status == 1)
            {
                return CloneSession(session);
            }

            var expectedBalance = session.OpeningBalance + session.TotalCash;

            session.Status = 1;
            session.ClosingDateUtc = DateTime.UtcNow;
            session.ExpectedBalance = expectedBalance;
            session.ActualBalance = actualBalance;
            session.ClosingBalance = actualBalance;
            session.Difference = actualBalance - expectedBalance;
            session.Notes = notes?.Trim();
            session.ClosedByUserGuid = closedByUserGuid;
            session.ClosedByUserName = closedByUserName;

            return CloneSession(session);
        }
    }

    public IReadOnlyList<CashierSessionState> GetSessions(int pageNumber, int pageSize)
    {
        lock (_sync)
        {
            var safePage = Math.Max(pageNumber, 1);
            var safeSize = Math.Clamp(pageSize, 1, 500);

            return _sessions.Values
                .OrderByDescending(x => x.OpeningDateUtc)
                .Skip((safePage - 1) * safeSize)
                .Take(safeSize)
                .Select(CloneSession)
                .ToList();
        }
    }

    public int GetSessionsCount()
    {
        lock (_sync)
        {
            return _sessions.Count;
        }
    }

    public Guid? GetSessionStoreDetailsId(int sessionId)
    {
        lock (_sync)
        {
            if (!_sessions.TryGetValue(sessionId, out var session))
            {
                return null;
            }

            return session.StoreDetailsId;
        }
    }

    public CashierSessionSummaryState BuildSessionSummary(int sessionId)
    {
        lock (_sync)
        {
            if (!_sessions.TryGetValue(sessionId, out var session))
            {
                return new CashierSessionSummaryState
                {
                    SessionId = sessionId
                };
            }

            var expectedCash = session.OpeningBalance + session.TotalCash;
            var actualCash = session.ActualBalance ?? expectedCash;

            return new CashierSessionSummaryState
            {
                SessionId = session.Id,
                TotalTransactions = session.TransactionsCount,
                TotalSalesAmount = session.TotalSales,
                TotalRefundsAmount = session.TotalRefunds,
                CashAmount = session.TotalCash,
                CardAmount = session.TotalCard,
                CreditAmount = session.TotalCredit,
                ExpectedCash = expectedCash,
                ActualCash = actualCash,
                Difference = actualCash - expectedCash
            };
        }
    }

    public void TrackInvoice(
        int invoiceId,
        int? sessionId,
        Guid userGuid,
        string userName,
        decimal totalAmount,
        DateTime createdAtUtc,
        Guid? storeDetailsId,
        IReadOnlyDictionary<Guid, decimal>? reservedByClass = null,
        int? tableId = null)
    {
        lock (_sync)
        {
            var resolvedSessionId = sessionId;
            if (!resolvedSessionId.HasValue)
            {
                resolvedSessionId = _sessions.Values
                    .Where(x => x.CashierUserGuid == userGuid && x.Status == 0)
                    .OrderByDescending(x => x.OpeningDateUtc)
                    .Select(x => (int?)x.Id)
                    .FirstOrDefault();
            }

            var invoice = new CashierInvoiceState
            {
                InvoiceId = invoiceId,
                SessionId = resolvedSessionId,
                CashierUserGuid = userGuid,
                CashierUserName = userName,
                TotalAmount = totalAmount,
                CreatedAtUtc = createdAtUtc.ToUniversalTime(),
                StoreDetailsId = storeDetailsId.HasValue && storeDetailsId.Value != Guid.Empty ? storeDetailsId : null,
                TableId = tableId,
                ReservedByClass = reservedByClass is null
                    ? new Dictionary<Guid, decimal>()
                    : reservedByClass
                        .Where(x => x.Key != Guid.Empty && x.Value > 0m)
                        .GroupBy(x => x.Key)
                        .ToDictionary(group => group.Key, group => group.Sum(v => v.Value))
            };

            _invoices[invoiceId] = invoice;
        }
    }

    public IReadOnlyList<int> GetSessionInvoiceIds(int sessionId)
    {
        lock (_sync)
        {
            return _invoices.Values
                .Where(x => x.SessionId == sessionId)
                .OrderByDescending(x => x.CreatedAtUtc)
                .Select(x => x.InvoiceId)
                .ToList();
        }
    }

    public IReadOnlyList<int> GetTableInvoiceIds(int tableId)
    {
        lock (_sync)
        {
            return _invoices.Values
                .Where(x => x.TableId.HasValue && x.TableId.Value == tableId)
                .OrderByDescending(x => x.CreatedAtUtc)
                .Select(x => x.InvoiceId)
                .ToList();
        }
    }

    public CashierInvoiceState? GetInvoiceState(int invoiceId)
    {
        lock (_sync)
        {
            return _invoices.TryGetValue(invoiceId, out var invoice) ? CloneInvoice(invoice) : null;
        }
    }

    public IReadOnlyDictionary<Guid, decimal> GetReservedQuantitiesByClass(
        Guid storeDetailsId,
        IReadOnlyCollection<Guid> classIds,
        int? excludeInvoiceId = null)
    {
        lock (_sync)
        {
            if (classIds.Count == 0)
            {
                return new Dictionary<Guid, decimal>();
            }

            var classIdSet = classIds.Where(x => x != Guid.Empty).ToHashSet();
            var reserved = new Dictionary<Guid, decimal>();

            foreach (var invoice in _invoices.Values)
            {
                if (excludeInvoiceId.HasValue && invoice.InvoiceId == excludeInvoiceId.Value)
                {
                    continue;
                }

                if (invoice.IsRefunded || invoice.IsPaid)
                {
                    continue;
                }

                if (!invoice.StoreDetailsId.HasValue || invoice.StoreDetailsId.Value != storeDetailsId)
                {
                    continue;
                }

                if (invoice.ReservedByClass is null || invoice.ReservedByClass.Count == 0)
                {
                    continue;
                }

                foreach (var entry in invoice.ReservedByClass)
                {
                    if (!classIdSet.Contains(entry.Key) || entry.Value <= 0m)
                    {
                        continue;
                    }

                    if (reserved.TryGetValue(entry.Key, out var existing))
                    {
                        reserved[entry.Key] = existing + entry.Value;
                    }
                    else
                    {
                        reserved[entry.Key] = entry.Value;
                    }
                }
            }

            return reserved;
        }
    }

    public bool MarkInvoicePaid(int invoiceId)
    {
        lock (_sync)
        {
            if (!_invoices.TryGetValue(invoiceId, out var invoice))
            {
                return false;
            }

            invoice.IsPaid = true;
            return true;
        }
    }

    public CashierPaymentState RecordPayment(
        int invoiceId,
        int paymentMethod,
        decimal paidAmount,
        decimal changeAmount,
        decimal cashAmount,
        decimal cardAmount,
        decimal creditAmount)
    {
        lock (_sync)
        {
            if (!_invoices.TryGetValue(invoiceId, out var invoice))
            {
                throw new InvalidOperationException($"Invoice {invoiceId} is not tracked in cashier runtime store.");
            }

            if (_payments.TryGetValue(invoiceId, out var existingPayment))
            {
                RemovePaymentImpact(invoice, existingPayment);
            }

            var payment = new CashierPaymentState
            {
                InvoiceId = invoiceId,
                PaymentMethod = paymentMethod,
                PaidAmount = paidAmount,
                ChangeAmount = changeAmount,
                CashAmount = cashAmount,
                CardAmount = cardAmount,
                CreditAmount = creditAmount,
                PaidAtUtc = DateTime.UtcNow
            };

            _payments[invoiceId] = payment;
            ApplyPaymentImpact(invoice, payment);
            return ClonePayment(payment);
        }
    }

    public CashierPaymentState? GetPayment(int invoiceId)
    {
        lock (_sync)
        {
            return _payments.TryGetValue(invoiceId, out var payment) ? ClonePayment(payment) : null;
        }
    }

    public bool MarkInvoiceRefunded(int invoiceId, string? reason)
    {
        lock (_sync)
        {
            if (!_invoices.TryGetValue(invoiceId, out var invoice))
            {
                return false;
            }

            if (invoice.IsRefunded)
            {
                return true;
            }

            if (_payments.TryGetValue(invoiceId, out var payment))
            {
                RemovePaymentImpact(invoice, payment);
            }

            invoice.IsRefunded = true;
            invoice.RefundReason = reason?.Trim();
            invoice.RefundedAtUtc = DateTime.UtcNow;

            if (invoice.SessionId.HasValue && _sessions.TryGetValue(invoice.SessionId.Value, out var session))
            {
                session.TotalRefunds += invoice.TotalAmount;
            }

            return true;
        }
    }

    public DailySalesReportState BuildDailySalesReport(DateTime date)
    {
        lock (_sync)
        {
            var targetDate = date.Date;

            var paidInvoices = _payments.Values
                .Where(payment => payment.PaidAtUtc.Date == targetDate)
                .Select(payment => new
                {
                    Payment = payment,
                    Invoice = _invoices.TryGetValue(payment.InvoiceId, out var invoice) ? invoice : null
                })
                .Where(x => x.Invoice is not null && !x.Invoice.IsRefunded)
                .ToList();

            var totalSales = paidInvoices.Sum(x => x.Invoice!.TotalAmount);
            var totalTransactions = paidInvoices.Count;
            var cashAmount = paidInvoices.Sum(x => x.Payment.CashAmount);
            var cardAmount = paidInvoices.Sum(x => x.Payment.CardAmount);
            var creditAmount = paidInvoices.Sum(x => x.Payment.CreditAmount);

            var refundsAmount = _invoices.Values
                .Where(x => x.IsRefunded && x.RefundedAtUtc?.Date == targetDate)
                .Sum(x => x.TotalAmount);

            return new DailySalesReportState
            {
                Date = targetDate,
                TotalSales = totalSales,
                TotalTransactions = totalTransactions,
                CashAmount = cashAmount,
                CardAmount = cardAmount,
                CreditAmount = creditAmount,
                RefundsAmount = refundsAmount,
                AverageTransaction = totalTransactions > 0 ? totalSales / totalTransactions : 0m
            };
        }
    }

    public CashierPerformanceReportState BuildPerformanceReport(Guid userGuid, DateTime fromUtc, DateTime toUtc)
    {
        lock (_sync)
        {
            var from = fromUtc.ToUniversalTime();
            var to = toUtc.ToUniversalTime();

            var paidInvoices = _payments.Values
                .Where(payment => payment.PaidAtUtc >= from && payment.PaidAtUtc <= to)
                .Select(payment => new
                {
                    Payment = payment,
                    Invoice = _invoices.TryGetValue(payment.InvoiceId, out var invoice) ? invoice : null
                })
                .Where(x => x.Invoice is not null && x.Invoice.CashierUserGuid == userGuid && !x.Invoice.IsRefunded)
                .ToList();

            var sessionCount = _sessions.Values.Count(
                session => session.CashierUserGuid == userGuid &&
                           session.OpeningDateUtc >= from &&
                           session.OpeningDateUtc <= to);

            var totalSales = paidInvoices.Sum(x => x.Invoice!.TotalAmount);
            var totalTransactions = paidInvoices.Count;
            var totalCash = paidInvoices.Sum(x => x.Payment.CashAmount);
            var totalCard = paidInvoices.Sum(x => x.Payment.CardAmount);
            var totalCredit = paidInvoices.Sum(x => x.Payment.CreditAmount);

            return new CashierPerformanceReportState
            {
                CashierUserGuid = userGuid,
                TotalSales = totalSales,
                TotalTransactions = totalTransactions,
                TotalCash = totalCash,
                TotalCard = totalCard,
                TotalCredit = totalCredit,
                AverageTransaction = totalTransactions > 0 ? totalSales / totalTransactions : 0m,
                SessionsCount = sessionCount
            };
        }
    }

    private void ApplyPaymentImpact(CashierInvoiceState invoice, CashierPaymentState payment)
    {
        if (invoice.IsRefunded)
        {
            return;
        }

        if (!invoice.SessionId.HasValue || !_sessions.TryGetValue(invoice.SessionId.Value, out var session))
        {
            return;
        }

        session.TotalSales += invoice.TotalAmount;
        session.TotalCash += payment.CashAmount;
        session.TotalCard += payment.CardAmount;
        session.TotalCredit += payment.CreditAmount;
        session.TransactionsCount += 1;
    }

    private void RemovePaymentImpact(CashierInvoiceState invoice, CashierPaymentState payment)
    {
        if (!invoice.SessionId.HasValue || !_sessions.TryGetValue(invoice.SessionId.Value, out var session))
        {
            return;
        }

        session.TotalSales -= invoice.TotalAmount;
        session.TotalCash -= payment.CashAmount;
        session.TotalCard -= payment.CardAmount;
        session.TotalCredit -= payment.CreditAmount;
        session.TransactionsCount = Math.Max(0, session.TransactionsCount - 1);
    }

    private static CashierSessionState CloneSession(CashierSessionState source) =>
        new()
        {
            Id = source.Id,
            SessionNumber = source.SessionNumber,
            CashierUserId = source.CashierUserId,
            CashierUserGuid = source.CashierUserGuid,
            CashierUserName = source.CashierUserName,
            OpeningDateUtc = source.OpeningDateUtc,
            ClosingDateUtc = source.ClosingDateUtc,
            OpeningBalance = source.OpeningBalance,
            ClosingBalance = source.ClosingBalance,
            ExpectedBalance = source.ExpectedBalance,
            ActualBalance = source.ActualBalance,
            Difference = source.Difference,
            Status = source.Status,
            Notes = source.Notes,
            TreasuryId = source.TreasuryId,
            StoreDetailsId = source.StoreDetailsId,
            TotalSales = source.TotalSales,
            TotalRefunds = source.TotalRefunds,
            TotalCash = source.TotalCash,
            TotalCard = source.TotalCard,
            TotalCredit = source.TotalCredit,
            TransactionsCount = source.TransactionsCount,
            ClosedByUserGuid = source.ClosedByUserGuid,
            ClosedByUserName = source.ClosedByUserName,
            StockAlertLimit = source.StockAlertLimit
        };

    private static CashierInvoiceState CloneInvoice(CashierInvoiceState source) =>
        new()
        {
            InvoiceId = source.InvoiceId,
            SessionId = source.SessionId,
            CashierUserGuid = source.CashierUserGuid,
            CashierUserName = source.CashierUserName,
            TotalAmount = source.TotalAmount,
            CreatedAtUtc = source.CreatedAtUtc,
            StoreDetailsId = source.StoreDetailsId,
            TableId = source.TableId,
            IsPaid = source.IsPaid,
            IsRefunded = source.IsRefunded,
            RefundedAtUtc = source.RefundedAtUtc,
            RefundReason = source.RefundReason,
            ReservedByClass = source.ReservedByClass.ToDictionary(x => x.Key, x => x.Value)
        };

    private static CashierPaymentState ClonePayment(CashierPaymentState source) =>
        new()
        {
            InvoiceId = source.InvoiceId,
            PaymentMethod = source.PaymentMethod,
            PaidAmount = source.PaidAmount,
            ChangeAmount = source.ChangeAmount,
            CashAmount = source.CashAmount,
            CardAmount = source.CardAmount,
            CreditAmount = source.CreditAmount,
            PaidAtUtc = source.PaidAtUtc
        };
}

public sealed class CashierSessionState
{
    public int Id { get; set; }
    public string SessionNumber { get; set; } = string.Empty;
    public int CashierUserId { get; set; }
    public Guid CashierUserGuid { get; set; }
    public string CashierUserName { get; set; } = string.Empty;
    public DateTime OpeningDateUtc { get; set; }
    public DateTime? ClosingDateUtc { get; set; }
    public decimal OpeningBalance { get; set; }
    public decimal? ClosingBalance { get; set; }
    public decimal? ExpectedBalance { get; set; }
    public decimal? ActualBalance { get; set; }
    public decimal? Difference { get; set; }
    public int Status { get; set; }
    public string? Notes { get; set; }
    public Guid? ClosedByUserGuid { get; set; }
    public string? ClosedByUserName { get; set; }
    public int? TreasuryId { get; set; }
    public Guid? StoreDetailsId { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalRefunds { get; set; }
    public decimal TotalCash { get; set; }
    public decimal TotalCard { get; set; }
    public decimal TotalCredit { get; set; }
    public int TransactionsCount { get; set; }
    public int? StockAlertLimit { get; set; }
}

public sealed class CashierInvoiceState
{
    public int InvoiceId { get; set; }
    public int? SessionId { get; set; }
    public Guid CashierUserGuid { get; set; }
    public string CashierUserName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid? StoreDetailsId { get; set; }
    public int? TableId { get; set; }
    public bool IsPaid { get; set; }
    public bool IsRefunded { get; set; }
    public DateTime? RefundedAtUtc { get; set; }
    public string? RefundReason { get; set; }
    public Dictionary<Guid, decimal> ReservedByClass { get; set; } = new();
}

public sealed class CashierPaymentState
{
    public int InvoiceId { get; set; }
    public int PaymentMethod { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal ChangeAmount { get; set; }
    public decimal CashAmount { get; set; }
    public decimal CardAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public DateTime PaidAtUtc { get; set; }
}

public sealed class CashierSessionSummaryState
{
    public int SessionId { get; set; }
    public int TotalTransactions { get; set; }
    public decimal TotalSalesAmount { get; set; }
    public decimal TotalRefundsAmount { get; set; }
    public decimal CashAmount { get; set; }
    public decimal CardAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public decimal ExpectedCash { get; set; }
    public decimal ActualCash { get; set; }
    public decimal Difference { get; set; }
}

public sealed class DailySalesReportState
{
    public DateTime Date { get; set; }
    public decimal TotalSales { get; set; }
    public int TotalTransactions { get; set; }
    public decimal CashAmount { get; set; }
    public decimal CardAmount { get; set; }
    public decimal CreditAmount { get; set; }
    public decimal RefundsAmount { get; set; }
    public decimal AverageTransaction { get; set; }
}

public sealed class CashierPerformanceReportState
{
    public Guid CashierUserGuid { get; set; }
    public decimal TotalSales { get; set; }
    public int TotalTransactions { get; set; }
    public decimal TotalCash { get; set; }
    public decimal TotalCard { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal AverageTransaction { get; set; }
    public int SessionsCount { get; set; }
}
