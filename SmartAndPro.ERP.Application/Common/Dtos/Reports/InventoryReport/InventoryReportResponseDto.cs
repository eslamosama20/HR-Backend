namespace SmartAndPro.ERP.Application.Common.Dtos.Reports.InventoryReport
{
    public class InventoryReportResponseDto
    {
        public string MainPurchaseClassReferenceCode { get; set; }
        public string MainPurchaseClassArabicName { get; set; }
        public string MainPurchaseClassEnglishName { get; set; }
        public string MeasurementUnitEnglishName { get; set; }
        public string MeasurementUnitArabicName { get; set; }
        public decimal BeforePeriodQuantity { get; set; } // the quantity of productUnits before the start date
        public decimal LastItemPrice { get; set; } // the price of the last item
        public decimal BeforePeriodTotalPrice { get; set; } // the total price before the start date BeforePeriodQuantity * LastItemPrice
        public decimal PeriodQuantity { get; set; } // the quantity of productUnits in the period
        public decimal InPeriodAverageItemPrice { get; set; } // the average price of the items in the period
        public decimal InPeriodTotalPrice { get; set; } // the total price in the period
        public decimal TotalQuantity { get; set; } // the total quantity of productUnits
        public decimal TotalPrice { get; set; } // the total price of productUnits TotalQuantity * LastItemPrice
    }
}
