using System;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Cashier;
using SmartAndPro.ERP.Infrastructure.Contexts;
using SmartAndPro.ERP.Infrastructure.Contexts.Utility;
using SmartAndPro.ERP.WebAPI.Services.Cashier;
using Xunit;

namespace SmartAndPro.ERP.Tests.ApplicationTests.Services.Cashier;

public class CashierReservationServiceTests
{
    [Fact]
    public async Task GetReservedQuantitiesByClassAsync_ShouldExcludeInvoiceAndOnlyCountReserved()
    {
        // Arrange
        var context = CreateContext();
        var storeId = Guid.NewGuid();
        var classA = Guid.NewGuid();
        var classB = Guid.NewGuid();

        context.CashierStockReservations.AddRange(
            CreateReservation(1001, storeId, classA, 3m, CashierReservationStatus.Reserved),
            CreateReservation(1002, storeId, classA, 2m, CashierReservationStatus.Reserved),
            CreateReservation(1003, storeId, classB, 4m, CashierReservationStatus.Confirmed),
            CreateReservation(1004, storeId, classB, 1.5m, CashierReservationStatus.Reserved)
        );
        await context.SaveChangesAsync();

        var mockServiceBaseFactory = new Mock<ServiceBaseFactory>();
        var mockLogger = new Mock<ILogger<CashierReservationService>>();
        var service = new CashierReservationService(context, mockServiceBaseFactory.Object, mockLogger.Object);

        // Act
        var result = await service.GetReservedQuantitiesByClassAsync(storeId, new[] { classA, classB }, excludeInvoiceId: 1002);

        // Assert
        result.Should().ContainKey(classA);
        result[classA].Should().Be(3m);

        result.Should().ContainKey(classB);
        result[classB].Should().Be(1.5m);
    }

    [Fact]
    public async Task ConfirmInvoiceAsync_ShouldMoveReservationOutOfReservedBucket()
    {
        // Arrange
        var context = CreateContext();
        var storeId = Guid.NewGuid();
        var classA = Guid.NewGuid();

        context.CashierStockReservations.Add(CreateReservation(2001, storeId, classA, 5m, CashierReservationStatus.Reserved));
        await context.SaveChangesAsync();

        var mockServiceBaseFactory = new Mock<ServiceBaseFactory>();
        var mockLogger = new Mock<ILogger<CashierReservationService>>();
        var service = new CashierReservationService(context, mockServiceBaseFactory.Object, mockLogger.Object);

        // Act
        await service.ConfirmInvoiceAsync(2001);
        var reservedAfterConfirm = await service.GetReservedQuantitiesByClassAsync(storeId, new[] { classA });

        // Assert
        reservedAfterConfirm.ContainsKey(classA).Should().BeFalse();

        var updated = await context.CashierStockReservations.FirstAsync(x => x.InvoiceId == 2001);
        updated.Status.Should().Be(CashierReservationStatus.Confirmed);
        updated.ConfirmedAtUtc.Should().NotBeNull();
    }

    private static ERPContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ERPContext>()
            .UseInMemoryDatabase($"cashier-reservation-tests-{Guid.NewGuid()}")
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var autoNumberGenerator = new Mock<IAutoNumberGenerator>();
        return new ERPContext(options, autoNumberGenerator.Object);
    }

    private static CashierStockReservation CreateReservation(
        int invoiceId,
        Guid storeId,
        Guid classId,
        decimal quantity,
        CashierReservationStatus status)
    {
        return new CashierStockReservation
        {
            Id = Guid.NewGuid(),
            InvoiceId = invoiceId,
            SessionId = 1,
            StoreDetailsId = storeId,
            MainSalesClassId = classId,
            Quantity = quantity,
            Status = status,
            ReservedAtUtc = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = Guid.NewGuid(),
            IsActive = true
        };
    }
}
