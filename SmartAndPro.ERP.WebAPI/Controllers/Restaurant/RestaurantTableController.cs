using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartAndPro.ERP.Application.IServices;
using SmartAndPro.ERP.WebAPI.Services.Cashier;
using System.Security.Claims;

namespace SmartAndPro.ERP.WebAPI.Controllers.Restaurant;

[ApiController]
[Authorize]
[Route("api/restaurant/table")]
public class RestaurantTableController : ControllerBase
{
    private readonly IRestaurantRuntimeStore _restaurantStore;
    private readonly ICashierRuntimeStore _cashierStore;
    private readonly ISalesInvoicesService _salesInvoicesService;
    private readonly ICashierAuditService _auditService;

    public RestaurantTableController(
        IRestaurantRuntimeStore restaurantStore,
        ICashierRuntimeStore cashierStore,
        ISalesInvoicesService salesInvoicesService,
        ICashierAuditService auditService)
    {
        _restaurantStore = restaurantStore;
        _cashierStore = cashierStore;
        _salesInvoicesService = salesInvoicesService;
        _auditService = auditService;
    }

    [HttpPost("open")]
    public async Task<IActionResult> OpenTable([FromBody] OpenRestaurantTableRequest request)
    {
        if (!HasCashierAccess())
        {
            return StatusCode(403, new { message = "Cashier or Admin role is required" });
        }

        if (request.TableId <= 0)
        {
            return BadRequest(new { message = "TableId is required" });
        }

        var sessionId = HttpContext.Items["CashierSessionId"] as int?;
        var storeId = HttpContext.Items["CashierStoreId"] as Guid?;

        if (!sessionId.HasValue || !storeId.HasValue || storeId.Value == Guid.Empty)
        {
            return StatusCode(403, new
            {
                errorCode = "NO_ACTIVE_SESSION",
                message = "No active store-bound cashier session"
            });
        }

        var table = _restaurantStore.OpenTable(
            request.TableId,
            request.TableName,
            sessionId.Value,
            storeId.Value,
            ResolveCurrentUserGuid(),
            ResolveCurrentUserName());

        await _auditService.LogAsync(
            ResolveCurrentUserGuid(),
            ResolveCurrentUserName(),
            "TABLE_OPENED",
            "RestaurantTable",
            table.TableId.ToString(),
            sessionId,
            storeId,
            new { table.TableName, table.OpenedAtUtc });

        return Ok(new
        {
            tableId = table.TableId,
            tableName = table.TableName,
            status = table.Status,
            hasDraftInvoice = table.HasDraftInvoice,
            openedAt = table.OpenedAtUtc
        });
    }

    [HttpGet("{id}/invoices")]
    public async Task<IActionResult> GetTableInvoices(int id)
    {
        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid table id" });
        }

        var table = _restaurantStore.GetTable(id);
        if (table == null)
        {
            return NotFound(new { message = "Table not found" });
        }

        var invoiceIds = _cashierStore.GetTableInvoiceIds(id);
        var result = new List<object>();

        foreach (var invoiceId in invoiceIds)
        {
            var invoice = await _salesInvoicesService.GetByIdAsync(invoiceId);
            if (invoice == null)
            {
                continue;
            }

            var tracked = _cashierStore.GetInvoiceState(invoiceId);
            var isPaid = tracked?.IsPaid == true || invoice.TransactionId.HasValue;

            result.Add(new
            {
                id = invoice.Id,
                invoiceNo = invoice.InvoiceNo,
                invoiceDate = invoice.IssueDate,
                totalAmount = invoice.NetPrice,
                isPaid
            });
        }

        return Ok(result);
    }

    [HttpPost("{id}/close")]
    public async Task<IActionResult> CloseTable(int id)
    {
        if (!HasCashierAccess())
        {
            return StatusCode(403, new { message = "Cashier or Admin role is required" });
        }

        if (id <= 0)
        {
            return BadRequest(new { message = "Invalid table id" });
        }

        var table = _restaurantStore.GetTable(id);
        if (table == null)
        {
            return NotFound(new { message = "Table not found" });
        }

        var invoiceIds = _cashierStore.GetTableInvoiceIds(id);
        var unpaid = new List<int>();

        foreach (var invoiceId in invoiceIds)
        {
            var tracked = _cashierStore.GetInvoiceState(invoiceId);
            var invoice = await _salesInvoicesService.GetByIdAsync(invoiceId);
            var isPaid = tracked?.IsPaid == true || invoice?.TransactionId != null;
            if (!isPaid)
            {
                unpaid.Add(invoiceId);
            }
        }

        if (unpaid.Count > 0)
        {
            return Conflict(new
            {
                message = "Cannot close table while unpaid invoices exist",
                unpaidInvoiceIds = unpaid
            });
        }

        var closed = _restaurantStore.CloseTable(id, ResolveCurrentUserGuid(), ResolveCurrentUserName());

        await _auditService.LogAsync(
            ResolveCurrentUserGuid(),
            ResolveCurrentUserName(),
            "TABLE_CLOSED",
            "RestaurantTable",
            id.ToString(),
            table.SessionId,
            table.StoreDetailsId,
            new { closed.ClosedAtUtc, InvoiceCount = invoiceIds.Count });

        return Ok(new
        {
            tableId = closed.TableId,
            status = closed.Status,
            closedAt = closed.ClosedAtUtc
        });
    }

    private Guid ResolveCurrentUserGuid()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue(ClaimTypes.Name)
                  ?? User.FindFirstValue("sub");

        return Guid.TryParse(raw, out var parsed)
            ? parsed
            : Guid.Empty;
    }

    private string ResolveCurrentUserName()
    {
        var raw = User.FindFirstValue(ClaimTypes.GivenName)
                  ?? User.FindFirstValue(ClaimTypes.Name)
                  ?? User.Identity?.Name;

        return string.IsNullOrWhiteSpace(raw)
            ? "Cashier"
            : raw.Trim();
    }

    private bool HasCashierAccess()
    {
        return User.IsInRole("Admin") || User.IsInRole("Cashier") || User.HasClaim(c => c.Type == "Permission" && c.Value == "Cashier.POS");
    }
}

public class OpenRestaurantTableRequest
{
    public int TableId { get; set; }
    public string? TableName { get; set; }
}
