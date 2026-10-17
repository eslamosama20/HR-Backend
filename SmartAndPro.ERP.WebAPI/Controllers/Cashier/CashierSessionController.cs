using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.IServices;
using SmartAndPro.ERP.Application.Services.Auth.Interfaces;
using SmartAndPro.ERP.Domains.Entities.Auth.Security;
using SmartAndPro.ERP.Domains.Entities.Inventory.SalesClassesManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.StoreManagement;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.WebAPI.Services.Cashier;
using SmartAndPro.ERP.WebAPI.Services.Salla;
using System.Security.Claims;

namespace SmartAndPro.ERP.WebAPI.Controllers.Cashier
{
    /// <summary>
    /// Cashier Session Management Controller
    /// Controls for managing cashier sessions (shifts)
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/cashier/session")]
    public class CashierSessionController : ControllerBase
    {
        private readonly ILogger<CashierSessionController> _logger;
        private readonly ICashierRuntimeStore _cashierStore;
        private readonly ICashierAuditService _auditService;
        private readonly ISalesInvoicesService _salesInvoicesService;
        private readonly RepositoriesFactory _repositoriesFactory;
        private readonly ISallaIntegrationService _sallaService;
        private readonly IAuthService _authService;

        // Discrepancy threshold — session close is blocked if |diff| > this value
        private const decimal DiscrepancyThreshold = 50m;

        public CashierSessionController(
            ILogger<CashierSessionController> logger,
            ICashierRuntimeStore cashierStore,
            ICashierAuditService auditService,
            ISalesInvoicesService salesInvoicesService,
            RepositoriesFactory repositoriesFactory,
            ISallaIntegrationService sallaService,
            IAuthService authService)
        {
            _logger = logger;
            _cashierStore = cashierStore;
            _auditService = auditService;
            _salesInvoicesService = salesInvoicesService;
            _repositoriesFactory = repositoriesFactory;
            _sallaService = sallaService;
            _authService = authService;
        }

        // ── Permission helper ──────────────────────────────────────
        private async Task<bool> HasPermissionAsync(string permission)
        {
            // Quick claim check first (avoids DB hit if claims are embedded in JWT)
            if (User.HasClaim(c => c.Type == "Permission" && c.Value == permission))
                return true;

            // Admin role always has access
            if (User.IsInRole("Admin") || User.IsInRole("System.Admin"))
                return true;

            // Database-backed check
            var userGuid = ResolveCurrentUserGuid();
            if (userGuid == Guid.Empty) return false;
            try { return await _authService.AuthorizeAsync(userGuid, permission); }
            catch { return false; }
        }

        private async Task<IActionResult?> RequirePermissionAsync(string permission)
        {
            if (await HasPermissionAsync(permission)) return null;

            var userGuid = ResolveCurrentUserGuid();
            await _auditService.LogAsync(userGuid, ResolveCurrentUserName(),
                "PERMISSION_DENIED", "Permission", permission, null, null,
                new { required = permission, action = HttpContext.Request.Path });

            return StatusCode(403, new { message = $"Access denied. Required permission: {permission}" });
        }


        /// <summary>
        /// Get current active session for the logged-in cashier
        /// </summary>
        [HttpGet("current")]
        public async Task<IActionResult> GetCurrentSession()
        {
            try
            {
                var userGuid = ResolveCurrentUserGuid();
                var session = _cashierStore.GetCurrentSession(userGuid);

                // If not found in memory, try loading from database
                if (session is null)
                {
                    var sessionRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Cashier.CashierSession>();
                    var dbSession = (await sessionRepo.Filter(s => s.CashierUserGuid == userGuid && s.Status == 0))
                        .Data?.
                        OrderByDescending(s => s.OpeningDateUtc)
                        .FirstOrDefault();
                    
                    if (dbSession is null)
                    {
                        return NotFound(new { message = "No active session found" });
                    }
                    
                    // Convert DB entity to state for response and restore to runtime store
                    session = DbSessionToState(dbSession);
                    _cashierStore.RestoreSession(session);
                }

                string? storeName = null;
                if (session.StoreDetailsId.HasValue && session.StoreDetailsId.Value != Guid.Empty)
                {
                    var storeRepo = _repositoriesFactory.GetRepository<StoreDetails>();
                    var store = (await storeRepo.Filter(s => s.Id == session.StoreDetailsId.Value)).Data?.FirstOrDefault();
                    storeName = store?.ArabicName ?? store?.EnglishName;
                }

                return Ok(ToSessionDto(session, storeName));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current session");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Open a new cashier session
        /// </summary>
        [HttpPost("open")]
        public async Task<IActionResult> OpenSession([FromBody] OpenSessionRequest request)
        {
            try
            {
                // ── Permission check ──────────────────────────────
                var permDenied = await RequirePermissionAsync("Cashier.Session.Open");
                if (permDenied != null) return permDenied;

                // Get session repository for database operations
                var sessionRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Cashier.CashierSession>();

                // Validation 1: Opening balance cannot be negative
                if (request.OpeningBalance < 0)
                {
                    return BadRequest(new { message = "Opening balance cannot be negative" });
                }

                // Validation 2: Store is required
                if (!request.StoreDetailsId.HasValue || request.StoreDetailsId.Value == Guid.Empty)
                {
                    return BadRequest(new { message = "Store is required to open session" });
                }

                // Validation 3: Validate store exists
                var storeExists = await ValidateStoreExists(request.StoreDetailsId.Value);
                if (!storeExists)
                {
                    return NotFound(new { message = "Store not found. Please select a valid store." });
                }

                // Validation 4: Check for existing active session (check both memory and database)
                var userGuid = ResolveCurrentUserGuid();
                var existingSession = _cashierStore.GetCurrentSession(userGuid);
                
                if (existingSession == null)
                {
                    var dbSession = (await sessionRepo.Filter(s => s.CashierUserGuid == userGuid && s.Status == 0))
                        .Data?.OrderByDescending(s => s.OpeningDateUtc).FirstOrDefault();
                    if (dbSession != null) existingSession = DbSessionToState(dbSession);
                }
                
                if (existingSession != null)
                {
                    return Conflict(new
                    {
                        message = "You already have an active session. Please close it first.",
                        sessionId = existingSession.Id,
                        sessionNumber = existingSession.SessionNumber,
                        openingDate = existingSession.OpeningDateUtc,
                        storeDetailsId = existingSession.StoreDetailsId
                    });
                }

                // ── Terminal locking ──────────────────────────────
                if (!string.IsNullOrWhiteSpace(request.TerminalId))
                {
                    var terminalActive = (await sessionRepo.Filter(s => s.TerminalId == request.TerminalId && s.Status == 0))
                        .Data?.Any() == true;

                    if (terminalActive)
                    {
                        // Check if the user has multi-terminal override permission
                        var hasOverride = await HasPermissionAsync("Cashier.Session.MultiTerminalOverride");
                        if (!hasOverride)
                        {
                            await _auditService.LogAsync(userGuid, ResolveCurrentUserName(),
                                "MULTI_TERMINAL_CONFLICT", "CashierSession", request.TerminalId,
                                null, request.StoreDetailsId,
                                new { terminalId = request.TerminalId });

                            return Conflict(new { message = $"Terminal '{request.TerminalId}' already has an active session. HTTP 409 Conflict." });
                        }
                    }
                }

                // Sync session ID counter with database
                var allSessions = (await sessionRepo.Filter(s => s.SessionId > 0)).Data?.ToList();
                var maxSessionId = allSessions != null && allSessions.Any() ? allSessions.Max(s => s.SessionId) : 0;
                _cashierStore.SyncSessionIdCounter(maxSessionId);

                string sessionNumberToUse;
                if (!string.IsNullOrWhiteSpace(request.SessionNumber))
                {
                    // Check permission
                    var hasCustomNumPermission = await HasPermissionAsync("Cashier.Session.SetCustomNumber");
                    if (!hasCustomNumPermission)
                    {
                        return StatusCode(403, new { message = "You do not have permission to set a custom session number." });
                    }

                    // Check validation: non-whitespace, not longer than 50 chars
                    var trimmedNum = request.SessionNumber.Trim();
                    if (trimmedNum.Length < 1)
                    {
                        return BadRequest(new { message = "Session number cannot be empty." });
                    }
                    if (trimmedNum.Length > 50)
                    {
                        return BadRequest(new { message = "Session number is too long (maximum 50 characters)." });
                    }

                    // Check uniqueness
                    var duplicateExists = allSessions != null && allSessions.Any(s => 
                        s.SessionNumber.Trim().Equals(trimmedNum, StringComparison.OrdinalIgnoreCase) ||
                        CleanSessionNumber(s.SessionNumber).Equals(trimmedNum, StringComparison.OrdinalIgnoreCase) ||
                        CleanSessionNumber(s.SessionNumber).Equals(CleanSessionNumber(trimmedNum), StringComparison.OrdinalIgnoreCase)
                    );
                    if (duplicateExists)
                    {
                        return BadRequest(new { message = $"رقم الوردية '{CleanSessionNumber(trimmedNum)}' مستخدم بالفعل. يرجى اختيار رقم آخر." });
                    }

                    sessionNumberToUse = trimmedNum;
                }
                else
                {
                    // Generate next serial number
                    int nextSeq = 1;
                    if (allSessions != null && allSessions.Any())
                    {
                        var numericValues = allSessions
                            .Select(s => {
                                if (int.TryParse(s.SessionNumber, out int val)) return val;
                                var segments = s.SessionNumber.Split('-');
                                if (segments.Length > 0 && int.TryParse(segments.Last(), out int lastSegmentVal))
                                {
                                    return lastSegmentVal;
                                }
                                var digits = new string(s.SessionNumber.Where(char.IsDigit).ToArray());
                                if (int.TryParse(digits, out int parsedDigits)) return parsedDigits;
                                return 0;
                            })
                            .ToList();
                        
                        if (numericValues.Any())
                        {
                            nextSeq = numericValues.Max() + 1;
                        }
                        else
                        {
                            nextSeq = allSessions.Count() + 1;
                        }
                    }

                    sessionNumberToUse = nextSeq.ToString("D5"); // e.g. "00001"

                    // Ensure uniqueness
                    while (allSessions != null && allSessions.Any(s => 
                        s.SessionNumber.Equals(sessionNumberToUse, StringComparison.OrdinalIgnoreCase) ||
                        CleanSessionNumber(s.SessionNumber).Equals(sessionNumberToUse, StringComparison.OrdinalIgnoreCase)
                    ))
                    {
                        nextSeq++;
                        sessionNumberToUse = nextSeq.ToString("D5");
                    }
                }

                // Open session
                var hasSetAlertLimitPermission = User.IsInRole("Admin") || User.IsInRole("System.Admin") || User.HasClaim(c => c.Type == "Permission" && c.Value == "Cashier.Session.SetAlertLimit");
                int alertLimit = 10; // Default limit
                if (hasSetAlertLimitPermission && request.StockAlertLimit.HasValue && request.StockAlertLimit.Value > 0)
                {
                    alertLimit = request.StockAlertLimit.Value;
                }

                var userName = ResolveCurrentUserName();
                var session = _cashierStore.OpenSession(userGuid, userName, request.OpeningBalance, request.TreasuryId, request.StoreDetailsId.Value, sessionNumberToUse, alertLimit);

                // Persist session to database
                try
                {
                    var dbSessionNew = new SmartAndPro.ERP.Domains.Entities.Cashier.CashierSession
                    {
                        SessionId = session.Id,
                        SessionNumber = session.SessionNumber,
                        CashierUserGuid = session.CashierUserGuid,
                        CashierUserName = session.CashierUserName,
                        OpeningDateUtc = session.OpeningDateUtc,
                        OpeningBalance = session.OpeningBalance,
                        Status = session.Status,
                        TreasuryId = session.TreasuryId,
                        StoreDetailsId = session.StoreDetailsId,
                        TerminalId = request.TerminalId?.Trim(),
                        StockAlertLimit = session.StockAlertLimit,
                        TotalSales = 0m, TotalRefunds = 0m, TotalCash = 0m,
                        TotalCard = 0m, TotalCredit = 0m, TransactionsCount = 0,
                        TotalCashIn = 0m, TotalCashOut = 0m, TotalTax = 0m, TotalDiscounts = 0m
                    };
                    await sessionRepo.Add(dbSessionNew);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to persist session {SessionId} to database", session.Id);
                }

                _logger.LogInformation(
                    "Session {SessionNumber} opened by {UserName} (GUID: {UserGuid}) store {StoreId} terminal {TerminalId}",
                    session.SessionNumber, userName, userGuid, request.StoreDetailsId.Value, request.TerminalId);

                await _auditService.LogAsync(userGuid, userName, "SESSION_OPENED", "CashierSession",
                    session.Id.ToString(), session.Id, request.StoreDetailsId,
                    new { session.SessionNumber, request.OpeningBalance, request.TreasuryId, request.TerminalId });

                string? storeName = null;
                if (request.StoreDetailsId.HasValue && request.StoreDetailsId.Value != Guid.Empty)
                {
                    var storeRepo = _repositoriesFactory.GetRepository<StoreDetails>();
                    var store = (await storeRepo.Filter(s => s.Id == request.StoreDetailsId.Value)).Data?.FirstOrDefault();
                    storeName = store?.ArabicName ?? store?.EnglishName;
                }

                return Ok(ToSessionDto(session, storeName));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error opening session");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Validate if store exists in the system
        /// </summary>
        private async Task<bool> ValidateStoreExists(Guid storeId)
        {
            try
            {
                var storeRepo = _repositoriesFactory.GetRepository<StoreDetails>();
                var result = await storeRepo.Filter(s => s.Id == storeId);
                return result?.Data?.Any() == true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error validating store {StoreId}", storeId);
                return false;
            }
        }

        /// <summary>
        /// Close cashier session — enforces ownership + supervisor approval for large discrepancies
        /// </summary>
        [HttpPost("{sessionId}/close")]
        public async Task<IActionResult> CloseSession(int sessionId, [FromBody] CloseSessionRequest request)
        {
            try
            {
                // ── Permission check ──────────────────────────────
                var permDenied = await RequirePermissionAsync("Cashier.Session.Close");
                if (permDenied != null) return permDenied;

                var userGuid = ResolveCurrentUserGuid();
                var userName = ResolveCurrentUserName();

                var sessionRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Cashier.CashierSession>();

                // Load session from memory or DB
                var dbSession = (await sessionRepo.Filter(s => s.SessionId == sessionId)).Data?.FirstOrDefault();
                if (dbSession == null)
                    return NotFound(new { message = "Session not found" });

                if (dbSession.Status == 1)
                    return Ok(ToSessionDto(DbSessionToState(dbSession)));

                // ── Session ownership validation ──────────────────
                if (dbSession.CashierUserGuid != userGuid)
                {
                    var hasSupervisorClose = await HasPermissionAsync("Cashier.Session.SupervisorClose");
                    var hasCloseAny = await HasPermissionAsync("Cashier.Session.CloseAny");
                    if (!hasSupervisorClose && !hasCloseAny)
                    {
                        await _auditService.LogAsync(userGuid, userName, "PERMISSION_DENIED",
                            "CashierSession", sessionId.ToString(), sessionId, dbSession.StoreDetailsId,
                            new { reason = "Session owned by another user", ownerId = dbSession.CashierUserGuid });
                        return StatusCode(403, new { message = "You can only close your own session." });
                    }
                }

                // ── Server-side cash reconciliation ──────────────
                var expectedCash = dbSession.OpeningBalance + dbSession.TotalCash
                                   + dbSession.TotalCashIn - dbSession.TotalCashOut;
                var discrepancy = Math.Abs(request.ActualBalance - expectedCash);

                // ── Supervisor approval for large discrepancies ───
                if (discrepancy > DiscrepancyThreshold)
                {
                    if (request.SupervisorApproval == null)
                    {
                        return UnprocessableEntity(new
                        {
                            message = "Cash discrepancy exceeds allowed threshold. Supervisor approval is required.",
                            expectedCash,
                            actualCash = request.ActualBalance,
                            discrepancy,
                            threshold = DiscrepancyThreshold,
                            requiresSupervisorApproval = true
                        });
                    }

                    // Validate supervisor
                    if (string.IsNullOrWhiteSpace(request.SupervisorApproval.SupervisorId) ||
                        string.IsNullOrWhiteSpace(request.SupervisorApproval.ApprovalPasscode))
                        return BadRequest(new { message = "Supervisor ID and passcode are required." });

                    if (!Guid.TryParse(request.SupervisorApproval.SupervisorId, out var supervisorGuid))
                        return BadRequest(new { message = "Invalid supervisor ID format." });

                    // ── Verify supervisor's actual password (prevents empty/fake passcode bypass) ──
                    var userRepo = _repositoriesFactory.GetRepository<User>();
                    var supervisorResult = await userRepo.Filter(u => u.Id == supervisorGuid);
                    var supervisor = supervisorResult?.Data?.FirstOrDefault();
                    if (supervisor == null)
                    {
                        await _auditService.LogAsync(userGuid, userName, "SUPERVISOR_NOT_FOUND",
                            "CashierSession", sessionId.ToString(), sessionId, dbSession.StoreDetailsId,
                            new { supervisorId = supervisorGuid });
                        return BadRequest(new { message = "Supervisor not found." });
                    }

                    var hashedPasscode = SmartAndPro.ERP.Application.Services.Auth.Security.SecurityService.ComputeHMAC(request.SupervisorApproval.ApprovalPasscode, "UserID");
                    var passwordValid = supervisor.Password == hashedPasscode;
                    if (!passwordValid)
                    {
                        await _auditService.LogAsync(supervisorGuid, request.SupervisorApproval.SupervisorId,
                            "SUPERVISOR_PASSCODE_FAILED", "CashierSession", sessionId.ToString(),
                            sessionId, dbSession.StoreDetailsId,
                            new { reason = "Invalid supervisor passcode", requestedBy = userGuid });
                        return StatusCode(401, new { message = "Invalid supervisor passcode. Override denied.", messageAr = "كلمة مرور المشرف غير صحيحة. تم رفض التجاوز." });
                    }

                    var supervisorHasPermission = await _authService.AuthorizeAsync(supervisorGuid, "Cashier.Session.SupervisorClose");
                    if (!supervisorHasPermission)
                    {
                        await _auditService.LogAsync(userGuid, userName, "PERMISSION_DENIED",
                            "SupervisorApproval", sessionId.ToString(), sessionId, dbSession.StoreDetailsId,
                            new { supervisorId = supervisorGuid, reason = "Supervisor lacks SupervisorClose permission" });
                        return StatusCode(403, new { message = "Supervisor does not have the required permission." });
                    }

                    // Log supervisor override
                    await _auditService.LogAsync(supervisorGuid, request.SupervisorApproval.SupervisorId,
                        "SUPERVISOR_OVERRIDE", "CashierSession", sessionId.ToString(),
                        sessionId, dbSession.StoreDetailsId,
                        new { discrepancy, expectedCash, actualCash = request.ActualBalance, requestedBy = userGuid });
                }

                // ── Close session ─────────────────────────────────
                var session = _cashierStore.CloseSession(sessionId, request.ActualBalance, request.Notes, userGuid, userName);

                dbSession.Status = 1;
                dbSession.ClosingDateUtc = DateTime.UtcNow;
                dbSession.ActualBalance = request.ActualBalance;
                dbSession.Notes = request.Notes?.Trim();
                dbSession.ClosedByUserGuid = userGuid;
                dbSession.ClosedByUserName = userName;

                if (session != null)
                {
                    dbSession.TotalSales = session.TotalSales;
                    dbSession.TotalRefunds = session.TotalRefunds;
                    dbSession.TotalCash = session.TotalCash;
                    dbSession.TotalCard = session.TotalCard;
                    dbSession.TotalCredit = session.TotalCredit;
                    dbSession.TransactionsCount = session.TransactionsCount;
                }

                await sessionRepo.Update(dbSession);
                session ??= DbSessionToState(dbSession);

                await _auditService.LogAsync(userGuid, userName, "SESSION_CLOSED", "CashierSession",
                    sessionId.ToString(), sessionId, dbSession.StoreDetailsId,
                    new { request.ActualBalance, expectedCash, discrepancy = request.ActualBalance - expectedCash, request.Notes });

                // ── Cash Discrepancy Journal Entry ────────────────
                var finalDiscrepancy = request.ActualBalance - expectedCash;
                if (Math.Abs(finalDiscrepancy) > 0)
                {
                    try
                    {
                        await _auditService.LogAsync(userGuid, userName, "CASH_DISCREPANCY_JOURNAL",
                            "CashierSession", sessionId.ToString(), sessionId, dbSession.StoreDetailsId,
                            new
                            {
                                discrepancy = finalDiscrepancy,
                                expectedCash,
                                actualCash = request.ActualBalance,
                                requiresAccountingEntry = Math.Abs(finalDiscrepancy) > DiscrepancyThreshold,
                                approvalStatus = Math.Abs(finalDiscrepancy) > DiscrepancyThreshold
                                    ? "SUPERVISOR_APPROVED"
                                    : "WITHIN_THRESHOLD",
                                journalNote = finalDiscrepancy > 0
                                    ? "Cash surplus: actual exceeds expected"
                                    : "Cash shortage: actual below expected"
                            });
                    }
                    catch (Exception journalEx)
                    {
                        _logger.LogWarning(journalEx, "Failed to write cash discrepancy journal for session {SessionId}", sessionId);
                    }
                }

                return Ok(ToSessionDto(session));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing session");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Close current active cashier session (alias endpoint)
        /// </summary>
        [HttpPost("close")]
        public async Task<IActionResult> CloseCurrentSession([FromBody] CloseSessionRequest request)
        {
            try
            {
                var permDenied = await RequirePermissionAsync("Cashier.Session.Close");
                if (permDenied != null) return permDenied;

                var userGuid = ResolveCurrentUserGuid();
                var activeSession = _cashierStore.GetCurrentSession(userGuid);

                if (activeSession == null)
                {
                    var sessionRepo2 = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Cashier.CashierSession>();
                    var dbSession2 = (await sessionRepo2.Filter(s => s.CashierUserGuid == userGuid && s.Status == 0))
                        .Data?.OrderByDescending(s => s.OpeningDateUtc).FirstOrDefault();
                    if (dbSession2 != null) activeSession = DbSessionToState(dbSession2);
                }

                if (activeSession is null)
                    return NotFound(new { message = "No active session found" });

                return await CloseSession(activeSession.Id, request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing current session");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Cash in / cash out operation
        /// POST /api/cashier/session/{sessionId}/cash-action
        /// </summary>
        [HttpPost("{sessionId}/cash-action")]
        public async Task<IActionResult> CashAction(int sessionId, [FromBody] CashActionRequest request)
        {
            try
            {
                var permDenied = await RequirePermissionAsync("Cashier.Session.CashAction");
                if (permDenied != null) return permDenied;

                var userGuid = ResolveCurrentUserGuid();
                var userName = ResolveCurrentUserName();

                if (request.Amount <= 0)
                    return BadRequest(new { message = "Amount must be greater than zero." });

                var sessionRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Cashier.CashierSession>();
                var dbSession = (await sessionRepo.Filter(s => s.SessionId == sessionId)).Data?.FirstOrDefault();

                if (dbSession == null)
                    return NotFound(new { message = "Session not found" });

                if (dbSession.Status == 1)
                    return Conflict(new { message = "Session is already closed." });

                // ── Session ownership validation ──────────────────
                if (dbSession.CashierUserGuid != userGuid)
                {
                    await _auditService.LogAsync(userGuid, userName, "PERMISSION_DENIED",
                        "CashAction", sessionId.ToString(), sessionId, dbSession.StoreDetailsId,
                        new { reason = "Not session owner" });
                    return StatusCode(403, new { message = "You can only perform cash actions on your own session." });
                }

                var actionType = request.ActionType?.ToUpperInvariant();
                if (actionType == "CASH_IN")
                    dbSession.TotalCashIn += request.Amount;
                else if (actionType == "CASH_OUT")
                    dbSession.TotalCashOut += request.Amount;
                else
                    return BadRequest(new { message = "ActionType must be 'CASH_IN' or 'CASH_OUT'." });

                await sessionRepo.Update(dbSession);

                var auditEvent = actionType == "CASH_IN" ? "CASH_IN" : "CASH_OUT";
                await _auditService.LogAsync(userGuid, userName, auditEvent, "CashierSession",
                    sessionId.ToString(), sessionId, dbSession.StoreDetailsId,
                    new { request.Amount, request.Notes, actionType });

                return Ok(new
                {
                    sessionId,
                    actionType,
                    amount = request.Amount,
                    notes = request.Notes,
                    totalCashIn = dbSession.TotalCashIn,
                    totalCashOut = dbSession.TotalCashOut,
                    message = "Cash action recorded successfully."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing cash action for session {SessionId}", sessionId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get session summary
        /// </summary>
        [HttpGet("{sessionId}/summary")]
        public async Task<IActionResult> GetSessionSummary(int sessionId)
        {
            try
            {
                var permDenied = await RequirePermissionAsync("Cashier.Reports.View");
                if (permDenied != null) return permDenied;

                var summary = _cashierStore.BuildSessionSummary(sessionId);
                return Ok(new
                {
                    sessionId = summary.SessionId,
                    totalTransactions = summary.TotalTransactions,
                    totalSalesAmount = summary.TotalSalesAmount,
                    totalRefundsAmount = summary.TotalRefundsAmount,
                    cashAmount = summary.CashAmount,
                    cardAmount = summary.CardAmount,
                    creditAmount = summary.CreditAmount,
                    expectedCash = summary.ExpectedCash,
                    actualCash = summary.ActualCash,
                    difference = summary.Difference
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting session summary");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get full session report (session info + statistics + invoices + refunds)
        /// </summary>
        [HttpGet("{sessionId}/report")]
        public async Task<IActionResult> GetSessionReport(int sessionId)
        {
            try
            {
                var permDenied = await RequirePermissionAsync("Cashier.Reports.View");
                if (permDenied != null) return permDenied;
                
                var sessionRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Cashier.CashierSession>();
                var dbSession = (await sessionRepo.Filter(s => s.SessionId == sessionId)).Data?.FirstOrDefault();
                if (dbSession is null)
                    return NotFound(new { message = "Session not found" });

                var session = DbSessionToState(dbSession);

                // Load payments from DB via CashierPayment
                var paymentRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Cashier.CashierPayment>();
                var paymentsQuery = await paymentRepo.Filter(p => p.SessionId == sessionId);
                var payments = paymentsQuery.Data?.ToList() ?? new List<SmartAndPro.ERP.Domains.Entities.Cashier.CashierPayment>();

                var invoiceIds = payments.Select(p => p.InvoiceId).Where(id => id > 0).Distinct().ToList();
                var saleInvoiceRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Invoice.Sale.SaleInvoice>();
                
                List<SmartAndPro.ERP.Domains.Entities.Invoice.Sale.SaleInvoice> dbInvoices = new();
                if (invoiceIds.Count > 0)
                {
                    var invoicesQuery = (await saleInvoiceRepo.Filter(i => invoiceIds.Contains(i.Id))).Data;
                    dbInvoices = await invoicesQuery
                        .Include(i => i.CustomerProfile)
                        .Include(i => i.SaleItems)
                        .ToListAsync();
                }

                var invoices = new List<object>();
                var invoiceMap = dbInvoices.ToDictionary(i => i.Id);

                foreach (var payment in payments)
                {
                    if (!invoiceMap.TryGetValue(payment.InvoiceId, out var invoice)) continue;

                    var totalAmount = invoice.TotalPrice > 0 ? invoice.TotalPrice : invoice.NetPrice + invoice.TaxAmount;
                    var paymentMethodName = payment.PaymentMethod switch
                    {
                        0 => "نقدي",
                        1 => "بطاقة",
                        2 => "آجل",
                        _ => "غير محدد"
                    };

                    invoices.Add(new
                    {
                        id = invoice.Id,
                        invoiceNo = invoice.InvoiceNo,
                        invoiceDate = invoice.IssueDate,
                        customerName = invoice.CustomerProfile?.ArabicName ?? invoice.CustomerProfile?.EnglishName ?? "عميل نقدي",
                        totalAmount,
                        paymentMethod = payment.PaymentMethod,
                        paymentMethodName,
                        paidAmount = payment.PaidAmount,
                        changeAmount = payment.ChangeAmount,
                        itemsCount = invoice.SaleItems?.Count ?? 0
                    });
                }

                // Load refunds from DB
                var refundRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Cashier.CashierRefund>();
                var refundsList = (await refundRepo.Filter(r => r.SessionId == sessionId)).Data?.ToList()
                                  ?? new List<SmartAndPro.ERP.Domains.Entities.Cashier.CashierRefund>();

                var statusName = session.Status switch
                {
                    0 => "مفتوحة",
                    1 => "مغلقة",
                    _ => "غير معروف"
                };

                var totalItemsSold = dbInvoices.Sum(i => i.SaleItems?.Sum(si => (int)si.Quantity) ?? 0);
                if (totalItemsSold == 0) totalItemsSold = invoices.Count;

                var totalSalesAmt = session.TotalSales > 0 ? session.TotalSales : payments.Sum(p => p.PaidAmount);
                var totalRefundsAmt = session.TotalRefunds > 0 ? session.TotalRefunds : refundsList.Sum(r => r.RefundTotalAmount);
                var txCount = session.TransactionsCount > 0 ? session.TransactionsCount : invoices.Count;

                return Ok(new
                {
                    session = new
                    {
                        sessionId = session.Id,
                        sessionNumber = session.SessionNumber,
                        cashierName = session.CashierUserName,
                        openingDate = session.OpeningDateUtc,
                        closingDate = session.ClosingDateUtc,
                        status = session.Status,
                        statusName,
                        openingBalance = session.OpeningBalance,
                        closingBalance = session.ClosingBalance,
                        expectedBalance = session.ExpectedBalance,
                        actualBalance = session.ActualBalance,
                        difference = session.Difference,
                        notes = session.Notes,
                        stockAlertLimit = session.StockAlertLimit
                    },
                    statistics = new
                    {
                        totalTransactions = txCount,
                        totalSalesAmount = totalSalesAmt,
                        totalRefundsAmount = totalRefundsAmt,
                        netSales = totalSalesAmt - totalRefundsAmt,
                        totalCash = session.TotalCash > 0 ? session.TotalCash : payments.Where(p => p.PaymentMethod == 0).Sum(p => p.PaidAmount),
                        totalCard = session.TotalCard > 0 ? session.TotalCard : payments.Where(p => p.PaymentMethod == 1).Sum(p => p.PaidAmount),
                        totalCredit = session.TotalCredit > 0 ? session.TotalCredit : payments.Where(p => p.PaymentMethod == 2).Sum(p => p.PaidAmount),
                        totalItemsSold,
                        averageTransactionValue = txCount > 0
                            ? (totalSalesAmt - totalRefundsAmt) / txCount
                            : 0m
                    },
                    invoices,
                    refunds = refundsList.Select(r => new
                    {
                        id = r.Id,
                        refundNumber = r.RefundNo,
                        refundTotalAmount = r.RefundTotalAmount,
                        refundMethod = r.RefundMethod,
                        status = r.Status,
                        createdAt = r.CreatedAt
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting session report for session {SessionId}", sessionId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Real Z-Report — end-of-day financial reconciliation report
        /// GET /api/cashier/session/{sessionId}/z-report
        /// </summary>
        [HttpGet("{sessionId}/z-report")]
        public async Task<IActionResult> GetZReport(int sessionId)
        {
            try
            {
                var permDenied = await RequirePermissionAsync("Cashier.Reports.ZReport");
                if (permDenied != null) return permDenied;

                // Load session from DB (source of truth)
                var sessionRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Cashier.CashierSession>();
                var dbSession = (await sessionRepo.Filter(s => s.SessionId == sessionId)).Data?.FirstOrDefault();
                if (dbSession == null)
                    return NotFound(new { message = "Session not found" });

                // Load store/branch name
                var storeName = "الفرع الرئيسي";
                if (dbSession.StoreDetailsId.HasValue)
                {
                    var storeRepo = _repositoriesFactory.GetRepository<StoreDetails>();
                    var store = (await storeRepo.Filter(s => s.Id == dbSession.StoreDetailsId.Value)).Data?.FirstOrDefault();
                    if (store != null)
                    {
                        storeName = store.ArabicName ?? store.EnglishName ?? storeName;
                    }
                }

                // Load payments for this session
                var paymentRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Cashier.CashierPayment>();
                var payments = (await paymentRepo.Filter(p => p.SessionId == sessionId)).Data?.ToList()
                               ?? new List<SmartAndPro.ERP.Domains.Entities.Cashier.CashierPayment>();

                // Load refunds for this session
                var refundRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Cashier.CashierRefund>();
                var refunds = (await refundRepo.Filter(r => r.SessionId == sessionId && r.Status == "completed")).Data?.ToList()
                              ?? new List<SmartAndPro.ERP.Domains.Entities.Cashier.CashierRefund>();

                // Server-side calculated totals (never trust frontend values)
                var cashSales   = payments.Where(p => p.PaymentMethod == 0).Sum(p => p.PaidAmount);
                var cardSales   = payments.Where(p => p.PaymentMethod == 1).Sum(p => p.PaidAmount);
                var creditSales = payments.Where(p => p.PaymentMethod == 2).Sum(p => p.PaidAmount);
                var totalSales  = cashSales + cardSales + creditSales;

                if (totalSales == 0 && dbSession.TotalSales > 0)
                {
                    totalSales = dbSession.TotalSales;
                    cashSales = dbSession.TotalCash;
                    cardSales = dbSession.TotalCard;
                    creditSales = dbSession.TotalCredit;
                }

                var refundsTotal = refunds.Sum(r => r.RefundTotalAmount);
                if (refundsTotal == 0 && dbSession.TotalRefunds > 0)
                {
                    refundsTotal = dbSession.TotalRefunds;
                }

                var netSales = totalSales - refundsTotal;

                // Expected cash & reconciliation
                var expectedCash = dbSession.OpeningBalance + cashSales
                                   + dbSession.TotalCashIn - dbSession.TotalCashOut
                                   - refunds.Where(r => r.RefundMethod == "cash" || r.RefundMethod == "0").Sum(r => r.RefundTotalAmount);
                var actualCash = dbSession.ActualBalance ?? (dbSession.Status == 1 ? dbSession.ActualBalance ?? expectedCash : expectedCash);
                var difference = actualCash - expectedCash;

                // Load invoices and items for Category Breakdown and Top Products
                var invoiceIds = payments.Select(p => p.InvoiceId).Where(id => id > 0).Distinct().ToList();
                var saleInvoiceRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Invoice.Sale.SaleInvoice>();
                
                List<SmartAndPro.ERP.Domains.Entities.Invoice.Sale.SaleInvoice> invoices = new();
                if (invoiceIds.Count > 0)
                {
                    var invoicesQuery = (await saleInvoiceRepo.Filter(i => invoiceIds.Contains(i.Id))).Data;
                    invoices = await invoicesQuery
                        .Include(i => i.SaleItems)
                            .ThenInclude(si => si.MainSalesClass)
                        .ToListAsync();
                }

                // Load all categories for fast mapping
                var detailSalesClassRepo = _repositoriesFactory.GetRepository<DetailSalesClass>();
                var allCategories = (await detailSalesClassRepo.GetAll()).Data?.ToList() ?? new List<DetailSalesClass>();
                var catById = allCategories.ToDictionary(c => c.Id);

                var totalTax = invoices.Sum(i => i.TaxAmount > 0 ? i.TaxAmount : i.SaleItems.Sum(si => si.TaxAmount));
                if (totalTax == 0 && dbSession.TotalTax > 0)
                    totalTax = dbSession.TotalTax;

                var totalDiscounts = invoices.Sum(i => i.DiscountAmount > 0 ? i.DiscountAmount : i.SaleItems.Sum(si => si.DiscountAmount));
                if (totalDiscounts == 0 && dbSession.TotalDiscounts > 0)
                    totalDiscounts = dbSession.TotalDiscounts;

                var totalTransactions = invoiceIds.Count > 0 ? invoiceIds.Count : (dbSession.TransactionsCount > 0 ? dbSession.TransactionsCount : payments.Count);

                var categoryMap = new Dictionary<string, (decimal Qty, decimal Amount)>(StringComparer.OrdinalIgnoreCase);
                var productMap = new Dictionary<string, (decimal Qty, decimal Amount)>(StringComparer.OrdinalIgnoreCase);

                var allItems = invoices.SelectMany(i => i.SaleItems).ToList();
                foreach (var item in allItems)
                {
                    var prodName = !string.IsNullOrWhiteSpace(item.Name)
                        ? item.Name
                        : (item.MainSalesClass?.ArabicName ?? item.MainSalesClass?.EnglishName ?? "صنف غير محدد");

                    string catName = "عام";
                    if (item.MainSalesClass != null && catById.TryGetValue(item.MainSalesClass.DetailSalesClassId, out var detailCat))
                    {
                        catName = detailCat.ArabicName ?? detailCat.EnglishName ?? "عام";
                    }
                    else if (item.MainSalesClass != null)
                    {
                        catName = item.MainSalesClass.ArabicName ?? item.MainSalesClass.EnglishName ?? "عام";
                    }

                    var qty = item.Quantity > 0 ? item.Quantity : 1m;
                    var amount = item.TotalPrice > 0 ? item.TotalPrice : (item.Price * qty);

                    if (!categoryMap.ContainsKey(catName))
                        categoryMap[catName] = (0m, 0m);
                    categoryMap[catName] = (categoryMap[catName].Qty + qty, categoryMap[catName].Amount + amount);

                    if (!productMap.ContainsKey(prodName))
                        productMap[prodName] = (0m, 0m);
                    productMap[prodName] = (productMap[prodName].Qty + qty, productMap[prodName].Amount + amount);
                }

                var categoryBreakdown = categoryMap
                    .Select(kv => new
                    {
                        categoryName = kv.Key,
                        name = kv.Key,
                        soldQty = kv.Value.Qty,
                        qty = kv.Value.Qty,
                        itemCount = kv.Value.Qty,
                        amount = kv.Value.Amount,
                        totalAmount = kv.Value.Amount
                    })
                    .OrderByDescending(c => c.amount)
                    .ToList();

                var topProducts = productMap
                    .OrderByDescending(kv => kv.Value.Amount)
                    .Take(10)
                    .Select(kv => new
                    {
                        productName = kv.Key,
                        name = kv.Key,
                        qty = kv.Value.Qty,
                        quantitySold = kv.Value.Qty,
                        revenue = kv.Value.Amount,
                        totalRevenue = kv.Value.Amount
                    })
                    .ToList();

                var report = new
                {
                    reportDate = dbSession.ClosingDateUtc ?? DateTime.UtcNow,
                    sessionNumber = dbSession.SessionNumber,
                    cashierName = dbSession.CashierUserName,
                    storeName,
                    branchName = storeName,
                    openTime = dbSession.OpeningDateUtc,
                    closeTime = dbSession.ClosingDateUtc,
                    openingBalance = dbSession.OpeningBalance,
                    totalSales,
                    totalTax,
                    totalDiscounts,
                    netSales,
                    totalTransactions,
                    refundsTotal,
                    totalRefunds = refundsTotal,
                    refundCount = refunds.Count,
                    paymentBreakdown = new
                    {
                        cashSales,
                        cardSales,
                        creditSales,
                        otherSales = 0m
                    },
                    cashReconciliation = new
                    {
                        openingCash = dbSession.OpeningBalance,
                        cashIn = dbSession.TotalCashIn,
                        cashOut = dbSession.TotalCashOut,
                        expectedCash,
                        actualCash,
                        difference
                    },
                    categoryBreakdown,
                    topProducts
                };

                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Z-Report for session {SessionId}", sessionId);
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get all sessions with pagination (from database)
        /// </summary>
        [HttpGet("~/api/cashier/sessions")]
        [HttpGet]
        public async Task<IActionResult> GetSessions([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            try
            {
                var sessionRepo = _repositoriesFactory.GetRepository<SmartAndPro.ERP.Domains.Entities.Cashier.CashierSession>();
                var canViewAll = await HasPermissionAsync("Cashier.Session.ViewAll");
                var currentUserGuid = ResolveCurrentUserGuid();

                var allSessionsResult = (canViewAll || currentUserGuid == Guid.Empty)
                    ? await sessionRepo.Filter(s => true)
                    : await sessionRepo.Filter(s => s.CashierUserGuid == currentUserGuid);

                var allSessions = allSessionsResult.Data?
                    .OrderByDescending(s => s.OpeningDateUtc)
                    .ToList() ?? new List<SmartAndPro.ERP.Domains.Entities.Cashier.CashierSession>();

                var storeRepo = _repositoriesFactory.GetRepository<StoreDetails>();
                var allStoresResult = await storeRepo.Filter(s => true);
                var storeMap = allStoresResult.Data?
                    .Where(s => s.Id != Guid.Empty)
                    .ToDictionary(s => s.Id, s => s.ArabicName ?? s.EnglishName ?? string.Empty) 
                    ?? new Dictionary<Guid, string>();

                var total = allSessions.Count;
                var pagedSessions = allSessions
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(db =>
                    {
                        var state = DbSessionToState(db);
                        var storeName = state.StoreDetailsId.HasValue && storeMap.TryGetValue(state.StoreDetailsId.Value, out var name)
                            ? name
                            : null;
                        return ToSessionDto(state, storeName);
                    })
                    .ToList();

                var response = new
                {
                    items = pagedSessions,
                    total,
                    pageNumber,
                    pageSize
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting sessions");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// Get session invoices
        /// </summary>
        [HttpGet("{sessionId}/invoices")]
        public async Task<IActionResult> GetSessionInvoices(int sessionId)
        {
            try
            {
                var invoiceIds = _cashierStore.GetSessionInvoiceIds(sessionId);
                var invoices = new List<object>();

                foreach (var invoiceId in invoiceIds)
                {
                    var invoice = await _salesInvoicesService.GetByIdAsync(invoiceId);
                    if (invoice is null)
                    {
                        continue;
                    }

                    var payment = _cashierStore.GetPayment(invoiceId);
                    var totalAmount = invoice.TotalPrice > 0 ? invoice.TotalPrice : invoice.NetPrice + invoice.TaxAmount;

                    invoices.Add(new
                    {
                        id = invoice.Id,
                        invoiceNo = invoice.InvoiceNo,
                        invoiceDate = invoice.IssueDate,
                        customerName = invoice.CustomerProfile?.ArabicName ?? invoice.CustomerProfile?.EnglishName ?? "Walk-in Customer",
                        totalAmount,
                        paymentMethod = payment?.PaymentMethod ?? 0,
                        paidAmount = payment?.PaidAmount ?? totalAmount,
                        changeAmount = payment?.ChangeAmount ?? 0m
                    });
                }

                return Ok(invoices);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting session invoices");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        private Guid ResolveCurrentUserGuid()
        {
            var rawId =
                User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                User.FindFirstValue(ClaimTypes.Name) ??
                User.FindFirstValue("sub");

            return Guid.TryParse(rawId, out var parsed)
                ? parsed
                : Guid.Parse("00000000-0000-0000-0000-000000000001");
        }

        private string ResolveCurrentUserName()
        {
            var userName =
                User.FindFirstValue(ClaimTypes.GivenName) ??
                User.FindFirstValue(ClaimTypes.Name) ??
                User.Identity?.Name ??
                "Cashier";

            return string.IsNullOrWhiteSpace(userName) ? "Cashier" : userName.Trim();
        }

        private object ToSessionDto(CashierSessionState session, string? storeName = null)
        {
            return new
            {
                id = session.Id,
                sessionNumber = session.SessionNumber,
                cashierUserId = session.CashierUserId,
                cashierUserName = session.CashierUserName,
                openingDate = session.OpeningDateUtc,
                closingDate = session.ClosingDateUtc,
                openingBalance = session.OpeningBalance,
                closingBalance = session.ClosingBalance,
                expectedBalance = session.ExpectedBalance,
                actualBalance = session.ActualBalance,
                difference = session.Difference,
                status = session.Status,
                notes = session.Notes,
                treasuryId = session.TreasuryId,
                storeDetailsId = session.StoreDetailsId,
                storeName = storeName,
                totalSales = session.TotalSales,
                totalRefunds = session.TotalRefunds,
                totalCash = session.TotalCash,
                totalCard = session.TotalCard,
                totalCredit = session.TotalCredit,
                stockAlertLimit = session.StockAlertLimit
            };
        }

        private bool HasCashierAccess()
        {
            return User.IsInRole("Admin") || User.IsInRole("Cashier");
        }

        private string CleanSessionNumber(string sessionNumber)
        {
            if (string.IsNullOrWhiteSpace(sessionNumber)) return string.Empty;
            var segments = sessionNumber.Split('-');
            if (segments.Length > 0 && int.TryParse(segments.Last(), out int lastSegmentVal))
            {
                return lastSegmentVal.ToString("D5");
            }
            return sessionNumber;
        }

        private CashierSessionState DbSessionToState(SmartAndPro.ERP.Domains.Entities.Cashier.CashierSession dbSession)
        {
            var expectedCash = dbSession.OpeningBalance + dbSession.TotalCash
                               + dbSession.TotalCashIn - dbSession.TotalCashOut;
            return new CashierSessionState
            {
                Id = dbSession.SessionId,
                SessionNumber = CleanSessionNumber(dbSession.SessionNumber),
                CashierUserId = 0,
                CashierUserGuid = dbSession.CashierUserGuid,
                CashierUserName = dbSession.CashierUserName,
                OpeningDateUtc = dbSession.OpeningDateUtc,
                ClosingDateUtc = dbSession.ClosingDateUtc,
                OpeningBalance = dbSession.OpeningBalance,
                ClosingBalance = dbSession.ActualBalance,
                ExpectedBalance = expectedCash,
                ActualBalance = dbSession.ActualBalance,
                Difference = dbSession.ActualBalance.HasValue ? dbSession.ActualBalance.Value - expectedCash : null,
                Status = dbSession.Status,
                Notes = dbSession.Notes,
                ClosedByUserGuid = dbSession.ClosedByUserGuid,
                ClosedByUserName = dbSession.ClosedByUserName,
                TreasuryId = dbSession.TreasuryId,
                StoreDetailsId = dbSession.StoreDetailsId,
                TotalSales = dbSession.TotalSales,
                TotalRefunds = dbSession.TotalRefunds,
                TotalCash = dbSession.TotalCash,
                TotalCard = dbSession.TotalCard,
                TotalCredit = dbSession.TotalCredit,
                TransactionsCount = dbSession.TransactionsCount,
                StockAlertLimit = dbSession.StockAlertLimit
            };
        }
    }

    #region Request Models

    public class OpenSessionRequest
    {
        public decimal OpeningBalance { get; set; }
        public int? TreasuryId { get; set; }
        public Guid? StoreDetailsId { get; set; }
        /// <summary>POS terminal identifier for locking — e.g. "POS-01"</summary>
        public string? TerminalId { get; set; }
        public string? SessionNumber { get; set; }
        public int? StockAlertLimit { get; set; }
    }

    public class CloseSessionRequest
    {
        public decimal ActualBalance { get; set; }
        public string? Notes { get; set; }
        /// <summary>Required when cash discrepancy exceeds allowed threshold.</summary>
        public SupervisorApprovalDto? SupervisorApproval { get; set; }
    }

    public class SupervisorApprovalDto
    {
        public string SupervisorId { get; set; } = string.Empty;
        public string ApprovalPasscode { get; set; } = string.Empty;
    }

    public class CashActionRequest
    {
        /// <summary>"CASH_IN" or "CASH_OUT"</summary>
        public string ActionType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Notes { get; set; }
    }

    #endregion
}

