using SmartAndPro.ERP.Infrastructure.Contexts;
using System;
using System.Linq;

namespace SmartAndPro.ERP.Infrastructure.Data.Persistence;

public class DataSeeder
{
    public static void Seed(ERPContext context)
    {
        // TODO: check if dev mode dont seed
        AuthSeeder.SeedOTPTypes(context);
        AuthSeeder.SeedOTPStatuses(context);
        AuthSeeder.SeedDomainDetails(context);
        AuthSeeder.SeedRoles(context);
        AuthSeeder.SeedPermissions(context);
        AuthSeeder.SeedRolePermissions(context);
        AuthSeeder.SeedAdminUser(context);
        
        AccountingSeeder.SeedRootAccounts(context);
        AccountingSeeder.SeedHeaderAccounts(context);
        AccountingSeeder.SeedDetailAccounts(context);
        AccountingSeeder.SeedMainAccounts(context);
        AccountingSeeder.SeedTransactionTypes(context);

        InvociesSeeder.InvoiceTypes(context);
        ReservationSeeder.SeedAll(context);

        // Seed CompanyDetails and PdfTemplateConfiguration if empty
        if (!context.CompanyDetails.Any())
        {
            context.CompanyDetails.Add(new SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement.CompanyDetails
            {
                Id = Guid.Parse("2ee25ac0-60d1-49b7-60ff-08ddde4f49b7"),
                ArabicName = "سمارت برو للخدمات",
                EnglishName = "Smart & Pro",
                Email = "smaartx7@gmail.com",
                Address = "شارع المطار, المدينة المنورة, المملكة العربية السعودية, 432222",
                IsActive = true,
                CreatedBy = Guid.Empty,
                CreatedAt = DateTime.Now,
                TaxNumber = "3007006532111",
                IsZatcaIntegrated = false,
                RegistrationNumber = "65780332460",
                UnifiedNumber = "7005432167",
                ArabicAddress = "شارع المطار, المدينة المنورة, المملكة العربية السعودية, 432222"
            });
        }

        var pdfSet = context.Set<SmartAndPro.ERP.Domains.Entities.PdfTemplateConfiguration>();
        if (!pdfSet.Any())
        {
            pdfSet.Add(new SmartAndPro.ERP.Domains.Entities.PdfTemplateConfiguration
            {
                Id = Guid.Parse("b9a91cb9-adcf-4dfb-1c2a-08ddcd9a4cc8"),
                Header = new SmartAndPro.ERP.Domains.ValueObjects.PdfHeaderFooterConfig
                {
                    AddressSubfields = new System.Collections.Generic.List<string> { "city" }
                },
                Footer = new SmartAndPro.ERP.Domains.ValueObjects.PdfHeaderFooterConfig
                {
                    AddressSubfields = new System.Collections.Generic.List<string> { "city" }
                },
                IsActive = true,
                CreatedBy = Guid.Empty,
                CreatedAt = DateTime.Now
            });
        }
        context.SaveChanges();
    }
}
