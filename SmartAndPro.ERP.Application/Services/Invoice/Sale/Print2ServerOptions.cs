namespace SmartAndPro.ERP.Application.Services.Invoice.Sale;

/// <summary>
/// Settings for Server PDF Print 2 (A4Portrait, A4Landscape, etc. generated via QuestPDF).
/// Controls whether Terms & Conditions and Bank Accounts should be rendered in the document.
/// Default is false for both.
/// </summary>
public sealed record Print2ServerOptions(
    bool ShowTerms = false,
    bool ShowBanks = false,
    IReadOnlyCollection<Guid>? UncheckedPolicyIds = null,
    IReadOnlyCollection<Guid>? UncheckedBankIds = null,
    string? TimeZone = null);
