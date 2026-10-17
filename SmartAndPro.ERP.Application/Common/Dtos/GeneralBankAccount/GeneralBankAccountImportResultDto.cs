namespace SmartAndPro.ERP.Application.Common.Dtos.GeneralBankAccount
{
    /// <summary>
    /// Returned by POST /api/GeneralBankAccount/import
    /// </summary>
    public class GeneralBankAccountImportResultDto
    {
        public int Imported { get; set; }
        public int Updated { get; set; }
        public List<GeneralBankAccountImportFailureDto> Failed { get; set; } = new();
    }

    public class GeneralBankAccountImportFailureDto
    {
        public int Row { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string? ErrorCode { get; set; }
    }
}
