namespace SmartAndPro.ERP.Application.Common.Constants;

public static class ErrorMessageConstants
{
    public static class ImportErrorCodes
    {
        public const string DuplicateName = "DUPLICATE_NAME";
        public const string MissingRequiredField = "MISSING_REQUIRED_FIELD";
        public const string InvalidFormat = "INVALID_FORMAT";
        public const string DatabaseConstraint = "DATABASE_CONSTRAINT";
        public const string TransactionFailed = "TRANSACTION_FAILED";
    }
}
