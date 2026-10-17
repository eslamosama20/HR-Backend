namespace SmartAndPro.ERP.SharedKernel.BusinessRule;
public class BusinessRuleResult
{
    public bool IsSuccess { get; }
    public List<BusinessRuleError> Errors { get; }

    private BusinessRuleResult(bool isSuccess, List<BusinessRuleError> errors = null)
    {
        IsSuccess = isSuccess;
        Errors = errors ?? new List<BusinessRuleError>();
    }

    public static BusinessRuleResult Success() => new BusinessRuleResult(true);
    public static BusinessRuleResult Failure(List<BusinessRuleError> errors) => new BusinessRuleResult(false, errors);
}