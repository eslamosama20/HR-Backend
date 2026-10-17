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
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.WebAPI.Controllers.Cashier;
using SmartAndPro.ERP.WebAPI.Controllers.Cashier.Dtos;
using SmartAndPro.ERP.WebAPI.Services.Cashier;
using SmartAndPro.ERP.WebAPI.Services.Pharmacy;
using SmartAndPro.ERP.WebAPI.Services.Wasafaty;
using Xunit;

namespace SmartAndPro.ERP.Tests.ApplicationTests.Controllers;

public class CashierPaymentControllerTests
{
    [Fact]
    public async Task ProcessPayment_ShouldSucceed_WhenAccountingTransactionFails()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        const int sessionId = 11;
        const int invoiceId = 9001;

        var cashierStore = new CashierRuntimeStore();
        cashierStore.TrackInvoice(
            invoiceId,
            sessionId,
            userId,
            "cashier",
            55m,
            DateTime.UtcNow,
            storeId);

        var salesService = new Mock<ISalesInvoicesService>();
        salesService
            .Setup(service => service.GetByIdAsync(invoiceId))
            .ReturnsAsync(new SaleInvoice
            {
                Id = invoiceId,
                InvoiceNo = "INV-9001",
                TransactionId = null,
                IsFrontal = false,
                NetPrice = 55m
            });

        salesService
            .Setup(service => service.CreateTransaction(invoiceId, false))
            .ReturnsAsync(0);

        var reservationService = new Mock<ICashierReservationService>();
        var auditService = new Mock<ICashierAuditService>();
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<CashierPaymentController>>();
        var cchiService = new Mock<IPharmacyCchiClaimService>();

        var controller = new CashierPaymentController(
            salesService.Object,
            cashierStore,
            reservationService.Object,
            auditService.Object,
            logger.Object,
            new RepositoriesFactory(),
            new Mock<IWasafatyIntegrationService>().Object,
            cchiService.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = BuildHttpContext(userId, sessionId, storeId)
            }
        };

        var request = new SmartAndPro.ERP.WebAPI.Controllers.Cashier.Dtos.PaymentRequest
        {
            InvoiceId = invoiceId,
            TotalAmount = 55m,
            PaymentMethod = 0,
            CashAmount = 60m
        };

        // Act
        var result = await controller.ProcessPayment(request);

        // Assert
        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var payload = ok.Value.Should().BeOfType<PaymentResponse>().Subject;

        payload.Success.Should().BeTrue();
        payload.InvoiceId.Should().Be(invoiceId);
        payload.Message.Should().Contain("Payment processed successfully");

        cashierStore.GetInvoiceState(invoiceId)!.IsPaid.Should().BeTrue();

        reservationService.Verify(service => service.ConfirmInvoiceAsync(invoiceId, It.IsAny<CancellationToken>()), Times.Once);
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
            Times.Once);
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