
using SmartAndPro.ERP.Domains.Entities.Inventory.ProductManagement;
using SmartAndPro.ERP.Domains.Entities.Inventory.SalesClassesManagement;
using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.Infrastructure.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartAndPro.ERP.Infrastructure.Data.Persistence
{
    public class ReservationSeeder
    {
        public static void SeedAll(ERPContext context)
        {
            var measurementUnitsList = new List<MeasurementUnit>
            {
                new MeasurementUnit {ArabicName ="نوس" ,EnglishName = "Nos" ,ArabicSymbol = "ن",
                EnglishSymbol = "N"},     
    
            };

            foreach (var unit in measurementUnitsList)
            {
                if (!context.MeasurementUnit.Any(x => x.EnglishName == unit.EnglishName))
                {
                    context.MeasurementUnit.Add(unit);
                }
            }
            context.SaveChanges();
            var measurementUnits = context.MeasurementUnit
                             .GroupBy(x => x.EnglishName)
                             .ToDictionary(g => g.Key, g => g.First().Id);


            var root = new RootSalesClass {
                
                ArabicName = "Root Container", EnglishName = "Root Container",
                
            };
            if (!context.RootSalesClasses.Any(x => x.EnglishName == root.EnglishName))
            {
                context.RootSalesClasses.Add(root);
                context.SaveChanges();
            }

            var header = new HeaderSalesClass
            {
                ArabicName = "Header Container",
                EnglishName = "Header Container",
                RootSalesClassId = root.Id ,
            };
            if (!context.HeaderSalesClasses.Any(x => x.EnglishName == header.EnglishName))
            {
                context.HeaderSalesClasses.Add(header);
                context.SaveChanges();
            }

            var detail = new DetailSalesClass
            {
                ArabicName = "Detail Container",
                EnglishName = "Detail Container",
                HeaderSalesClassId = header.Id
            };
            if (!context.DetailSalesClasses.Any(x => x.EnglishName == detail.EnglishName))
            {
                context.DetailSalesClasses.Add(detail);
                context.SaveChanges();
            }

            var mainItems = new List<MainSalesClass>
            {
            };

            foreach (var main in mainItems)
            {
                if (!context.MainSalesClasses.Any(x => x.EnglishName == main.EnglishName))
                {
                    context.MainSalesClasses.Add(main);
                }
            }
            context.SaveChanges();
        }
    }
}
