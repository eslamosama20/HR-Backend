using System.Collections.ObjectModel;

namespace SmartAndPro.ERP.Application.Common.Constants
{
    /// <summary>
    /// Shared import field definitions and header aliases for upcoming Excel imports.
    /// Phase 1 scope: definitions only (no endpoint/service wiring yet).
    /// </summary>
    public static class ImportFieldMappings
    {
        public static class Treasury
        {
            public static readonly IReadOnlyCollection<string> RequiredFields =
                Array.AsReadOnly(new[] { "name" });

            public static readonly IReadOnlyDictionary<string, string> HeaderAliases =
                new ReadOnlyDictionary<string, string>(
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["name"] = "name",
                        ["treasuryname"] = "name",
                        ["treasury name"] = "name",
                        ["treasury_name"] = "name",
                        ["اسم الخزينة"] = "name",
                        ["الخزينة"] = "name",
                        ["mainaccountid"] = "mainAccountId",
                        ["main account id"] = "mainAccountId",
                        ["main_account_id"] = "mainAccountId",
                        ["معرف الحساب الرئيسي"] = "mainAccountId",
                        ["الحساب الرئيسي"] = "mainAccountId",
                        ["isfrontal"] = "isFrontal",
                        ["is frontal"] = "isFrontal",
                        ["is_frontal"] = "isFrontal",
                        ["فرونتال"] = "isFrontal",
                    });
        }

        public static class GeneralBankAccount
        {
            public static readonly IReadOnlyCollection<string> RequiredFields =
                Array.AsReadOnly(new[] { "accountName" });

            public static readonly IReadOnlyDictionary<string, string> HeaderAliases =
                new ReadOnlyDictionary<string, string>(
                    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        ["accountname"] = "accountName",
                        ["account name"] = "accountName",
                        ["account_name"] = "accountName",
                        ["اسم الحساب"] = "accountName",
                        ["bankname"] = "bankName",
                        ["bank name"] = "bankName",
                        ["bank_name"] = "bankName",
                        ["اسم البنك"] = "bankName",
                        ["bankaccountnumber"] = "bankAccountNumber",
                        ["bank account number"] = "bankAccountNumber",
                        ["bank_account_number"] = "bankAccountNumber",
                        ["رقم الحساب البنكي"] = "bankAccountNumber",
                        ["رقم الحساب"] = "bankAccountNumber",
                        ["bankbranchname"] = "bankBranchName",
                        ["bank branch name"] = "bankBranchName",
                        ["bank_branch_name"] = "bankBranchName",
                        ["اسم الفرع"] = "bankBranchName",
                        ["swiftcode"] = "swiftCode",
                        ["swift code"] = "swiftCode",
                        ["swift_code"] = "swiftCode",
                        ["رمز سويفت"] = "swiftCode",
                        ["iban"] = "iban",
                        ["ايبان"] = "iban",
                        ["آيبان"] = "iban",
                        ["mainaccountid"] = "mainAccountId",
                        ["main account id"] = "mainAccountId",
                        ["main_account_id"] = "mainAccountId",
                        ["معرف الحساب الرئيسي"] = "mainAccountId",
                        ["الحساب الرئيسي"] = "mainAccountId",
                        ["isfrontal"] = "isFrontal",
                        ["is frontal"] = "isFrontal",
                        ["is_frontal"] = "isFrontal",
                        ["فرونتال"] = "isFrontal",
                        ["isactive"] = "isActive",
                        ["is active"] = "isActive",
                        ["is_active"] = "isActive",
                        ["نشط"] = "isActive",
                    });
        }
    }
}
