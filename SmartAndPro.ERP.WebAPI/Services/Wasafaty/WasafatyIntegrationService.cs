using System.Diagnostics;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SmartAndPro.ERP.Domains.Entities.Wasafaty;
using SmartAndPro.ERP.Domains.Entities.Pharmacy;
using SmartAndPro.ERP.Domains.Enums.Wasafaty;
using SmartAndPro.ERP.Infrastructure.Contexts;
using SmartAndPro.ERP.WebAPI.Controllers.Integrations.Wasafaty.Dtos;

namespace SmartAndPro.ERP.WebAPI.Services.Wasafaty;

public interface IWasafatyIntegrationService
{
    // Auth
    Task<string> GetAccessTokenAsync();
    Task SetTokenManuallyAsync(string accessToken, string? refreshToken, long? expiresAtUnix);

    // Webhook
    bool VerifyWebhookSignature(string payload, string signature);
    Task HandleWebhookAsync(string eventType, WasafatyNotificationData data, string rawPayload);

    // Prescription Operations
    Task<List<WasafatyPrescriptionResponse>> GetPendingPrescriptionsAsync(Guid? storeDetailsId);
    Task<WasafatyPrescriptionResponse?> GetPrescriptionByIdAsync(Guid id);
    Task<WasafatyPrescriptionResponse?> FetchFromWasafatyAsync(FetchPrescriptionRequest request, Guid? storeDetailsId);
    Task<VerifyOtpResponse> VerifyPatientOtpAsync(VerifyOtpRequest request);

    // Dispensing
    Task<bool> MarkDispensedAsync(DispenseRequest request);
    Task ReportDispenseToWasafatyAsync(Guid wasafatyPrescriptionId);

    // Insurance Claims (NPHIES)
    Task<SubmitClaimResponse> SubmitInsuranceClaimAsync(Guid wasafatyPrescriptionId);

    // Dashboard / Settings
    Task<WasafatySettingsResponse> GetSettingsAsync();
    Task<List<WasafatySyncLogResponse>> GetSyncLogsAsync(int page, int pageSize, string? eventType);
}

public class WasafatyIntegrationService : IWasafatyIntegrationService
{
    private readonly ERPContext _db;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<WasafatyIntegrationService> _logger;

    private readonly string _baseUrl;
    private readonly string _clientId;
    private readonly string _clientSecret;
    private readonly string _tokenUrl;
    private readonly string _facilityCode;
    private readonly string _webhookSecret;
    private readonly string _nphiesBaseUrl;

    private static string? _accessToken;
    private static string? _refreshToken;
    private static DateTime _tokenExpiry = DateTime.MinValue;

    public WasafatyIntegrationService(
        ERPContext db,
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<WasafatyIntegrationService> logger)
    {
        _db = db;
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;

        _baseUrl = configuration["Wasafaty:BaseUrl"] ?? "https://api.wasafaty.sa/fhir/r4";
        _clientId = configuration["Wasafaty:ClientId"] ?? "";
        _clientSecret = configuration["Wasafaty:ClientSecret"] ?? "";
        _tokenUrl = configuration["Wasafaty:TokenUrl"] ?? "https://auth.nhic.gov.sa/oauth2/token";
        _facilityCode = configuration["Wasafaty:FacilityCode"] ?? "";
        _webhookSecret = configuration["Wasafaty:WebhookSecret"] ?? "";
        _nphiesBaseUrl = configuration["Wasafaty:NphiesBaseUrl"] ?? "https://api.nphies.sa/fhir/r4";
    }

    // ═══════════════════════════ Authentication ═══════════════════════════

    public async Task<string> GetAccessTokenAsync()
    {
        // Restore from DB if in-memory is empty
        if (_accessToken == null)
        {
            var stored = await _db.Set<WasafatyAuthToken>()
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync();

            if (stored != null)
            {
                _accessToken = stored.AccessToken;
                _refreshToken = stored.RefreshToken;
                _tokenExpiry = stored.ExpiresAt;
            }
        }

        // If still no token, request new one via client_credentials
        if (_accessToken == null || DateTime.UtcNow >= _tokenExpiry)
        {
            var form = new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = _clientId,
                ["client_secret"] = _clientSecret,
                ["scope"] = "pharmacy.read pharmacy.dispense"
            };

            var response = await _httpClient.PostAsync(_tokenUrl, new FormUrlEncodedContent(form));
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var tokenData = JsonSerializer.Deserialize<JsonElement>(json);

            _accessToken = tokenData.GetProperty("access_token").GetString()!;
            _tokenExpiry = DateTime.UtcNow.AddSeconds(
                tokenData.GetProperty("expires_in").GetInt32() - 60);

            if (tokenData.TryGetProperty("refresh_token", out var rt))
                _refreshToken = rt.GetString();

            await PersistTokenAsync(_accessToken, _refreshToken, _tokenExpiry);
        }

        return _accessToken;
    }

    public async Task SetTokenManuallyAsync(string accessToken, string? refreshToken, long? expiresAtUnix)
    {
        _accessToken = accessToken;
        _refreshToken = refreshToken;
        _tokenExpiry = expiresAtUnix.HasValue
            ? DateTimeOffset.FromUnixTimeSeconds(expiresAtUnix.Value).UtcDateTime
            : DateTime.UtcNow.AddHours(4);

        await PersistTokenAsync(_accessToken, _refreshToken, _tokenExpiry);
    }

    private async Task EnsureAuthenticatedAsync()
    {
        var token = await GetAccessTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
    }

    private async Task PersistTokenAsync(string accessToken, string? refreshToken, DateTime expiresAt)
    {
        var old = await _db.Set<WasafatyAuthToken>().ToListAsync();
        _db.Set<WasafatyAuthToken>().RemoveRange(old);

        _db.Set<WasafatyAuthToken>().Add(new WasafatyAuthToken
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            IsActive = true
        });

        await _db.SaveChangesAsync();
    }

    // ═══════════════════════════ Webhook ═══════════════════════════

    public bool VerifyWebhookSignature(string payload, string signature)
    {
        if (string.IsNullOrEmpty(_webhookSecret)) return true;

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_webhookSecret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var computed = Convert.ToHexString(hash).ToLowerInvariant();
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computed),
            Encoding.UTF8.GetBytes(signature.ToLowerInvariant()));
    }

    public async Task HandleWebhookAsync(string eventType, WasafatyNotificationData data, string rawPayload)
    {
        var sw = Stopwatch.StartNew();
        _logger.LogInformation("Wasafaty webhook received: {Event}, FhirId: {FhirId}", eventType, data.FhirId);

        try
        {
            switch (eventType)
            {
                case "prescription.created":
                case "prescription.updated":
                    await HandlePrescriptionNotificationAsync(data);
                    break;
                case "prescription.cancelled":
                    await HandlePrescriptionCancelledAsync(data.FhirId);
                    break;
                default:
                    _logger.LogWarning("Unhandled Wasafaty event: {Event}", eventType);
                    break;
            }

            await LogSyncAsync(eventType, WasafatySyncDirection.WasafatyToErp, WasafatySyncStatus.Synced,
                rawPayload, null, null, data.FhirId, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling Wasafaty webhook {Event}", eventType);
            await LogSyncAsync(eventType, WasafatySyncDirection.WasafatyToErp, WasafatySyncStatus.Failed,
                rawPayload, null, ex.Message, data.FhirId, sw.ElapsedMilliseconds);
            throw;
        }
    }

    private async Task HandlePrescriptionNotificationAsync(WasafatyNotificationData data)
    {
        // Check if already imported
        if (await _db.Set<WasafatyPrescription>().AnyAsync(w => w.FhirResourceId == data.FhirId))
        {
            _logger.LogInformation("Wasafaty prescription {FhirId} already exists, skipping", data.FhirId);
            return;
        }

        // Pull full prescription from FHIR API
        await EnsureAuthenticatedAsync();
        var fhirUrl = $"{_baseUrl}/MedicationRequest/{data.FhirId}";
        var response = await _httpClient.GetAsync(fhirUrl);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Failed to fetch prescription {FhirId} from Wasafaty: {Status}",
                data.FhirId, response.StatusCode);
            return;
        }

        var json = await response.Content.ReadAsStringAsync();
        var fhir = JsonSerializer.Deserialize<FhirMedicationRequest>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (fhir == null) return;

        var prescription = MapFhirToPrescription(fhir, json);
        _db.Set<WasafatyPrescription>().Add(prescription);
        await _db.SaveChangesAsync();
    }

    private async Task HandlePrescriptionCancelledAsync(string fhirId)
    {
        var prescription = await _db.Set<WasafatyPrescription>()
            .FirstOrDefaultAsync(w => w.FhirResourceId == fhirId);

        if (prescription == null) return;

        prescription.Status = WasafatyPrescriptionStatus.Cancelled;
        await _db.SaveChangesAsync();
    }

    // ═══════════════════════════ Prescription Operations ═══════════════════════════

    public async Task<List<WasafatyPrescriptionResponse>> GetPendingPrescriptionsAsync(Guid? storeDetailsId)
    {
        var query = _db.Set<WasafatyPrescription>()
            .Include(w => w.Items)
            .Where(w => w.Status == WasafatyPrescriptionStatus.Pending
                      || w.Status == WasafatyPrescriptionStatus.Verified);

        if (storeDetailsId.HasValue)
            query = query.Where(w => w.StoreDetailsId == storeDetailsId || w.StoreDetailsId == null);

        var prescriptions = await query
            .OrderByDescending(w => w.ReceivedAt)
            .ToListAsync();

        return prescriptions.Select(MapToResponse).ToList();
    }

    public async Task<WasafatyPrescriptionResponse?> GetPrescriptionByIdAsync(Guid id)
    {
        var prescription = await _db.Set<WasafatyPrescription>()
            .Include(w => w.Items)
            .FirstOrDefaultAsync(w => w.Id == id);

        return prescription == null ? null : MapToResponse(prescription);
    }

    public async Task<WasafatyPrescriptionResponse?> FetchFromWasafatyAsync(
        FetchPrescriptionRequest request, Guid? storeDetailsId)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            await EnsureAuthenticatedAsync();

            string fhirUrl;
            if (!string.IsNullOrEmpty(request.FhirResourceId))
            {
                // Direct FHIR resource lookup
                fhirUrl = $"{_baseUrl}/MedicationRequest/{request.FhirResourceId}";
            }
            else if (!string.IsNullOrEmpty(request.NationalId))
            {
                // Search by patient NID — only active prescriptions
                fhirUrl = $"{_baseUrl}/MedicationRequest" +
                           $"?patient.identifier=urn:oid:2.16.840.1.113883.4.340|{request.NationalId}" +
                           $"&performer={_facilityCode}" +
                           "&status=active";
            }
            else
            {
                return null;
            }

            var response = await _httpClient.GetAsync(fhirUrl);
            var responseJson = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                await LogSyncAsync("prescription.fetch", WasafatySyncDirection.WasafatyToErp,
                    WasafatySyncStatus.Failed, fhirUrl, responseJson,
                    $"HTTP {response.StatusCode}", request.FhirResourceId, sw.ElapsedMilliseconds);
                return null;
            }

            FhirMedicationRequest fhir;

            // If searching by NID, we get a Bundle; by ID we get direct resource
            if (string.IsNullOrEmpty(request.FhirResourceId))
            {
                var bundle = JsonSerializer.Deserialize<FhirBundle>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                var firstEntry = bundle?.Entry?.FirstOrDefault()?.Resource;
                if (firstEntry == null) return null;
                fhir = firstEntry;
            }
            else
            {
                fhir = JsonSerializer.Deserialize<FhirMedicationRequest>(responseJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })!;
            }

            // Idempotency — check if already in DB
            var existing = await _db.Set<WasafatyPrescription>()
                .Include(w => w.Items)
                .FirstOrDefaultAsync(w => w.FhirResourceId == fhir.Id);

            if (existing != null)
            {
                return MapToResponse(existing);
            }

            // Save new
            var prescription = MapFhirToPrescription(fhir, responseJson);
            prescription.StoreDetailsId = storeDetailsId;
            _db.Set<WasafatyPrescription>().Add(prescription);
            await _db.SaveChangesAsync();

            await LogSyncAsync("prescription.fetch", WasafatySyncDirection.WasafatyToErp,
                WasafatySyncStatus.Synced, fhirUrl, null, null, fhir.Id, sw.ElapsedMilliseconds);

            return MapToResponse(prescription);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching prescription from Wasafaty");
            await LogSyncAsync("prescription.fetch", WasafatySyncDirection.WasafatyToErp,
                WasafatySyncStatus.Failed, null, null, ex.Message, request.FhirResourceId, sw.ElapsedMilliseconds);
            throw;
        }
    }

    public async Task<VerifyOtpResponse> VerifyPatientOtpAsync(VerifyOtpRequest request)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            await EnsureAuthenticatedAsync();

            var payload = new
            {
                nationalId = request.NationalId,
                otp = request.Otp,
                facilityCode = _facilityCode
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/Patient/$verify-otp", content);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                await LogSyncAsync("patient.verify-otp", WasafatySyncDirection.ErpToWasafaty,
                    WasafatySyncStatus.Failed, JsonSerializer.Serialize(payload), json,
                    $"HTTP {response.StatusCode}", null, sw.ElapsedMilliseconds);

                return new VerifyOtpResponse
                {
                    Verified = false,
                    Message = "OTP verification failed",
                    MessageAr = "فشل التحقق من رمز OTP"
                };
            }

            // Mark any matching prescriptions as OTP verified
            if (!string.IsNullOrEmpty(request.FhirResourceId))
            {
                var prescription = await _db.Set<WasafatyPrescription>()
                    .FirstOrDefaultAsync(w => w.FhirResourceId == request.FhirResourceId);

                if (prescription != null)
                {
                    prescription.OtpVerified = true;
                    prescription.Status = WasafatyPrescriptionStatus.Verified;
                    await _db.SaveChangesAsync();
                }
            }

            await LogSyncAsync("patient.verify-otp", WasafatySyncDirection.ErpToWasafaty,
                WasafatySyncStatus.Synced, null, null, null, null, sw.ElapsedMilliseconds);

            return new VerifyOtpResponse
            {
                Verified = true,
                Message = "Patient identity verified successfully",
                MessageAr = "تم التحقق من هوية المريض بنجاح"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying patient OTP");
            return new VerifyOtpResponse
            {
                Verified = false,
                Message = "OTP verification error",
                MessageAr = "خطأ في التحقق من رمز OTP"
            };
        }
    }

    // ═══════════════════════════ Dispensing ═══════════════════════════

    public async Task<bool> MarkDispensedAsync(DispenseRequest request)
    {
        var prescription = await _db.Set<WasafatyPrescription>()
            .Include(w => w.Items)
            .FirstOrDefaultAsync(w => w.Id == request.WasafatyPrescriptionId);

        if (prescription == null)
            throw new InvalidOperationException("Wasafaty prescription not found");

        foreach (var dispenseItem in request.Items)
        {
            var item = prescription.Items.FirstOrDefault(i => i.Id == dispenseItem.ItemId);
            if (item == null) continue;

            item.DispensedQty = dispenseItem.DispensedQty;
            item.BatchNo = dispenseItem.BatchNo;
            item.ExpiryDate = dispenseItem.ExpiryDate;
            item.ErpProductId = dispenseItem.ErpProductId;
            item.UnitPrice = dispenseItem.UnitPrice;
            item.LineTotal = dispenseItem.UnitPrice * dispenseItem.DispensedQty;

            if (prescription.InsuranceCoveragePercent > 0)
            {
                item.InsuranceCoveredAmount = item.LineTotal * (prescription.InsuranceCoveragePercent / 100m);
                item.PatientPayAmount = item.LineTotal - item.InsuranceCoveredAmount;
            }
            else
            {
                item.PatientPayAmount = item.LineTotal;
            }
        }

        // Link to sale invoice and cashier payment if provided
        if (request.SaleInvoiceId.HasValue)
            prescription.SaleInvoiceId = request.SaleInvoiceId;

        if (request.CashierPaymentId.HasValue)
            prescription.CashierPaymentId = request.CashierPaymentId;

        // Check if fully or partially dispensed
        var allFullyDispensed = prescription.Items
            .All(i => i.DispensedQty >= i.RequestedQty);

        prescription.Status = allFullyDispensed
            ? WasafatyPrescriptionStatus.Dispensed
            : WasafatyPrescriptionStatus.PartiallyDispensed;

        prescription.DispensedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return true;
    }

    public async Task ReportDispenseToWasafatyAsync(Guid wasafatyPrescriptionId)
    {
        var sw = Stopwatch.StartNew();

        var prescription = await _db.Set<WasafatyPrescription>()
            .Include(w => w.Items)
            .FirstOrDefaultAsync(w => w.Id == wasafatyPrescriptionId);

        if (prescription == null) return;

        try
        {
            await EnsureAuthenticatedAsync();

            // Build FHIR MedicationDispense resource
            var dispenseResource = new
            {
                resourceType = "MedicationDispense",
                status = "completed",
                medicationReference = new { reference = $"MedicationRequest/{prescription.FhirResourceId}" },
                subject = new
                {
                    identifier = new { system = "urn:oid:2.16.840.1.113883.4.340", value = prescription.PatientNationalId }
                },
                performer = new[]
                {
                    new { actor = new { identifier = new { system = "urn:oid:2.16.840.1.113883.4.341", value = _facilityCode } } }
                },
                whenHandedOver = prescription.DispensedAt?.ToString("o") ?? DateTime.UtcNow.ToString("o"),
                quantity = new { value = prescription.Items.Sum(i => i.DispensedQty), unit = "unit" }
            };

            var content = new StringContent(
                JsonSerializer.Serialize(dispenseResource),
                Encoding.UTF8,
                "application/fhir+json");

            var response = await _httpClient.PostAsync($"{_baseUrl}/MedicationDispense", content);
            var responseJson = await response.Content.ReadAsStringAsync();

            await LogSyncAsync("prescription.dispense", WasafatySyncDirection.ErpToWasafaty,
                response.IsSuccessStatusCode ? WasafatySyncStatus.Synced : WasafatySyncStatus.Failed,
                JsonSerializer.Serialize(dispenseResource), responseJson,
                response.IsSuccessStatusCode ? null : $"HTTP {response.StatusCode}",
                prescription.FhirResourceId, sw.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reporting dispense to Wasafaty for {FhirId}", prescription.FhirResourceId);
            await LogSyncAsync("prescription.dispense", WasafatySyncDirection.ErpToWasafaty,
                WasafatySyncStatus.Failed, null, null, ex.Message,
                prescription.FhirResourceId, sw.ElapsedMilliseconds);
        }
    }

    // ═══════════════════════════ Insurance Claims (NPHIES) ═══════════════════════════

    public async Task<SubmitClaimResponse> SubmitInsuranceClaimAsync(Guid wasafatyPrescriptionId)
    {
        var sw = Stopwatch.StartNew();

        var prescription = await _db.Set<WasafatyPrescription>()
            .Include(w => w.Items)
            .FirstOrDefaultAsync(w => w.Id == wasafatyPrescriptionId);

        if (prescription == null)
            return new SubmitClaimResponse { Success = false, Message = "Prescription not found", MessageAr = "الوصفة غير موجودة" };

        if (prescription.InsuranceCoveragePercent <= 0)
            return new SubmitClaimResponse { Success = false, Message = "No insurance coverage", MessageAr = "لا يوجد تغطية تأمينية" };

        try
        {
            await EnsureAuthenticatedAsync();

            // Build NPHIES Claim resource
            var claimResource = new
            {
                resourceType = "Claim",
                type = new { coding = new[] { new { system = "http://terminology.hl7.org/CodeSystem/claim-type", code = "pharmacy" } } },
                patient = new
                {
                    identifier = new { system = "urn:oid:2.16.840.1.113883.4.340", value = prescription.PatientNationalId }
                },
                provider = new
                {
                    identifier = new { system = "urn:oid:2.16.840.1.113883.4.341", value = _facilityCode }
                },
                insurance = new[]
                {
                    new
                    {
                        sequence = 1,
                        focal = true,
                        coverage = new { display = prescription.InsuranceName },
                        identifier = prescription.InsurancePolicyNo
                    }
                },
                item = prescription.Items.Select((item, index) => new
                {
                    sequence = index + 1,
                    productOrService = new { coding = new[] { new { system = "urn:oid:2.16.840.1.113883.6.312", code = item.DrugCode } } },
                    quantity = new { value = item.DispensedQty },
                    unitPrice = new { value = item.UnitPrice, currency = "SAR" },
                    net = new { value = item.LineTotal, currency = "SAR" }
                }).ToList(),
                total = new { value = prescription.Items.Sum(i => i.LineTotal), currency = "SAR" },
                prescription = new { reference = $"MedicationRequest/{prescription.FhirResourceId}" }
            };

            var content = new StringContent(
                JsonSerializer.Serialize(claimResource),
                Encoding.UTF8,
                "application/fhir+json");

            var response = await _httpClient.PostAsync($"{_nphiesBaseUrl}/Claim", content);
            var responseJson = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var claimResponse = JsonSerializer.Deserialize<JsonElement>(responseJson);
                var claimId = claimResponse.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;

                prescription.ClaimStatus = WasafatyClaimStatus.Submitted;
                prescription.NphiesClaimId = claimId;
                await _db.SaveChangesAsync();

                await LogSyncAsync("nphies.claim.submit", WasafatySyncDirection.ErpToWasafaty,
                    WasafatySyncStatus.Synced, JsonSerializer.Serialize(claimResource), responseJson,
                    null, prescription.FhirResourceId, sw.ElapsedMilliseconds);

                return new SubmitClaimResponse
                {
                    Success = true,
                    NphiesClaimId = claimId,
                    Status = "Submitted",
                    Message = "Insurance claim submitted successfully",
                    MessageAr = "تم تقديم مطالبة التأمين بنجاح"
                };
            }
            else
            {
                prescription.ClaimStatus = WasafatyClaimStatus.Rejected;
                prescription.ErrorMessage = $"NPHIES HTTP {response.StatusCode}";
                await _db.SaveChangesAsync();

                await LogSyncAsync("nphies.claim.submit", WasafatySyncDirection.ErpToWasafaty,
                    WasafatySyncStatus.Failed, JsonSerializer.Serialize(claimResource), responseJson,
                    $"HTTP {response.StatusCode}", prescription.FhirResourceId, sw.ElapsedMilliseconds);

                return new SubmitClaimResponse
                {
                    Success = false,
                    Status = "Failed",
                    Message = "Insurance claim submission failed",
                    MessageAr = "فشل تقديم مطالبة التأمين"
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting NPHIES claim for prescription {Id}", wasafatyPrescriptionId);
            return new SubmitClaimResponse
            {
                Success = false,
                Message = $"Error: {ex.Message}",
                MessageAr = "خطأ في تقديم مطالبة التأمين"
            };
        }
    }

    // ═══════════════════════════ Dashboard / Settings ═══════════════════════════

    public async Task<WasafatySettingsResponse> GetSettingsAsync()
    {
        var today = DateTime.UtcNow.Date;

        return new WasafatySettingsResponse
        {
            IsConnected = _accessToken != null && DateTime.UtcNow < _tokenExpiry,
            FacilityCode = _facilityCode,
            TokenExpiresAt = _tokenExpiry > DateTime.MinValue ? _tokenExpiry : null,
            PendingPrescriptions = await _db.Set<WasafatyPrescription>()
                .CountAsync(w => w.Status == WasafatyPrescriptionStatus.Pending
                              || w.Status == WasafatyPrescriptionStatus.Verified),
            DispensedToday = await _db.Set<WasafatyPrescription>()
                .CountAsync(w => w.DispensedAt != null && w.DispensedAt.Value.Date == today),
            PendingClaims = await _db.Set<WasafatyPrescription>()
                .CountAsync(w => w.InsuranceCoveragePercent > 0
                              && w.ClaimStatus == WasafatyClaimStatus.NotClaimed
                              && w.Status == WasafatyPrescriptionStatus.Dispensed)
        };
    }

    public async Task<List<WasafatySyncLogResponse>> GetSyncLogsAsync(int page, int pageSize, string? eventType)
    {
        var query = _db.Set<WasafatySyncLog>().AsQueryable();

        if (!string.IsNullOrEmpty(eventType))
            query = query.Where(l => l.EventType == eventType);

        return await query
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new WasafatySyncLogResponse
            {
                Id = l.Id,
                EventType = l.EventType,
                Direction = l.Direction.ToString(),
                Status = l.Status.ToString(),
                ErrorMessage = l.ErrorMessage,
                FhirResourceId = l.FhirResourceId,
                DurationMs = l.DurationMs,
                CreatedAt = l.CreatedAt
            })
            .ToListAsync();
    }

    // ═══════════════════════════ Private Helpers ═══════════════════════════

    private WasafatyPrescription MapFhirToPrescription(FhirMedicationRequest fhir, string rawJson)
    {
        var prescription = new WasafatyPrescription
        {
            FhirResourceId = fhir.Id,
            PatientNationalId = fhir.PatientNationalId
                ?? fhir.Subject?.Identifier?.Value
                ?? "",
            PatientName = fhir.PatientName
                ?? fhir.Subject?.Display
                ?? "Unknown",
            DoctorName = fhir.DoctorName
                ?? fhir.Requester?.Display,
            DoctorLicenseNo = fhir.DoctorLicenseNo
                ?? fhir.Requester?.Identifier?.Value,
            PrescriberFacilityName = fhir.FacilityName,
            InsuranceName = fhir.InsuranceName
                ?? fhir.Insurance?.FirstOrDefault()?.Coverage?.Display,
            InsurancePolicyNo = fhir.InsurancePolicyNo,
            InsuranceCoveragePercent = fhir.InsuranceCoveragePercent ?? 0,
            Status = WasafatyPrescriptionStatus.Pending,
            ReceivedAt = DateTime.UtcNow,
            RawFhirJson = rawJson,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        if (fhir.Items != null)
        {
            foreach (var fhirItem in fhir.Items)
            {
                prescription.Items.Add(new WasafatyPrescriptionItem
                {
                    DrugCode = fhirItem.DrugCode,
                    DrugName = fhirItem.DrugName,
                    GenericName = fhirItem.GenericName,
                    DosageForm = fhirItem.DosageForm,
                    Strength = fhirItem.Strength,
                    DosageInstruction = fhirItem.DosageInstruction,
                    Frequency = fhirItem.Frequency,
                    DurationDays = fhirItem.DurationDays,
                    RequestedQty = fhirItem.Quantity,
                    AllowSubstitute = fhirItem.AllowSubstitute,
                    IsControlled = fhirItem.IsControlled,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        prescription.ItemsCount = prescription.Items.Count;
        return prescription;
    }

    private static WasafatyPrescriptionResponse MapToResponse(WasafatyPrescription p)
    {
        return new WasafatyPrescriptionResponse
        {
            Id = p.Id,
            FhirResourceId = p.FhirResourceId,
            PatientNationalId = p.PatientNationalId,
            PatientName = p.PatientName,
            DoctorName = p.DoctorName,
            DoctorLicenseNo = p.DoctorLicenseNo,
            PrescriberFacilityName = p.PrescriberFacilityName,
            InsuranceName = p.InsuranceName,
            InsurancePolicyNo = p.InsurancePolicyNo,
            InsuranceCoveragePercent = p.InsuranceCoveragePercent,
            ItemsCount = p.ItemsCount,
            Status = p.Status.ToString(),
            OtpVerified = p.OtpVerified,
            ReceivedAt = p.ReceivedAt,
            Items = p.Items.Select(i => new WasafatyPrescriptionItemResponse
            {
                Id = i.Id,
                DrugCode = i.DrugCode,
                DrugName = i.DrugName,
                GenericName = i.GenericName,
                DosageForm = i.DosageForm,
                Strength = i.Strength,
                DosageInstruction = i.DosageInstruction,
                Frequency = i.Frequency,
                DurationDays = i.DurationDays,
                RequestedQty = i.RequestedQty,
                DispensedQty = i.DispensedQty,
                UnitPrice = i.UnitPrice,
                LineTotal = i.LineTotal,
                InsuranceCoveredAmount = i.InsuranceCoveredAmount,
                PatientPayAmount = i.PatientPayAmount,
                AllowSubstitute = i.AllowSubstitute,
                IsControlled = i.IsControlled,
                ErpProductId = i.ErpProductId,
                BatchNo = i.BatchNo,
                ExpiryDate = i.ExpiryDate
            }).ToList()
        };
    }

    private async Task LogSyncAsync(string eventType, WasafatySyncDirection direction, WasafatySyncStatus status,
        string? request, string? response, string? error, string? fhirResourceId, long durationMs)
    {
        _db.Set<WasafatySyncLog>().Add(new WasafatySyncLog
        {
            EventType = eventType,
            Direction = direction,
            Status = status,
            RequestPayload = request?.Length > 4000 ? request[..4000] : request,
            ResponsePayload = response?.Length > 4000 ? response[..4000] : response,
            ErrorMessage = error?.Length > 2000 ? error[..2000] : error,
            FhirResourceId = fhirResourceId,
            DurationMs = (int)durationMs
        });
        await _db.SaveChangesAsync();
    }
}
