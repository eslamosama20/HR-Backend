namespace SmartAndPro.ERP.Application.Services.Invoice.Sale;

/// <summary>
/// Optional parity settings for the server-side Print 1 replica
/// (<see cref="InvoicePrintType.Format1Server"/>). These mirror the frontend
/// localStorage print settings + active language that the browser Print 1
/// template reads, so the server document renders the exact same output.
/// When null, existing formats (A4 / thermal) keep their current behavior.
/// </summary>
public sealed record Print1ServerOptions(
    string Language = "ar",
    string? SellerName = null,
    bool ShowStamp = true,
    bool ShowCompanySignature = true,
    bool ShowCustomerSignature = true,
    IReadOnlyCollection<Guid>? UncheckedPolicyIds = null,
    IReadOnlyCollection<Guid>? UncheckedBankIds = null,
    string? TimeZone = null,
    string? Orientation = null);
