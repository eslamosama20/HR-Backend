using SmartAndPro.ERP.SharedKernel.BusinessRule;

namespace SmartAndPro.ERP.Domains.BusinessRulesEnforcer;

public abstract class EnforcerBase<T> : IEnforcer<T> 
{
    protected BusinessRuleResult EnsureRules(IEnumerable<(Func<bool> rule, BusinessRuleError error)> rules)
    {
        var errors = new List<BusinessRuleError>();

        foreach (var (rule, error) in rules)
        {
            if (!rule())
            {
                errors.Add(error);
            }
        }

        return errors.Count != 0 ? BusinessRuleResult.Failure(errors) : BusinessRuleResult.Success();
    }

    protected async Task<BusinessRuleResult> EnsureRulesAsync(IEnumerable<(Func<Task<bool>> rule, BusinessRuleError error)> rules)
    {
        var errors = new List<BusinessRuleError>();

        foreach (var (rule, error) in rules)
        {
            if (!await rule())
            {
                errors.Add(error);
            }
        }

        return errors.Any() ? BusinessRuleResult.Failure(errors) : BusinessRuleResult.Success();
    }

    public abstract BusinessRuleResult EnforceRules(T entity);
    
    public virtual Task<BusinessRuleResult> EnforceRulesAsync(T entity)
    {
        // By default, call the synchronous version and return it as a completed task
        return Task.FromResult(EnforceRules(entity));
    }
}