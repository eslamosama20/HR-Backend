using SmartAndPro.ERP.Domains.Entities.Invoice;
using SmartAndPro.ERP.Infrastructure.Contexts;

namespace SmartAndPro.ERP.Infrastructure.Data.Persistence
{
    public class InvociesSeeder
    {
        public static void InvoiceTypes(ERPContext context)
        {
            List<InvoiceType> invoiceTypes = new List<InvoiceType>() 
            {
                new InvoiceType {Name = "Purchase"},
                new InvoiceType {Name = "Sale"}, 
            };

            foreach (var invoiceType in invoiceTypes)
            {
                if (!context.InvoiceTypes.Any(x => x.Name == invoiceType.Name))
                {
                    context.InvoiceTypes.Add(invoiceType);
                }
            }

            context.SaveChanges();
        }
    }
}
