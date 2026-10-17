using SmartAndPro.ERP.SharedKernel.Enums;

namespace SmartAndPro.ERP.SharedKernel.BusinessRule;

public class BusinessRuleError
{
    public string RuleId { get; }
    public string ErrorMessage { get; }
    public string ErrorCode { get; } // Optional code for specific rules
    public SeverityLevel Severity { get; } // Severity level of the violation

    public BusinessRuleError(string ruleId, string errorMessage, string errorCode = null, SeverityLevel severity = SeverityLevel.Error)
    {
        RuleId = ruleId;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
        Severity = severity;
    }
}