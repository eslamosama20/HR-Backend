using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.ReceiptConnections;
using SmartAndPro.ERP.Application.Common.Dtos.Reservation.Reservation;
using SmartAndPro.ERP.Application.Common.Utility;
using SmartAndPro.ERP.Application.Services.Reservation;
using SmartAndPro.ERP.Domains.Entities.Invoice.Purchase;
using SmartAndPro.ERP.Domains.Entities.Invoice.Sale;
using SmartAndPro.ERP.Domains.Entities.Receipts;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.Infrastructure.Repositories.Base;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;
using System.Collections.Concurrent;

namespace SmartAndPro.ERP.Application.Services.Receipts.Common;

public enum ReceiptConnectionOwner
{
    Paycheck,
    Payment
}

public interface IReceiptWithConnections
{
    List<ReceiptConnectionResponseDto>? ReceiptConnections { get; set; }
    decimal PaidAmount { get; set; }
    decimal RemainingAmount { get; set; }
    decimal TotalInvoiceAmount { get; set; }
}

public sealed record ReceiptConnectionPayload(
    Guid? Id,
    int? SaleInvoiceId,
    int? PurchaseInvoiceId,
    Guid? ReservationId,
    decimal LinkedAmount,
    ReceiptConnectionType ReceiptConnectionType
);

public class ReceiptConnectionCoordinator
{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly IMapper _mapper;

    public ReceiptConnectionCoordinator(RepositoriesFactory repositoriesFactory, IMapper mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _mapper = mapper;
    }

    public async Task<OperationResponse<bool>> ValidateAsync(
        IEnumerable<ReceiptConnectionPayload> connections,
        ReceiptConnectionOwner owner,
        Guid? currentReceiptId = null)
    {
        var payloads = connections?.ToList() ?? new List<ReceiptConnectionPayload>();
        if (payloads.Count == 0)
        {
            return new OperationResponse<bool>(ErrorCodes.Success, true);
        }

        foreach (var connection in payloads)
        {
            if (connection.LinkedAmount <= 0)
            {
                var response = new OperationResponse<bool>(ErrorCodes.GeneralFail, false)
                {
                    Message = "Linked amount must be greater than zero."
                };
                return response;
            }
        }

        var receiptConnectionRepo = _repositoriesFactory.GetRepository<ReceiptConnection>();

        // Validate sales invoices
        var groupedSales = payloads
            .Where(x => x.SaleInvoiceId.HasValue)
            .GroupBy(x => x.SaleInvoiceId!.Value);

        foreach (var group in groupedSales)
        {
            var saleInvoiceRepo = _repositoriesFactory.GetRepository<SaleInvoice>();
            var saleInvoiceQuery = await saleInvoiceRepo.Filter(si => si.Id == group.Key);
            var saleInvoice = saleInvoiceQuery.Data?.FirstOrDefault();

            if (saleInvoice is null)
            {
                return new OperationResponse<bool>(ErrorCodes.GeneralFail, false)
                {
                    Message = $"SaleInvoice with ID {group.Key} not found in DB."
                };
            }

            var existingConnectionsQuery = await receiptConnectionRepo.Filter(rc =>
                rc.SaleInvoiceId == group.Key &&
                (owner == ReceiptConnectionOwner.Paycheck
                    ? rc.PaycheckReceiptId != currentReceiptId
                    : rc.PaymentReceiptId != currentReceiptId));

            var currentTotal = existingConnectionsQuery.Data?.Sum(rc => rc.LinkedAmount) ?? 0;
            var requestedTotal = group.Sum(x => x.LinkedAmount);
            var invoiceTotal = CalculateSaleInvoiceTotal(saleInvoice);

            if (currentTotal + requestedTotal > invoiceTotal)
            {
                return new OperationResponse<bool>(ErrorCodes.GeneralFail, false)
                {
                    Message =
                        $"Cannot link receipt: requested amount ({requestedTotal:F2}) with existing total ({currentTotal:F2}) exceeds sale invoice total ({invoiceTotal:F2})."
                };
            }
        }

        // Validate reservations
        var groupedReservations = payloads
            .Where(x => x.ReservationId.HasValue)
            .GroupBy(x => x.ReservationId!.Value);

        foreach (var group in groupedReservations)
        {
            var reservationRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Reservation.Reservation>();
            var reservationQuery = await reservationRepo.Filter(r => r.Id == group.Key);
            var reservation = reservationQuery.Data?.FirstOrDefault();

            if (reservation is null)
            {
                return new OperationResponse<bool>(ErrorCodes.GeneralFail, false)
                {
                    Message = $"Reservation with ID {group.Key} not found in DB."
                };
            }

            var reservationDto = _mapper.Map<ReservationResponseDto>(reservation);
            var totalPrice = ReservationService.GetTotalAmount(reservationDto);

            var existingConnectionsQuery = await receiptConnectionRepo.Filter(rc =>
                rc.ReservationId == group.Key &&
                (owner == ReceiptConnectionOwner.Paycheck
                    ? rc.PaycheckReceiptId != currentReceiptId
                    : rc.PaymentReceiptId != currentReceiptId));

            var currentTotal = existingConnectionsQuery.Data?.Sum(rc => rc.LinkedAmount) ?? 0;
            var requestedTotal = group.Sum(x => x.LinkedAmount);

            if (currentTotal + requestedTotal > totalPrice)
            {
                return new OperationResponse<bool>(ErrorCodes.GeneralFail, false)
                {
                    Message =
                        $"Cannot link receipt: requested amount ({requestedTotal:F2}) with existing total ({currentTotal:F2}) exceeds reservation total ({totalPrice:F2})."
                };
            }
        }

        // Validate purchase invoices
        var groupedPurchases = payloads
            .Where(x => x.PurchaseInvoiceId.HasValue)
            .GroupBy(x => x.PurchaseInvoiceId!.Value);

        foreach (var group in groupedPurchases)
        {
            var purchaseInvoiceRepo = _repositoriesFactory.GetRepository<PurchaseInvoice>();
            var purchaseInvoiceQuery = await purchaseInvoiceRepo.Filter(pi => pi.Id == group.Key);
            var purchaseInvoice = purchaseInvoiceQuery.Data?.FirstOrDefault();

            if (purchaseInvoice is null)
            {
                return new OperationResponse<bool>(ErrorCodes.GeneralFail, false)
                {
                    Message = $"PurchaseInvoice with ID {group.Key} not found in DB."
                };
            }

            var existingConnectionsQuery = await receiptConnectionRepo.Filter(rc =>
                rc.PurchaseInvoiceId == group.Key &&
                (owner == ReceiptConnectionOwner.Paycheck
                    ? rc.PaycheckReceiptId != currentReceiptId
                    : rc.PaymentReceiptId != currentReceiptId));

            var currentTotal = existingConnectionsQuery.Data?.Sum(rc => rc.LinkedAmount) ?? 0;
            var requestedTotal = group.Sum(x => x.LinkedAmount);
            var invoiceTotal = CalculatePurchaseInvoiceTotal(purchaseInvoice);

            if (currentTotal + requestedTotal > invoiceTotal)
            {
                return new OperationResponse<bool>(ErrorCodes.GeneralFail, false)
                {
                    Message =
                        $"Cannot link receipt: requested amount ({requestedTotal:F2}) with existing total ({currentTotal:F2}) exceeds purchase invoice total ({invoiceTotal:F2})."
                };
            }
        }

        return new OperationResponse<bool>(ErrorCodes.Success, true);
    }

    public async Task SyncConnectionsAsync(
        Guid receiptId,
        IEnumerable<ReceiptConnectionPutDto> connections,
        ReceiptConnectionOwner owner)
    {
        var connectionList = connections?.ToList() ?? new List<ReceiptConnectionPutDto>();
        var receiptConnectionRepo = _repositoriesFactory.GetRepository<ReceiptConnection>();

        var existingConnectionsQuery = owner switch
        {
            ReceiptConnectionOwner.Paycheck => await receiptConnectionRepo.Filter(rc => rc.PaycheckReceiptId == receiptId),
            ReceiptConnectionOwner.Payment => await receiptConnectionRepo.Filter(rc => rc.PaymentReceiptId == receiptId),
            _ => throw new ArgumentOutOfRangeException(nameof(owner), owner, null)
        };

        var existingConnections = existingConnectionsQuery.Data?.ToList() ?? new List<ReceiptConnection>();

        foreach (var connectionDto in connectionList)
        {
            var connectionEntity = _mapper.Map<ReceiptConnection>(connectionDto);
            SetOwner(connectionEntity, owner, receiptId);
            connectionEntity.PaycheckReceipt = null!;
            connectionEntity.PaymentReceipt = null!;
            connectionEntity.SaleInvoice = null!;
            connectionEntity.PurchaseInvoice = null!;
            connectionEntity.Reservation = null!;

            if (connectionDto.Id.HasValue)
            {
                await receiptConnectionRepo.Update(connectionEntity);
            }
            else
            {
                await receiptConnectionRepo.Add(connectionEntity);
            }
        }

        var toDelete = existingConnections
            .Where(existing => connectionList.All(dto => dto.Id != existing.Id))
            .ToList();

        foreach (var connection in toDelete)
        {
            connection.PaycheckReceipt = null!;
            connection.PaymentReceipt = null!;
            connection.SaleInvoice = null!;
            connection.PurchaseInvoice = null!;
            connection.Reservation = null!;
            await receiptConnectionRepo.Delete(connection);
        }
    }

    public async Task AddConnectionsAsync(
        Guid receiptId,
        IEnumerable<ReceiptConnectionPostDto> connections,
        ReceiptConnectionOwner owner)
    {
        var receiptConnectionRepo = _repositoriesFactory.GetRepository<ReceiptConnection>();
        foreach (var connectionDto in connections ?? Enumerable.Empty<ReceiptConnectionPostDto>())
        {
            var connectionEntity = _mapper.Map<ReceiptConnection>(connectionDto);
            SetOwner(connectionEntity, owner, receiptId);
            connectionEntity.PaycheckReceipt = null!;
            connectionEntity.PaymentReceipt = null!;
            connectionEntity.SaleInvoice = null!;
            connectionEntity.PurchaseInvoice = null!;
            connectionEntity.Reservation = null!;

            await receiptConnectionRepo.Add(connectionEntity);
        }
    }

    public async Task PopulateMetadataAsync(IReceiptWithConnections receipt)
    {
        if (receipt?.ReceiptConnections == null || receipt.ReceiptConnections.Count == 0)
        {
            receipt!.PaidAmount = 0;
            receipt.RemainingAmount = 0;
            receipt.TotalInvoiceAmount = 0;
            return;
        }

        var saleInvoiceIds = receipt.ReceiptConnections
            .Where(x => x.SaleInvoiceId.HasValue)
            .Select(x => x.SaleInvoiceId!.Value)
            .Distinct()
            .ToList();

        var purchaseInvoiceIds = receipt.ReceiptConnections
            .Where(x => x.PurchaseInvoiceId.HasValue)
            .Select(x => x.PurchaseInvoiceId!.Value)
            .Distinct()
            .ToList();

        var reservationIds = receipt.ReceiptConnections
            .Where(x => x.ReservationId.HasValue)
            .Select(x => x.ReservationId!.Value)
            .Distinct()
            .ToList();

        var saleInvoicesDict = await LoadSaleInvoicesAsync(saleInvoiceIds);
        var purchaseInvoicesDict = await LoadPurchaseInvoicesAsync(purchaseInvoiceIds);
        var reservationDict = await LoadReservationsAsync(reservationIds);

    var receiptConnectionRepo = _repositoriesFactory.GetRepository<ReceiptConnection>();

    var salePayments = await AggregateSalePaymentsAsync(receiptConnectionRepo, saleInvoiceIds);
    var purchasePayments = await AggregatePurchasePaymentsAsync(receiptConnectionRepo, purchaseInvoiceIds);
    var reservationPayments = await AggregateReservationPaymentsAsync(receiptConnectionRepo, reservationIds);

        var saleAggregationIds = new HashSet<int>();
        var purchaseAggregationIds = new HashSet<int>();
        var reservationAggregationIds = new HashSet<Guid>();
        decimal receiptLinkedAmountTotal = 0;

        foreach (var connection in receipt.ReceiptConnections)
        {
            receiptLinkedAmountTotal += connection.LinkedAmount;

            if (connection.SaleInvoiceId.HasValue)
            {
                var saleInvoiceId = connection.SaleInvoiceId.Value;
                saleAggregationIds.Add(saleInvoiceId);

                var invoiceTotal = saleInvoicesDict.TryGetValue(saleInvoiceId, out var saleInvoice)
                    ? CalculateSaleInvoiceTotal(saleInvoice)
                    : connection.InvoiceTotalAmount;

                var paidAmount = salePayments.TryGetValue(saleInvoiceId, out var paid) ? paid : 0;
                if (invoiceTotal <= 0)
                {
                    invoiceTotal = connection.LinkedAmount;
                }

                connection.InvoiceTotalAmount = decimal.Round(invoiceTotal, 2);
                connection.InvoicePaidAmount = decimal.Round(paidAmount, 2);
                connection.InvoiceRemainingAmount = decimal.Round(Math.Max(0, connection.InvoiceTotalAmount - connection.InvoicePaidAmount), 2);

                if (saleInvoicesDict.TryGetValue(saleInvoiceId, out var saleInvoiceEntity))
                {
                    connection.InvoiceReference = saleInvoiceEntity.InvoiceNo;
                    connection.OperationType = "فاتورة مبيعات";
                }
                else if (string.IsNullOrWhiteSpace(connection.OperationType))
                {
                    connection.OperationType = "فاتورة مبيعات";
                }
            }
            else if (connection.PurchaseInvoiceId.HasValue)
            {
                var purchaseInvoiceId = connection.PurchaseInvoiceId.Value;
                purchaseAggregationIds.Add(purchaseInvoiceId);

                var invoiceTotal = purchaseInvoicesDict.TryGetValue(purchaseInvoiceId, out var purchaseInvoice)
                    ? CalculatePurchaseInvoiceTotal(purchaseInvoice)
                    : connection.InvoiceTotalAmount;

                var paidAmount = purchasePayments.TryGetValue(purchaseInvoiceId, out var paid) ? paid : 0;
                if (invoiceTotal <= 0)
                {
                    invoiceTotal = connection.LinkedAmount;
                }

                connection.InvoiceTotalAmount = decimal.Round(invoiceTotal, 2);
                connection.InvoicePaidAmount = decimal.Round(paidAmount, 2);
                connection.InvoiceRemainingAmount = decimal.Round(Math.Max(0, connection.InvoiceTotalAmount - connection.InvoicePaidAmount), 2);

                if (purchaseInvoicesDict.TryGetValue(purchaseInvoiceId, out var purchaseInvoiceEntity))
                {
                    connection.InvoiceReference = purchaseInvoiceEntity.InvoiceNo;
                    connection.OperationType = "فاتورة شراء";
                }
                else if (string.IsNullOrWhiteSpace(connection.OperationType))
                {
                    connection.OperationType = "فاتورة شراء";
                }
            }
            else if (connection.ReservationId.HasValue)
            {
                var reservationId = connection.ReservationId.Value;
                reservationAggregationIds.Add(reservationId);

                var invoiceTotal = reservationDict.TryGetValue(reservationId, out var reservation)
                    ? ReservationService.GetTotalAmount(_mapper.Map<ReservationResponseDto>(reservation))
                    : connection.InvoiceTotalAmount;

                var paidAmount = reservationPayments.TryGetValue(reservationId, out var paid) ? paid : 0;
                if (invoiceTotal <= 0)
                {
                    invoiceTotal = connection.LinkedAmount;
                }

                connection.InvoiceTotalAmount = decimal.Round(invoiceTotal, 2);
                connection.InvoicePaidAmount = decimal.Round(paidAmount, 2);
                connection.InvoiceRemainingAmount = decimal.Round(Math.Max(0, connection.InvoiceTotalAmount - connection.InvoicePaidAmount), 2);

                if (reservationDict.TryGetValue(reservationId, out var reservationEntity))
                {
                    connection.InvoiceReference = reservationEntity.ReferenceNumber ?? $"R-{reservationId}";
                }
                if (string.IsNullOrWhiteSpace(connection.OperationType))
                {
                    connection.OperationType = "حجز";
                }
            }

            if (string.IsNullOrWhiteSpace(connection.InvoiceReference))
            {
                connection.InvoiceReference = connection.SaleInvoiceId.HasValue
                    ? $"SI-{connection.SaleInvoiceId}"
                    : connection.PurchaseInvoiceId.HasValue
                        ? $"PI-{connection.PurchaseInvoiceId}"
                        : connection.ReservationId.HasValue
                            ? $"R-{connection.ReservationId}"
                            : "-";
            }

            if (string.IsNullOrWhiteSpace(connection.OperationType))
            {
                connection.OperationType = "عملية";
            }
        }

        var aggregatedSaleTotal = saleAggregationIds.Sum(id =>
            saleInvoicesDict.TryGetValue(id, out var saleInvoice)
                ? CalculateSaleInvoiceTotal(saleInvoice)
                : 0);

        var aggregatedPurchaseTotal = purchaseAggregationIds.Sum(id =>
            purchaseInvoicesDict.TryGetValue(id, out var purchaseInvoice)
                ? CalculatePurchaseInvoiceTotal(purchaseInvoice)
                : 0);

        var aggregatedReservationTotal = reservationAggregationIds.Sum(id =>
        {
            if (!reservationDict.TryGetValue(id, out var reservation))
            {
                return 0m;
            }

            var reservationDto = _mapper.Map<ReservationResponseDto>(reservation);
            return ReservationService.GetTotalAmount(reservationDto);
        });

        var aggregatedSaleRemaining = saleAggregationIds.Sum(id =>
        {
            var total = saleInvoicesDict.TryGetValue(id, out var saleInvoice)
                ? CalculateSaleInvoiceTotal(saleInvoice)
                : 0;
            var paid = salePayments.TryGetValue(id, out var paidValue) ? paidValue : 0;
            return Math.Max(0, decimal.Round(total - paid, 2));
        });

        var aggregatedPurchaseRemaining = purchaseAggregationIds.Sum(id =>
        {
            var total = purchaseInvoicesDict.TryGetValue(id, out var purchaseInvoice)
                ? CalculatePurchaseInvoiceTotal(purchaseInvoice)
                : 0;
            var paid = purchasePayments.TryGetValue(id, out var paidValue) ? paidValue : 0;
            return Math.Max(0, decimal.Round(total - paid, 2));
        });

        var aggregatedReservationRemaining = reservationAggregationIds.Sum(id =>
        {
            var total = reservationDict.TryGetValue(id, out var reservation)
                ? ReservationService.GetTotalAmount(_mapper.Map<ReservationResponseDto>(reservation))
                : 0;
            var paid = reservationPayments.TryGetValue(id, out var paidValue) ? paidValue : 0;
            return Math.Max(0, decimal.Round(total - paid, 2));
        });

        receipt.TotalInvoiceAmount = decimal.Round(aggregatedSaleTotal + aggregatedPurchaseTotal + aggregatedReservationTotal, 2);
        receipt.PaidAmount = decimal.Round(receiptLinkedAmountTotal, 2);
        receipt.RemainingAmount = decimal.Round(aggregatedSaleRemaining + aggregatedPurchaseRemaining + aggregatedReservationRemaining, 2);
    }

    private static void SetOwner(ReceiptConnection connection, ReceiptConnectionOwner owner, Guid receiptId)
    {
        switch (owner)
        {
            case ReceiptConnectionOwner.Paycheck:
                connection.PaycheckReceiptId = receiptId;
                connection.PaymentReceiptId = null;
                break;
            case ReceiptConnectionOwner.Payment:
                connection.PaymentReceiptId = receiptId;
                connection.PaycheckReceiptId = null;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(owner), owner, null);
        }
    }

    private static decimal CalculateSaleInvoiceTotal(SaleInvoice saleInvoice)
    {
        if (saleInvoice == null)
        {
            return 0;
        }

        var itemsTotal = saleInvoice.SaleItems?.Sum(item => item.TotalPrice) ?? 0;
        if (itemsTotal > 0)
        {
            return decimal.Round(itemsTotal, 2);
        }

        var netItems = saleInvoice.SaleItems?.Sum(item => item.NetPrice) ?? 0;
        var taxItems = saleInvoice.SaleItems?.Sum(item => item.TaxAmount) ?? 0;
        var discountItems = saleInvoice.SaleItems?.Sum(item => item.DiscountAmount) ?? 0;
        var computed = netItems + taxItems - discountItems;
        if (computed > 0)
        {
            return decimal.Round(computed, 2);
        }

        var invoiceTotals = saleInvoice.NetPrice + saleInvoice.TaxAmount;
        if (invoiceTotals > 0)
        {
            return decimal.Round(invoiceTotals, 2);
        }

        if (saleInvoice.TotalPrice > 0)
        {
            return decimal.Round(saleInvoice.TotalPrice, 2);
        }

        return 0;
    }

    private static decimal CalculatePurchaseInvoiceTotal(PurchaseInvoice purchaseInvoice)
    {
        if (purchaseInvoice == null)
        {
            return 0;
        }

        var itemsTotal = purchaseInvoice.PurchaseItems?.Sum(item => item.TotalPrice) ?? 0;
        if (itemsTotal > 0)
        {
            return decimal.Round(itemsTotal, 2);
        }

        var netItems = purchaseInvoice.PurchaseItems?.Sum(item => item.NetPrice) ?? 0;
        var taxItems = purchaseInvoice.PurchaseItems?.Sum(item => item.TaxAmount) ?? 0;
        var discountItems = purchaseInvoice.PurchaseItems?.Sum(item => item.DiscountAmount) ?? 0;
        var computed = netItems + taxItems - discountItems;
        if (computed > 0)
        {
            return decimal.Round(computed, 2);
        }

        var invoiceTotals = purchaseInvoice.NetPrice + purchaseInvoice.TaxAmount;
        if (invoiceTotals > 0)
        {
            return Math.Round(invoiceTotals, 2);
        }

        if (purchaseInvoice.TotalPrice > 0)
        {
            return Math.Round(purchaseInvoice.TotalPrice, 2);
        }

        return 0;
    }

    private async Task<Dictionary<int, SaleInvoice>> LoadSaleInvoicesAsync(IEnumerable<int> ids)
    {
        var idList = ids?.ToList() ?? new List<int>();
        if (idList.Count == 0)
        {
            return new Dictionary<int, SaleInvoice>();
        }

        var saleInvoiceRepo = _repositoriesFactory.GetRepository<SaleInvoice>();
        var query = await saleInvoiceRepo.Filter(si => idList.Contains(si.Id));
        return query.Data?.ToDictionary(x => x.Id, x => x) ?? new Dictionary<int, SaleInvoice>();
    }

    private async Task<Dictionary<int, PurchaseInvoice>> LoadPurchaseInvoicesAsync(IEnumerable<int> ids)
    {
        var idList = ids?.ToList() ?? new List<int>();
        if (idList.Count == 0)
        {
            return new Dictionary<int, PurchaseInvoice>();
        }

        var purchaseInvoiceRepo = _repositoriesFactory.GetRepository<PurchaseInvoice>();
        var query = await purchaseInvoiceRepo.Filter(pi => idList.Contains(pi.Id));
        return query.Data?.ToDictionary(x => x.Id, x => x) ?? new Dictionary<int, PurchaseInvoice>();
    }

    private async Task<Dictionary<Guid, SmartAndPro.ERP.Domains.Entities.Reservation.Reservation>> LoadReservationsAsync(IEnumerable<Guid> ids)
    {
        var idList = ids?.ToList() ?? new List<Guid>();
        if (idList.Count == 0)
        {
            return new Dictionary<Guid, SmartAndPro.ERP.Domains.Entities.Reservation.Reservation>();
        }

        var reservationRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Reservation.Reservation>();
        var query = await reservationRepo.Filter(r => idList.Contains(r.Id));
        return query.Data?.ToDictionary(x => x.Id, x => x) ?? new Dictionary<Guid, SmartAndPro.ERP.Domains.Entities.Reservation.Reservation>();
    }

    private static async Task<Dictionary<int, decimal>> AggregateSalePaymentsAsync(
        IRepository<ReceiptConnection> repository,
        IEnumerable<int> saleInvoiceIds)
    {
        var idList = saleInvoiceIds?.ToList() ?? new List<int>();
        if (idList.Count == 0)
        {
            return new Dictionary<int, decimal>();
        }

        var connectionsQuery = await repository.Filter(rc => rc.SaleInvoiceId.HasValue && idList.Contains(rc.SaleInvoiceId.Value));
        var connections = connectionsQuery.Data?.ToList() ?? new List<ReceiptConnection>();

        return connections
            .Where(rc => rc.SaleInvoiceId.HasValue)
            .GroupBy(rc => rc.SaleInvoiceId!.Value)
            .ToDictionary(group => group.Key, group => group.Sum(rc => rc.LinkedAmount));
    }

    private static async Task<Dictionary<int, decimal>> AggregatePurchasePaymentsAsync(
        IRepository<ReceiptConnection> repository,
        IEnumerable<int> purchaseInvoiceIds)
    {
        var idList = purchaseInvoiceIds?.ToList() ?? new List<int>();
        if (idList.Count == 0)
        {
            return new Dictionary<int, decimal>();
        }

        var connectionsQuery = await repository.Filter(rc => rc.PurchaseInvoiceId.HasValue && idList.Contains(rc.PurchaseInvoiceId.Value));
        var connections = connectionsQuery.Data?.ToList() ?? new List<ReceiptConnection>();

        return connections
            .Where(rc => rc.PurchaseInvoiceId.HasValue)
            .GroupBy(rc => rc.PurchaseInvoiceId!.Value)
            .ToDictionary(group => group.Key, group => group.Sum(rc => rc.LinkedAmount));
    }

    private static async Task<Dictionary<Guid, decimal>> AggregateReservationPaymentsAsync(
        IRepository<ReceiptConnection> repository,
        IEnumerable<Guid> reservationIds)
    {
        var idList = reservationIds?.ToList() ?? new List<Guid>();
        if (idList.Count == 0)
        {
            return new Dictionary<Guid, decimal>();
        }

        var connectionsQuery = await repository.Filter(rc => rc.ReservationId.HasValue && idList.Contains(rc.ReservationId.Value));
        var connections = connectionsQuery.Data?.ToList() ?? new List<ReceiptConnection>();

        return connections
            .Where(rc => rc.ReservationId.HasValue)
            .GroupBy(rc => rc.ReservationId!.Value)
            .ToDictionary(group => group.Key, group => group.Sum(rc => rc.LinkedAmount));
    }
}
