using SmartAndPro.ERP.Domains.BusinessRulesEnforcer;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.SharedKernel.BusinessRule;
using SmartAndPro.ERP.SharedKernel.Enums;

namespace SmartAndPro.ERP.Application.Enforcers;

public class TransactionEnforcer : EnforcerBase<Transaction>
{
    public override BusinessRuleResult EnforceRules(Transaction transaction)
    {
        // all entries with debit type have total TotalAmount equal all entries with credit total TotalAmount
        var rules = new List<(Func<bool> rule, BusinessRuleError error)>
        {
            (
                () => { return transaction.TransactionEntries.Count > 1; },
                new BusinessRuleError(
                    "TransactionEntryCount",
                    "Transaction must have at least 2 Entries",
                    "ERR-001",
                    SeverityLevel.Error
                )  
            ),
            (
                // at least one entry must be credit and one entry must be debit
                () =>
                {
                    var debitEntries = transaction.TransactionEntries.Where(x => x.DebitAmount != 0).ToList();
                    var creditEntries = transaction.TransactionEntries.Where(x => x.CreditAmount != 0).ToList();
                    
                    return debitEntries.Count != 0 && creditEntries.Count != 0;
                },
                new BusinessRuleError(
                    "TransactionEntryBothCreditDebit",
                    "Transaction must have at least one credit and one debit entry",
                    "ERR-002",
                    SeverityLevel.Error
                )
            ),
            (
                () =>
                {
                    var Entries = transaction.TransactionEntries.ToList();
                    
                    var debitTotalAmount = Entries.Sum(x => x.DebitAmount);
                    var creditTotalAmount = Entries.Sum(x => x.CreditAmount);
                    
                    if (debitTotalAmount != creditTotalAmount)
                    {
                        return false;
                    }
                    
                    return true;
                },
                new BusinessRuleError(
                    "TransactionEntriesSumEqual",
                    "Transaction entries must have same total amount",
                    "ERR-003",
                    SeverityLevel.Error
                    )
            ),
        };
        
        return EnsureRules(rules);
    }
}