using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Domains.Entities.Cashier;
using SmartAndPro.ERP.Infrastructure.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.WebAPI.Controllers.Cashier
{
    [ApiController]
    [Route("api/cashier")]
    [Authorize]
    public class HeldOrdersController : ControllerBase
    {
        private readonly ERPContext _context;
        private readonly ILogger<HeldOrdersController> _logger;

        public HeldOrdersController(ERPContext context, ILogger<HeldOrdersController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ── 1. Create Held Order ──────────────────────────────────────────
        [HttpPost("held-orders")]
        public async Task<IActionResult> CreateHeldOrder([FromBody] HeldOrderRequestDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid payload.");

            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userId = string.IsNullOrEmpty(userIdString) ? Guid.Empty : (Guid.TryParse(userIdString, out var parsedUid) ? parsedUid : Guid.Empty);

            Guid storeDetailsGuid = Guid.Empty;
            if (!string.IsNullOrEmpty(dto.StoreDetailsId) && !Guid.TryParse(dto.StoreDetailsId, out storeDetailsGuid))
            {
                // try to find store
                var store = await _context.StoreDetails.IgnoreQueryFilters().FirstOrDefaultAsync(s => s.ArabicName == dto.StoreDetailsId || s.EnglishName == dto.StoreDetailsId);
                if (store != null) storeDetailsGuid = store.Id;
            }
            else if (!string.IsNullOrEmpty(dto.StoreDetailsId))
            {
                Guid.TryParse(dto.StoreDetailsId, out storeDetailsGuid);
            }

            var heldOrder = new HeldOrder
            {
                Id = Guid.NewGuid(),
                StoreDetailsId = storeDetailsGuid,
                TerminalId = dto.TerminalId ?? "POS-01",
                SessionId = dto.SessionId,
                RequestedByUserGuid = userId,
                HeldAtUtc = DateTime.UtcNow,
                CustomerName = dto.CustomerName ?? "طلب معلق",
                Notes = dto.Notes,
                TotalAmount = dto.TotalAmount,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            foreach (var line in dto.Lines ?? new List<HeldOrderLineRequestDto>())
            {
                Guid prodGuid = Guid.Empty;
                if (!string.IsNullOrWhiteSpace(line.ProductId) && !Guid.TryParse(line.ProductId, out prodGuid))
                {
                    // If productId is integer, look up in MainPurchaseClass by itemCode or Name
                    var pClass = await _context.MainPurchaseClasses.IgnoreQueryFilters()
                        .FirstOrDefaultAsync(p => p.ReferenceCode == line.ItemCode || p.ArabicName == line.ItemName);
                    if (pClass != null) prodGuid = pClass.Id;
                }
                else if (!string.IsNullOrWhiteSpace(line.ProductId))
                {
                    Guid.TryParse(line.ProductId, out prodGuid);
                }

                heldOrder.Lines.Add(new HeldOrderLine
                {
                    Id = Guid.NewGuid(),
                    HeldOrderId = heldOrder.Id,
                    ProductId = prodGuid,
                    ProductUnitId = line.ProductUnitId,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                    VatRate = line.VatRate,
                    VatAmount = line.VatAmount,
                    TotalAmount = line.TotalAmount,
                    ItemName = line.ItemName,
                    ItemCode = line.ItemCode,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                });
            }

            _context.HeldOrders.Add(heldOrder);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                id = heldOrder.Id,
                customerName = heldOrder.CustomerName,
                totalAmount = heldOrder.TotalAmount,
                heldAt = heldOrder.HeldAtUtc,
                lines = heldOrder.Lines.Select(l => new
                {
                    productId = l.ProductId,
                    productUnitId = l.ProductUnitId,
                    quantity = l.Quantity,
                    unitPrice = l.UnitPrice,
                    vatRate = l.VatRate,
                    vatAmount = l.VatAmount,
                    totalAmount = l.TotalAmount,
                    itemName = l.ItemName,
                    itemCode = l.ItemCode
                }).ToList()
            });
        }

        // ── 2. Get Held Orders ────────────────────────────────────────────
        [HttpGet("held-orders")]
        public async Task<IActionResult> GetHeldOrders(
            [FromQuery] Guid storeDetailsId,
            [FromQuery] int sessionId,
            [FromQuery] string? terminalId = null)
        {
            var query = _context.HeldOrders
                .Include(h => h.Lines)
                .Where(h => h.StoreDetailsId == storeDetailsId && h.SessionId == sessionId);

            if (!string.IsNullOrEmpty(terminalId))
            {
                query = query.Where(h => h.TerminalId == terminalId);
            }

            var results = await query.ToListAsync();
            return Ok(results);
        }

        // ── 3. Delete / Restore Held Order ────────────────────────────────
        [HttpDelete("held-orders/{id}")]
        public async Task<IActionResult> DeleteHeldOrder(Guid id, [FromQuery] bool restore = false)
        {
            // If restoring, we need to bypass the global query filter for IsActive
            var query = restore 
                ? _context.HeldOrders.IgnoreQueryFilters().Include(h => h.Lines)
                : _context.HeldOrders.Include(h => h.Lines);

            var heldOrder = await query.FirstOrDefaultAsync(h => h.Id == id);
            if (heldOrder == null)
                return NotFound("Held order not found.");

            if (restore)
            {
                heldOrder.IsActive = true;
                foreach (var line in heldOrder.Lines)
                {
                    line.IsActive = true;
                }
            }
            else
            {
                heldOrder.IsActive = false;
                foreach (var line in heldOrder.Lines)
                {
                    line.IsActive = false;
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true, restored = restore });
        }
    }

    public class HeldOrderRequestDto
    {
        public string? StoreDetailsId { get; set; }
        public string? TerminalId { get; set; }
        public int SessionId { get; set; }
        public string? CustomerName { get; set; }
        public string? Notes { get; set; }
        public decimal TotalAmount { get; set; }
        public List<HeldOrderLineRequestDto> Lines { get; set; } = new();
    }

    public class HeldOrderLineRequestDto
    {
        public string? ProductId { get; set; }
        public int? ProductUnitId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal VatRate { get; set; }
        public decimal VatAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string? ItemName { get; set; }
        public string? ItemCode { get; set; }
    }
}
