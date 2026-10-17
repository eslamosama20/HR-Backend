using SmartAndPro.ERP.SharedKernel.BusinessRule;

namespace SmartAndPro.ERP.SharedKernel.Exceptions;

public class BusinessRuleViolationException: Exception
{
    public List<BusinessRuleError> Errors { get; }

    public BusinessRuleViolationException(List<BusinessRuleError> errors)
        : base("One or more business rules were violated.")
    {
        Errors = errors;
    }
}