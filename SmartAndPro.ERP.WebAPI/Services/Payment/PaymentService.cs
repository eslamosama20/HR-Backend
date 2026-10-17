using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Infrastructure.Contexts;
using SmartAndPro.ERP.WebAPI.Services.Payment.Providers;
using PaymentEntity = SmartAndPro.ERP.Domains.Entities.Payment.Payment;

namespace SmartAndPro.ERP.WebAPI.Services.Payment;

public class PaymentService : IPaymentService
{
    private readonly ERPContext _db;
    private readonly IServiceProvider _sp;
    private readonly ILogger<PaymentService> _logger;
    private readonly PaymentProvidersOptions _options;

    // ── Method → Provider mapping ────────────────────────────────────
    private static readonly Dictionary<string, string> MethodProviderMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "card",      "paymob"   },
        { "mada",      "paymob"   },
        { "apple_pay", "paymob"   },
        { "tabby",     "tabby"    },
        { "tamara",    "tamara"   },
        { "stc_pay",   "hyperpay" },
    };

    // ── Available methods catalogue ──────────────────────────────────
    private static readonly List<PaymentMethodDto> AllMethods = new()
    {
        new() { Key = "card",      Label = "Credit / Debit Card",  Provider = "paymob",   IconUrl = "/icons/payment/card.svg"      },
        new() { Key = "mada",      Label = "Mada",                 Provider = "paymob",   IconUrl = "/icons/payment/mada.svg"      },
        new() { Key = "apple_pay", Label = "Apple Pay",            Provider = "paymob",   IconUrl = "/icons/payment/applepay.svg"  },
        new() { Key = "tabby",     Label = "Tabby (تابي)",         Provider = "tabby",    IconUrl = "/icons/payment/tabby.svg"     },
        new() { Key = "tamara",    Label = "Tamara (تمارا)",       Provider = "tamara",   IconUrl = "/icons/payment/tamara.svg"    },
        new() { Key = "stc_pay",   Label = "STC Pay",              Provider = "hyperpay", IconUrl = "/icons/payment/stcpay.svg"    },
    };

    public PaymentService(ERPContext db, IServiceProvider sp, ILogger<PaymentService> logger, Microsoft.Extensions.Options.IOptions<PaymentProvidersOptions> opts)
    {
        _db = db;
        _sp = sp;
        _logger = logger;
        _options = opts.Value;
    }

    // ── 1. Initiate payment ──────────────────────────────────────────
    public async Task<PaymentResponse> InitiatePaymentAsync(OrderDto order)
    {
        if (!MethodProviderMap.TryGetValue(order.PaymentMethod, out var providerKey))
            return new PaymentResponse { Success = false, ErrorMessage = $"Unsupported payment method: '{order.PaymentMethod}'." };

        var provider = ResolveProvider(providerKey);
        if (provider == null)
            return new PaymentResponse { Success = false, ErrorMessage = $"Provider '{providerKey}' is not registered." };

        // Persist a Pending record before calling the provider
        var payment = new PaymentEntity
        {
            Amount = order.Amount,
            Currency = order.Currency,
            Method = order.PaymentMethod.ToLower(),
            Provider = providerKey,
            Status = "Pending",
            OrderReference = order.OrderReference,
            CustomerName = order.CustomerName,
            CustomerEmail = order.CustomerEmail,
            CustomerPhone = order.CustomerPhone,
            Description = order.Description,
            CreatedAt = DateTime.UtcNow,
        };
        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();

        PaymentResponse response;
        try
        {
            response = await provider.CreatePaymentAsync(order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Provider {Provider} threw during CreatePayment for order {Ref}", providerKey, order.OrderReference);
            response = new PaymentResponse { Success = false, ErrorMessage = ex.Message, Status = "Failed" };
        }

        // Update record with provider response
        payment.ExternalId = response.ExternalId;
        payment.PaymentUrl = response.PaymentUrl;
        payment.Status = response.Success ? "Pending" : "Failed";
        await _db.SaveChangesAsync();

        response.RawData["internalPaymentId"] = payment.Id;
        return response;
    }

    // ── 2. Available methods ─────────────────────────────────────────
    public Task<List<PaymentMethodDto>> GetAvailableMethodsAsync(string country)
    {
        // All methods are available for SA by default.
        // You can filter by country here if needed.
        var methods = AllMethods
            .Where(m => m.Available)
            .ToList();

        return Task.FromResult(methods);
    }

    // ── 3. Handle webhook ────────────────────────────────────────────
    public async Task<(bool ok, string? error)> HandleWebhookAsync(string provider, string rawPayload, string? hmac = null)
    {
        var svc = ResolveProvider(provider);
        if (svc == null)
            return (false, $"Unknown provider: {provider}");

        try
        {
            if (provider.Equals("paymob", StringComparison.OrdinalIgnoreCase))
            {
                var hmacSecret = _options.Paymob.HmacSecret;
                if (string.IsNullOrWhiteSpace(hmac))
                {
                    try
                    {
                        using var docTmp = System.Text.Json.JsonDocument.Parse(rawPayload);
                        if (docTmp.RootElement.TryGetProperty("hmac", out var hProp))
                        {
                            hmac = hProp.GetString();
                        }
                    }
                    catch {}
                }

                if (string.IsNullOrWhiteSpace(hmac))
                {
                    _logger.LogWarning("[Paymob Webhook] Rejected: HMAC signature is missing.");
                    return (false, "HMAC signature is missing.");
                }

                bool isHmacValid = VerifyPaymobHmac(rawPayload, hmac, hmacSecret);
                if (!isHmacValid)
                {
                    _logger.LogWarning("[Paymob Webhook] Rejected: Invalid HMAC signature.");
                    return (false, "Invalid HMAC signature.");
                }

                string? eventId = null;
                try
                {
                    using var doc = System.Text.Json.JsonDocument.Parse(rawPayload);
                    if (doc.RootElement.TryGetProperty("obj", out var obj))
                    {
                        if (obj.TryGetProperty("id", out var idProp))
                        {
                            eventId = idProp.ValueKind == System.Text.Json.JsonValueKind.Number 
                                ? idProp.GetRawText() 
                                : idProp.GetString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to parse Paymob event ID");
                }

                if (!string.IsNullOrWhiteSpace(eventId))
                {
                    var eventExists = await _db.PaymentProviderEvents
                        .AnyAsync(e => e.Provider == "paymob" && e.EventId == eventId);

                    if (eventExists)
                    {
                        _logger.LogInformation("[Paymob Webhook] Event {EventId} already processed (idempotency hit). Returning success.", eventId);
                        return (true, "Event already processed");
                    }

                    var providerEvent = new SmartAndPro.ERP.Domains.Entities.Payment.PaymentProviderEvent
                    {
                        Provider = "paymob",
                        EventId = eventId,
                        Payload = rawPayload,
                        ProcessedAtUtc = DateTime.UtcNow,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = Guid.Empty
                    };
                    _db.PaymentProviderEvents.Add(providerEvent);
                    await _db.SaveChangesAsync();
                }
            }

            await svc.HandleWebhookAsync(rawPayload, hmac);

            // Try to update payment status from webhook payload
            await UpdatePaymentFromWebhookAsync(provider, rawPayload);

            return (true, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Webhook processing failed for provider {Provider}", provider);
            return (false, ex.Message);
        }
    }

    private static bool VerifyPaymobHmac(string rawPayload, string hmacVal, string hmacSecret)
    {
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(rawPayload);
            if (!doc.RootElement.TryGetProperty("obj", out var obj))
                return false;

            string GetVal(string name)
            {
                if (name.Contains('.'))
                {
                    var parts = name.Split('.');
                    if (obj.TryGetProperty(parts[0], out var parent) && parent.TryGetProperty(parts[1], out var child))
                    {
                        return GetJsonValueAsString(child);
                    }
                    return "";
                }
                else
                {
                    if (obj.TryGetProperty(name, out var val))
                    {
                        return GetJsonValueAsString(val);
                    }
                    return "";
                }
            }

            string GetJsonValueAsString(System.Text.Json.JsonElement el)
            {
                return el.ValueKind switch
                {
                    System.Text.Json.JsonValueKind.String => el.GetString() ?? "",
                    System.Text.Json.JsonValueKind.Number => el.GetRawText(),
                    System.Text.Json.JsonValueKind.True => "true",
                    System.Text.Json.JsonValueKind.False => "false",
                    _ => ""
                };
            }

            var fields = new[]
            {
                "amount_cents", "created_at", "currency", "error_occured", "has_parent_transaction",
                "id", "integration_id", "is_3d_secure", "is_auth", "is_capture", "is_refunded", "is_voided",
                "owner", "pending", "source_data.pan", "source_data.sub_type", "source_data.type", "success"
            };

            var concatenated = string.Concat(fields.Select(GetVal));

            using var hmacObj = new System.Security.Cryptography.HMACSHA512(System.Text.Encoding.UTF8.GetBytes(hmacSecret));
            var computedHash = hmacObj.ComputeHash(System.Text.Encoding.UTF8.GetBytes(concatenated));
            var computedHmacStr = Convert.ToHexString(computedHash).ToLower();

            return string.Equals(computedHmacStr, hmacVal, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    // ── Private helpers ──────────────────────────────────────────────

    private IPaymentProvider? ResolveProvider(string providerKey) => providerKey.ToLower() switch
    {
        "paymob"   => _sp.GetService<PaymobProvider>(),
        "tabby"    => _sp.GetService<TabbyProvider>(),
        "tamara"   => _sp.GetService<TamaraProvider>(),
        "hyperpay" => _sp.GetService<HyperPayProvider>(),
        _          => null
    };

    private async Task UpdatePaymentFromWebhookAsync(string provider, string rawPayload)
    {
        // Generic: try to find a payment by ExternalId mentioned in the payload
        // Provider-specific parsing can be extended per provider
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(rawPayload);
            string? externalId = null;
            string? newStatus = null;

            if (provider == "paymob")
            {
                // Paymob sends { "obj": { "id": ..., "success": true/false } }
                if (doc.RootElement.TryGetProperty("obj", out var obj))
                {
                    externalId = obj.TryGetProperty("id", out var idProp) ? idProp.GetRawText() : null;
                    if (obj.TryGetProperty("success", out var success))
                        newStatus = success.GetBoolean() ? "Paid" : "Failed";
                }
            }
            else
            {
                // Generic: look for "id" or "order_id" at root
                if (doc.RootElement.TryGetProperty("id", out var idProp))
                    externalId = idProp.GetString();
                if (doc.RootElement.TryGetProperty("status", out var statusProp))
                    newStatus = statusProp.GetString();
            }

            if (externalId != null)
            {
                var payment = await _db.Payments
                    .FirstOrDefaultAsync(p => p.ExternalId == externalId);

                if (payment != null && newStatus != null)
                {
                    payment.Status = newStatus;
                    payment.WebhookPayload = rawPayload[..Math.Min(2000, rawPayload.Length)];
                    if (newStatus == "Paid")
                        payment.PaidAt = DateTime.UtcNow;
                    await _db.SaveChangesAsync();
                    _logger.LogInformation("Payment {ExternalId} updated to {Status} via {Provider} webhook", externalId, newStatus, provider);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not parse webhook payload from {Provider}", provider);
        }
    }
}
