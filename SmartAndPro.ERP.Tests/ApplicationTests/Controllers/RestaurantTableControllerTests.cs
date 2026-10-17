using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SmartAndPro.ERP.Application.IServices;
using SmartAndPro.ERP.Domains.Entities.Invoice.Sale;
using SmartAndPro.ERP.WebAPI.Controllers.Restaurant;
using SmartAndPro.ERP.WebAPI.Services.Cashier;
using Xunit;

namespace SmartAndPro.ERP.Tests.ApplicationTests.Controllers;

public class RestaurantTableControllerTests
{
    [Fact]
    public async Task CloseTable_ShouldReturnConflict_WhenUnpaidInvoicesExist()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var restaurantStore = new RestaurantRuntimeStore();
        var cashierStore = new CashierRuntimeStore();

        restaurantStore.OpenTable(
            tableId: 12,
            tableName: "T12",
            sessionId: 1,
            storeDetailsId: storeId,
            cashierUserGuid: userId,
            cashierUserName: "cashier");

        cashierStore.TrackInvoice(
            invoiceId: 101,
            sessionId: 1,
            userGuid: userId,
            userName: "cashier",
            totalAmount: 50m,
            createdAtUtc: DateTime.UtcNow,
            storeDetailsId: storeId,
            tableId: 12);

        var salesInvoicesService = new Mock<ISalesInvoicesService>();
        salesInvoicesService
            .Setup(service => service.GetByIdAsync(101))
            .ReturnsAsync(new SaleInvoice
            {
                Id = 101,
                TransactionId = null
            });

        var auditService = new Mock<ICashierAuditService>();

        var controller = new RestaurantTableController(
            restaurantStore,
            cashierStore,
            salesInvoicesService.Object,
            auditService.Object);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = BuildHttpContext(userId, "Cashier")
        };

        // Act
        var result = await controller.CloseTable(12);

        // Assert
        var conflict = result.Should().BeOfType<ConflictObjectResult>().Subject;
        conflict.StatusCode.Should().Be(StatusCodes.Status409Conflict);

        var json = System.Text.Json.JsonSerializer.Serialize(conflict.Value);
        json.Should().Contain("Cannot close table while unpaid invoices exist");
        json.Should().Contain("101");

        auditService.Verify(
            service => service.LogAsync(
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<int?>(),
                It.IsAny<Guid?>(),
                It.IsAny<object?>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static DefaultHttpContext BuildHttpContext(Guid userId, string role)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, userId.ToString()),
            new(ClaimTypes.Role, role)
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };

        context.Items["CashierSessionId"] = 1;
        context.Items["CashierStoreId"] = Guid.NewGuid();
        return context;
    }
}
