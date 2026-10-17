namespace SmartAndPro.ERP.Application.Common.Dtos.Accounting.MainAccount;

public class MainAccountLookupDto
{
    public Guid Id { get; set; }
    public string ReferenceCode { get; set; } = string.Empty;
    public string ArabicName { get; set; } = string.Empty;
    public string EnglishName { get; set; } = string.Empty;
}
