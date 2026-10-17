using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Domains.Entities.PublicBooking;
using SmartAndPro.ERP.Infrastructure.Contexts;

namespace SmartAndPro.ERP.WebAPI;

public static class PublicPaidServiceSeeder
{
    public static void SeedPaidServices(ERPContext context)
    {
        // Check if already seeded
        if (context.PublicPaidServices.Any())
        {
            Console.WriteLine("Paid services already seeded.");
            return;
        }

        var services = new List<PublicPaidService>
        {
            new PublicPaidService
            {
                Id = Guid.NewGuid(),
                NameEn = "Transportation between Makkah and Madinah",
                NameAr = "نقل بين مكة والمدينة",
                DescriptionEn = "Comfortable transportation between the two holy cities",
                DescriptionAr = "خدمة نقل مريحة بين الحرمين الشريفين",
                Icon = "pi pi-car",
                ServiceTypeCode = "TRANSPORT",
                DisplayOrder = 1,
                IsActive = true,
                CreatedAt = DateTime.Now,
                CreatedBy = Guid.Empty
            },
            new PublicPaidService
            {
                Id = Guid.NewGuid(),
                NameEn = "Madinah Tours",
                NameAr = "مزارات المدينة",
                DescriptionEn = "Guided tours of historical sites in Madinah",
                DescriptionAr = "جولات إرشادية للمواقع التاريخية في المدينة المنورة",
                Icon = "pi pi-compass",
                ServiceTypeCode = "TOUR",
                DisplayOrder = 2,
                IsActive = true,
                CreatedAt = DateTime.Now,
                CreatedBy = Guid.Empty
            },
            new PublicPaidService
            {
                Id = Guid.NewGuid(),
                NameEn = "Makkah Tours",
                NameAr = "مزارات مكة",
                DescriptionEn = "Guided tours of historical sites in Makkah",
                DescriptionAr = "جولات إرشادية للمواقع التاريخية في مكة المكرمة",
                Icon = "pi pi-map-marker",
                ServiceTypeCode = "TOUR",
                DisplayOrder = 3,
                IsActive = true,
                CreatedAt = DateTime.Now,
                CreatedBy = Guid.Empty
            },
            new PublicPaidService
            {
                Id = Guid.NewGuid(),
                NameEn = "Transport from Madinah to Makkah to Muzdalifah",
                NameAr = "النقل من المدينة لمكة لمزدلفة",
                DescriptionEn = "Transportation service from Madinah to Makkah to Muzdalifah",
                DescriptionAr = "خدمة نقل من المدينة المنورة لمكة المكرمة لمزدلفة",
                Icon = "pi pi-directions",
                ServiceTypeCode = "TRANSPORT",
                DisplayOrder = 4,
                IsActive = true,
                CreatedAt = DateTime.Now,
                CreatedBy = Guid.Empty
            },
            new PublicPaidService
            {
                Id = Guid.NewGuid(),
                NameEn = "Hotel to Airport Transfer",
                NameAr = "النقل من الفندق للمطار",
                DescriptionEn = "Private transfer from hotel to airport",
                DescriptionAr = "نقل خاص من الفندق إلى المطار",
                Icon = "pi pi-send",
                ServiceTypeCode = "TRANSPORT",
                DisplayOrder = 5,
                IsActive = true,
                CreatedAt = DateTime.Now,
                CreatedBy = Guid.Empty
            },
            new PublicPaidService
            {
                Id = Guid.NewGuid(),
                NameEn = "Train Station to Hotel Transfer",
                NameAr = "النقل من محطة القطار إلى الفندق",
                DescriptionEn = "Private transfer from train station to hotel",
                DescriptionAr = "نقل خاص من محطة القطار إلى الفندق",
                Icon = "pi pi-building",
                ServiceTypeCode = "TRANSPORT",
                DisplayOrder = 6,
                IsActive = true,
                CreatedAt = DateTime.Now,
                CreatedBy = Guid.Empty
            }
        };

        context.PublicPaidServices.AddRange(services);
        context.SaveChanges();

        Console.WriteLine($"✓ Successfully seeded {services.Count} paid services");
    }
}
