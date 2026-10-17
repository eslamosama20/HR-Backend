using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Infrastructure.Contexts;

namespace SmartAndPro.ERP.Infrastructure.Data.Persistence;

public static class AccountingSeeder
{
    public static void SeedRootAccounts(ERPContext context)
    {
        if (!context.RootAccounts.Any())
        {
            var rootAccounts = new List<RootAccount>
            {
                new RootAccount { EnglishName = "Assets", ArabicName = "الأصول", ReferenceCode = "1" },
                new RootAccount { EnglishName = "Liabilities & Owner Share", ArabicName = "الخصوم وحقوق الملكية", ReferenceCode = "2" },
                new RootAccount { EnglishName = "Costs", ArabicName = "التكاليف", ReferenceCode = "3" },
                new RootAccount { EnglishName = "Operation Revenue", ArabicName = "ايرادات التشغيل", ReferenceCode = "4" },
                new RootAccount { EnglishName = "Expenses", ArabicName = "المصروفات", ReferenceCode = "5" }
            };
            
            foreach (var rootAccount in rootAccounts)
            {
                context.RootAccounts.Add(rootAccount);
            }

            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving Root Accounts: {ex.Message}");
                // Revert changes made to the context
                foreach (var rootAccount in rootAccounts)
                {
                    context.Entry(rootAccount).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                }
            }
        }
    }
    
    public static void SeedHeaderAccounts(ERPContext context)
    {


        if (!context.HeaderAccounts.Any())
        {
             var rootAccounts = context.RootAccounts.ToDictionary(ra => ra.ReferenceCode, ra => ra.Id);

             var headerAccounts = new List<HeaderAccount>
             {
                 // Assets header accounts
                 new HeaderAccount { 
                     EnglishName = "Fixed Assets", 
                     ArabicName = "اصول ثابتة", 
                     RootAccountId = rootAccounts["1"], 
                     ReferenceCode = "101" 
                 },
                 new HeaderAccount { 
                     EnglishName = "Current Assets", 
                     ArabicName = "الأصول المتداولة", 
                     RootAccountId = rootAccounts["1"], 
                     ReferenceCode = "102" 
                 },
    
                 // Liabilities header accounts
                 new HeaderAccount { 
                     EnglishName = "Liabilities", 
                     ArabicName = "الخصوم", 
                     RootAccountId = rootAccounts["2"], 
                     ReferenceCode = "201" 
                 },
    
                 // Expenses header accounts
                 new HeaderAccount { 
                     EnglishName = "Operation Cost", 
                     ArabicName = "تكاليف التشغيل", 
                     RootAccountId = rootAccounts["3"], 
                     ReferenceCode = "301" 
                 },
    
                 // Income header accounts
                 new HeaderAccount { 
                     EnglishName = "Revenue", 
                     ArabicName = "ايرادات", 
                     RootAccountId = rootAccounts["4"], 
                     ReferenceCode = "401" 
                 },

                 // Owner's Equity
                 new HeaderAccount { 
                     EnglishName = "Owner Share", 
                     ArabicName = "حقوق الملكية", 
                     RootAccountId = rootAccounts["2"], 
                     ReferenceCode = "202" 
                 },
    
                 new HeaderAccount { 
                     EnglishName = "Operation Expenses", 
                     ArabicName = "مصروفات التشغيل", 
                     RootAccountId = rootAccounts["5"], 
                     ReferenceCode = "501" 
                 }
             };

            
            foreach (var headerAccount in headerAccounts)
            {
                context.HeaderAccounts.Add(headerAccount);
            }
            
            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving Header Accounts: {ex.Message}");
                // Revert changes made to the context
                foreach (var headerAccount in headerAccounts)
                {
                    context.Entry(headerAccount).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                }
            }
        }
    }
    
            
            
    public static void SeedDetailAccounts(ERPContext context)
    {
        if (!context.DetailAccounts.Any())
        {
            var headerAccounts = context.HeaderAccounts.ToDictionary(ha => ha.ReferenceCode, ha => ha.Id);

            var detailAccounts = new List<DetailAccount>
            {
                // Main Expenses
                new DetailAccount
                {
                    EnglishName = "Main Expenses",
                    ArabicName = "مصروفات الرواتب ومافي حكمها",
                    HeaderAccountId = headerAccounts["501"],
                    ReferenceCode = "5011"
                },
                // Other Expenses
                new DetailAccount
                {
                    EnglishName = "Other Expenses",
                    ArabicName = "مصروفات أخري",
                    HeaderAccountId = headerAccounts["501"],
                    ReferenceCode = "5012"
                },
                // Cash & Safes
                new DetailAccount
                {
                    EnglishName = "Cash & Safes",
                    ArabicName = "النقدية",
                    HeaderAccountId = headerAccounts["102"],
                    ReferenceCode = "1021"
                },
                // Bank
                new DetailAccount
                {
                    EnglishName = "Bank",
                    ArabicName = "البنوك",
                    HeaderAccountId = headerAccounts["102"],
                    ReferenceCode = "1022"
                },
                // Clients
                new DetailAccount
                {
                    EnglishName = "Clients",
                    ArabicName = "العملاء",
                    HeaderAccountId = headerAccounts["102"],
                    ReferenceCode = "1023"
                },
                // Stores
                new DetailAccount
                {
                    EnglishName = "Stores",
                    ArabicName = "المخازن",
                    HeaderAccountId = headerAccounts["102"],
                    ReferenceCode = "1024"
                },
                // Creditors
                new DetailAccount
                {
                    EnglishName = "Creditors",
                    ArabicName = "دائنون",
                    HeaderAccountId = headerAccounts["201"],
                    ReferenceCode = "2012"
                },
                // Fixed Assets
                new DetailAccount
                {
                    EnglishName = "Fixed Assets",
                    ArabicName = "الاصول الثابتة",
                    HeaderAccountId = headerAccounts["101"],
                    ReferenceCode = "1011"
                },
                // Taxes & VAT
                new DetailAccount
                {
                    EnglishName = "Taxes & VAT",
                    ArabicName = "الضرائب",
                    HeaderAccountId = headerAccounts["201"],
                    ReferenceCode = "2013"
                },
                
                new DetailAccount
                {
	                EnglishName = "Sales Revenue",
	                ArabicName = "ايرادات مبيعات",
	                HeaderAccountId = headerAccounts["401"],
	                ReferenceCode = "4011"
                }
            };

                
            foreach (var detailAccount in detailAccounts)
            {
                context.DetailAccounts.Add(detailAccount);
            }
            
            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving Detail Accounts: {ex.Message}");
                // Revert changes made to the context
                foreach (var detailAccount in detailAccounts)
                {
                    context.Entry(detailAccount).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                }
            } 
        }
        
    }
    
    public static void SeedMainAccounts(ERPContext context)
    {
        if (!context.MainAccounts.Any())
        {
            var detailAccounts = context.DetailAccounts.ToDictionary(da => da.ReferenceCode, da => da.Id);
            var mainAccounts = new List<MainAccount>
            {
	            new MainAccount 
	            { 
		            ArabicName = "رواتب و أجور",
		            EnglishName = "Salaries & Wages",
		            ReferenceCode = "50110001",
		            DetailAccountId = detailAccounts["5011"],
	            },
	            new MainAccount 
	            { 
		            ArabicName = "مطبوعات وأدوات مكتبية",
		            EnglishName = "Printing & Stationary",
		            ReferenceCode = "50120001",
		            DetailAccountId = detailAccounts["5012"],
	            },
	            new MainAccount 
	            { 
		            ArabicName = "رسوم بنكية ",
		            EnglishName = "Bank Charge",
		            ReferenceCode = "50120002",
		            DetailAccountId = detailAccounts["5012"],
	            },
	            new MainAccount 
	            { 
		            ArabicName = "بدل اجازات",
		            EnglishName = "Vacation",
		            ReferenceCode = "50110002",
		            DetailAccountId = detailAccounts["5011"],
	            },
	            new MainAccount 
	            { 
		            ArabicName = "الخزينة الرئيسية",
		            EnglishName = "Main Treasury",
		            ReferenceCode = "10210001",
		            DetailAccountId = detailAccounts["1021"],
	            },
	            new MainAccount 
	            { 
		            ArabicName = "البنك الأهلي ",
		            EnglishName = "Ahli Bank ",
		            ReferenceCode = "10220001",
		            DetailAccountId = detailAccounts["1022"],
	            },
	            new MainAccount 
	            { 
		            ArabicName = "حاسب الي وطابعات",
		            EnglishName = "Computers & Printer",
		            ReferenceCode = "10110001",
		            DetailAccountId = detailAccounts["1011"],
	            },
	            new MainAccount 
	            { 
		            ArabicName = "وجبات الموظفين",
		            EnglishName = "Employee Meals",
		            ReferenceCode = "50110003",
		            DetailAccountId = detailAccounts["5011"],
	            },
	            new MainAccount 
	            { 
		            ArabicName = " بدل سكن الموظفين ",
		            EnglishName = "Employee Housing",
		            ReferenceCode = "50110004",
		            DetailAccountId = detailAccounts["5011"],
	            },
	            new MainAccount 
	            { 
		            ArabicName = "بدل مواصلات للموظفين",
		            EnglishName = "ُEmpolyee Transportation ",
		            ReferenceCode = "50110005",
		            DetailAccountId = detailAccounts["5011"],
	            },
	            new MainAccount 
	            { 
		            ArabicName = "تامنيات اجتماعية للموظفين",
		            EnglishName = "Benefits & Social Insurance",
		            ReferenceCode = "50110006",
		            DetailAccountId = detailAccounts["5011"],
	            },
	            new MainAccount 
	            { 
		            ArabicName = "تذاكر طيران للموظفين ",
		            EnglishName = "Air Fare Empolyee",
		            ReferenceCode = "50110007",
		            DetailAccountId = detailAccounts["5011"],
	            },
	            new MainAccount 
	            { 
		            ArabicName = "نهاية الخدمة للموظفين",
		            EnglishName = "End Of Service Empolyee",
		            ReferenceCode = "50110008",
		            DetailAccountId = detailAccounts["5011"],
	            },
	            new MainAccount 
	            { 
		            ArabicName = "تامين طبي للموظفين",
		            EnglishName = "Empolyee Medical Insurance",
		            ReferenceCode = "50110009",
		            DetailAccountId = detailAccounts["5011"],
	            },
	            new MainAccount 
	            { 
		            ArabicName = "اقامة  وتاشيرات ",
		            EnglishName = "Visa & Iqama",
		            ReferenceCode = "50110010",
		            DetailAccountId = detailAccounts["5011"],
	            },
	            new MainAccount 
	            { 
		            ArabicName = "مكافات موظفين وحوافز",
		            EnglishName = "Staff Bouns ",
		            ReferenceCode = "50110011",
		            DetailAccountId = detailAccounts["5011"],
	            },
	            new MainAccount 
	            { 
		            ArabicName = "تليفونات وفاكس",
		            EnglishName = "Telephone & Fax",
		            ReferenceCode = "50120003",
		            DetailAccountId = detailAccounts["5012"],
	            },
	            new MainAccount 
	            { 
		            ArabicName = "مواصلات داخلية ",
		            EnglishName = "Local Transportation",
		            ReferenceCode = "50120004",
		            DetailAccountId = detailAccounts["5012"],
	            },
	            new MainAccount 
	            { 
		            ArabicName = "بنك الراجحي ",
		            EnglishName = "Rajhi Bank ",
		            ReferenceCode = "10220002",
		            DetailAccountId = detailAccounts["1022"],
	            },
	            new MainAccount 
	            { 
		            ArabicName = "ضريبة القيمة المضافة  المشتريات",
		            EnglishName = "Input Vat Purchasing",
		            ReferenceCode = "20130001",
		            DetailAccountId = detailAccounts["2013"],
	            },
	            new MainAccount 
	            { 
		            ArabicName = "ضريبة القيمة المضافة  المبيعات",
		            EnglishName = "Output Vat Sales",
		            ReferenceCode = "20130003",
		            DetailAccountId = detailAccounts["2013"],
	            },
	            new MainAccount 
	            { 
		            ArabicName = "مدفوعات الضريبة ",
		            EnglishName = "Payment Vat ",
		            ReferenceCode = "20130005",
		            DetailAccountId = detailAccounts["2013"],
	            },
	            new MainAccount 
	            { 
		            ArabicName = "ايرادات مبيعات",
		            EnglishName = "Sales Revenue",
		            ReferenceCode = "40110001",
		            DetailAccountId = detailAccounts["4011"],
	            },
            };
            
            foreach (var mainAccount in mainAccounts)
            {
                context.MainAccounts.Add(mainAccount);
            }
            
            try
            {
                context.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving Main Accounts: {ex.Message}");
                // Revert changes made to the context
                foreach (var mainAccount in mainAccounts)
                {
                    context.Entry(mainAccount).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
                }
            }
        }
    }
    
    public static void SeedTransactionTypes(ERPContext context)
    {
	    var transactionTypes = new List<TransactionType>
	    {
		    new TransactionType { EnglishName = "Paycheck receipt", ArabicName = "سند قبض" },
		    new TransactionType { EnglishName = "Reservation", ArabicName = "قيد حجز" },
		    new TransactionType { EnglishName = "Inventory stocktaking", ArabicName = "جرد المخزون" },
		    new TransactionType { EnglishName = "Payment receipt", ArabicName = "سند صرف" },
		    new TransactionType { EnglishName = "Inventory permit", ArabicName = "إذن مخزن" },
		    new TransactionType { EnglishName = "Transaction", ArabicName = "قيد يوميه" },
		    new TransactionType { EnglishName = "Invoice Transaction", ArabicName = "قيد فاتورة" },
		    new TransactionType { EnglishName = "Return Transaction", ArabicName = "قيد مرتجع" },
		    new TransactionType { EnglishName = "Deposit", ArabicName = "إيداع" },
		    new TransactionType { EnglishName = "Withdrawal", ArabicName = "سحب" },
		    new TransactionType { EnglishName = "Transfer", ArabicName = "تحويل" },
		    new TransactionType { EnglishName = "Payment", ArabicName = "دفع" },
		    new TransactionType { EnglishName = "Receivable", ArabicName = "مستلم" },
		    new TransactionType { EnglishName = "Payable", ArabicName = "مدفوع" },
		    new TransactionType { EnglishName = "Cash", ArabicName = "نقد" },
		    new TransactionType { EnglishName = "Cheque", ArabicName = "شيك" },
		    new TransactionType { EnglishName = "Bank", ArabicName = "بنك" },
		    new TransactionType { EnglishName = "Other", ArabicName = "أخرى" }
	    };

	    foreach (var transactionType in transactionTypes)
	    {
		    var exists = context.TransactionTypes
			    .Any(tt => tt.EnglishName == transactionType.EnglishName);

		    if (!exists)
		    {
			    context.TransactionTypes.Add(transactionType);
		    }
	    }

	    try
	    {
		    context.SaveChanges();
	    }
	    catch (Exception ex)
	    {
		    Console.WriteLine($"Error saving Transaction Types: {ex.Message}");
		    // Revert changes made to the context
		    foreach (var transactionType in transactionTypes)
		    {
			    context.Entry(transactionType).State = Microsoft.EntityFrameworkCore.EntityState.Detached;
		    }
	    }
    }

}

