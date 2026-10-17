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

public class CashierRestaurantSmokeFlowTests
{
    [Fact]
    public async Task CashierRestaurantFlow_ShouldRequirePaymentBeforeTableClose_ThenCloseSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();

        var cashierStore = new CashierRuntimeStore();
        var restaurantStore = new RestaurantRuntimeStore();

        var session = cashierStore.OpenSession(
            userId,
            cashierUserName: "cashier",
            openingBalance: 100m,
            treasuryId: null,
            storeDetailsId: storeId);

        var salesInvoicesService = new Mock<ISalesInvoicesService>();
        salesInvoicesService
            .Setup(service => service.GetByIdAsync(501))
            .ReturnsAsync(new SaleInvoice
            {
                Id = 501,
                TransactionId = null
            });

        var auditService = new Mock<ICashierAuditService>();

        var controller = new RestaurantTableController(
            restaurantStore,
            cashierStore,
            salesInvoicesService.Object,
            auditService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = BuildHttpContext(userId, session.Id, storeId)
            }
        };

        // Step 1: open table
        var openResult = await controller.OpenTable(new OpenRestaurantTableRequest
        {
            TableId = 7,
            TableName = "T7"
        });
        openResult.Should().BeOfType<OkObjectResult>();

        // Step 2: create/track invoice for table
        cashierStore.TrackInvoice(
            invoiceId: 501,
            sessionId: session.Id,
            userGuid: userId,
            userName: "cashier",
            totalAmount: 42m,
            createdAtUtc: DateTime.UtcNow,
            storeDetailsId: storeId,
            tableId: 7);

        // Step 3: close should fail before payment
        var closeBeforePayment = await controller.CloseTable(7);
        closeBeforePayment.Should().BeOfType<ConflictObjectResult>();

        // Step 4: process payment then close should pass
        cashierStore.MarkInvoicePaid(501).Should().BeTrue();

        var closeAfterPayment = await controller.CloseTable(7);
        closeAfterPayment.Should().BeOfType<OkObjectResult>();

        var table = restaurantStore.GetTable(7);
        table.Should().NotBeNull();
        table!.Status.Should().Be(RestaurantTableStatus.Available);

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
            Times.Exactly(2));
    }

    private static DefaultHttpContext BuildHttpContext(Guid userId, int sessionId, Guid storeId)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, userId.ToString()),
            new(ClaimTypes.Role, "Cashier")
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(identity)
        };

        context.Items["CashierSessionId"] = sessionId;
        context.Items["CashierStoreId"] = storeId;
        return context;
    }
}