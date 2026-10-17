using OfficeOpenXml;
using OfficeOpenXml.Style;
using SmartAndPro.ERP.Application.Common.Dtos.Invoice.Sale;
using SmartAndPro.ERP.Application.Common.Dtos.Invoice.Purchase;
using SmartAndPro.ERP.Application.Common.Dtos.Reports.TopProducts;
using System.Drawing;

namespace SmartAndPro.ERP.Application.Utility
{
    public class ExcelExporter
    {
        public static byte[] ExportSalesInvoiceToExcel(List<SalesInvoiceDetailsResponse> invoices)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Set license context
            using (var package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sales Report");

                // Set headers
                worksheet.Cells["A1"].Value = "رقم الفاتورة";
                worksheet.Cells["B1"].Value = "اسم العميل";
                worksheet.Cells["C1"].Value = "الوصف";
                worksheet.Cells["D1"].Value = "تاريخ الفاتورة";
                worksheet.Cells["E1"].Value = "كود الصنف";
                worksheet.Cells["F1"].Value = "اسم الصنف";
                worksheet.Cells["G1"].Value = "العدد";
                worksheet.Cells["H1"].Value = "سعر الوحدة";
                worksheet.Cells["I1"].Value = "إجمالي الفاتورة";
                worksheet.Cells["A1:I1"].Style.Font.Bold = true;
                worksheet.Cells["A1:I1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells["A1:I1"].Style.Fill.BackgroundColor.SetColor(Color.LightGray);

                int row = 2;

                foreach (var invoice in invoices)
                {
                    // Invoice Header
                    worksheet.Cells[row, 1].Value = invoice.InvoiceNo;
                    worksheet.Cells[row, 2].Value = invoice.SuppilerArabicName;
                    worksheet.Cells[row, 3].Value = invoice.Description;
                    worksheet.Cells[row, 4].Value = invoice.IssueDate.ToString("dd/MM/yyyy");

                    worksheet.Cells[row, 9].Value = "إجمالي الفاتورة";
                    worksheet.Cells[row, 9].Style.Font.Color.SetColor(Color.Blue);
                    worksheet.Cells[row, 9].Style.Font.Bold = true;

                    row++;

                    // Items details
                    foreach (var item in invoice.ItemDetails)
                    {
                        worksheet.Cells[row, 5].Value = item.ReferenceCode;
                        worksheet.Cells[row, 6].Value = item.ItemArabicName;
                        worksheet.Cells[row, 7].Value = item.Quantity;
                        worksheet.Cells[row, 8].Value = item.Price;
                        worksheet.Cells[row, 8].Style.Numberformat.Format = "#,##0.00";
                        worksheet.Cells[row, 9].Value = item.Quantity * item.Price;
                        row++;
                    }

                    // Subtotal for each invoice
                    worksheet.Cells[row, 7].Value = "إجمالي العدد";
                    worksheet.Cells[row, 8].Value = "إجمالي السعر";
                    worksheet.Cells[row, 9].Value = invoice.TotalPrice;
                    worksheet.Cells[row, 9].Style.Font.Bold = true;
                    worksheet.Cells[row, 9].Style.Font.Color.SetColor(Color.Red);

                    row += 2; // add space between invoices
                }

                // Auto-fit columns for all data
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Save and return byte array
                return package.GetAsByteArray();
            }
        }

        public static byte[] ExportSalesVATToExcel(List<SalesVATDetailsResponse> invoices)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Sales VAT Report");

                // Set headers
                worksheet.Cells["A1"].Value = "رقم الفاتورة";
                worksheet.Cells["B1"].Value = "اسم العميل";
                worksheet.Cells["C1"].Value = "تاريخ الفاتورة";
                worksheet.Cells["D1"].Value = "معفاة من الضرائب";
                worksheet.Cells["E1"].Value = "كود الصنف";
                worksheet.Cells["F1"].Value = "اسم الصنف";
                worksheet.Cells["G1"].Value = "العدد";
                worksheet.Cells["H1"].Value = "قيمة القيمة المضافة";
                worksheet.Cells["A1:H1"].Style.Font.Bold = true;
                worksheet.Cells["A1:H1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells["A1:H1"].Style.Fill.BackgroundColor.SetColor(Color.LightGray);

                int row = 2;

                foreach (var invoice in invoices)
                {
                    foreach (var item in invoice.ItemDetails)
                    {
                        worksheet.Cells[row, 1].Value = invoice.InvoiceNo;
                        worksheet.Cells[row, 2].Value = invoice.SuppilerArabicName;
                        worksheet.Cells[row, 3].Value = invoice.IssueDate.ToString("dd/MM/yyyy");
                        worksheet.Cells[row, 4].Value = item.IsTaxExempted ? "نعم" : "لا";
                        worksheet.Cells[row, 5].Value = item.ReferenceCode;
                        worksheet.Cells[row, 6].Value = item.ItemArabicName;
                        worksheet.Cells[row, 7].Value = item.Quantity;
                        worksheet.Cells[row, 8].Value = item.VATAmount;
                        worksheet.Cells[row, 8].Style.Numberformat.Format = "#,##0.00";
                        row++;
                    }
                }

                // Auto-fit columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                return package.GetAsByteArray();
            }
        }

        public static byte[] ExportPurchaseVATToExcel(List<PurchaseVATDetailsResponse> invoices)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Purchase VAT Report");

                // Set headers
                worksheet.Cells["A1"].Value = "رقم الفاتورة";
                worksheet.Cells["B1"].Value = "اسم المورد";
                worksheet.Cells["C1"].Value = "تاريخ الفاتورة";
                worksheet.Cells["D1"].Value = "معفاة من الضرائب";
                worksheet.Cells["E1"].Value = "كود الصنف";
                worksheet.Cells["F1"].Value = "اسم الصنف";
                worksheet.Cells["G1"].Value = "العدد";
                worksheet.Cells["H1"].Value = "قيمة القيمة المضافة";
                worksheet.Cells["A1:H1"].Style.Font.Bold = true;
                worksheet.Cells["A1:H1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells["A1:H1"].Style.Fill.BackgroundColor.SetColor(Color.Orange);

                int row = 2;

                foreach (var invoice in invoices)
                {
                    foreach (var item in invoice.Items)
                    {
                        worksheet.Cells[row, 1].Value = invoice.InvoiceNo;
                        worksheet.Cells[row, 2].Value = invoice.SuppilerArabicName;
                        worksheet.Cells[row, 3].Value = invoice.IssueDate.ToString("dd/MM/yyyy");
                        worksheet.Cells[row, 4].Value = item.IsTaxExempted ? "نعم" : "لا";
                        worksheet.Cells[row, 5].Value = item.ReferenceCode;
                        worksheet.Cells[row, 6].Value = item.ItemArabicName;
                        worksheet.Cells[row, 7].Value = item.Quantity;
                        worksheet.Cells[row, 8].Value = item.VATAmount;
                        worksheet.Cells[row, 8].Style.Numberformat.Format = "#,##0.00";
                        row++;
                    }
                }

                // Auto-fit columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                return package.GetAsByteArray();
            }
        }

        /// <summary>
        /// تصدير تقرير المنتجات الأكثر مبيعاً/طلباً إلى Excel
        /// </summary>
        public static byte[] ExportTopProductsToExcel(List<TopProductDto> products, string reportTitle, string transactionColumnName = "عدد المبيعات", string lastTransactionColumnName = "آخر بيع")
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(reportTitle);
                
                // Set RTL direction for better Arabic support
                worksheet.View.RightToLeft = true;

                // Title row
                worksheet.Cells["A1:I1"].Merge = true;
                worksheet.Cells["A1"].Value = reportTitle;
                worksheet.Cells["A1"].Style.Font.Bold = true;
                worksheet.Cells["A1"].Style.Font.Size = 16;
                worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["A1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                worksheet.Cells["A1"].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 112, 192));
                worksheet.Cells["A1"].Style.Font.Color.SetColor(Color.White);

                // Date row
                worksheet.Cells["A2:I2"].Merge = true;
                worksheet.Cells["A2"].Value = $"تاريخ التقرير: {DateTime.Now:dd/MM/yyyy HH:mm}";
                worksheet.Cells["A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["A2"].Style.Font.Size = 11;

                // Headers row (row 4)
                int headerRow = 4;
                worksheet.Cells[headerRow, 1].Value = "م";
                worksheet.Cells[headerRow, 2].Value = "كود المنتج";
                worksheet.Cells[headerRow, 3].Value = "اسم المنتج";
                worksheet.Cells[headerRow, 4].Value = "الوحدة";
                worksheet.Cells[headerRow, 5].Value = transactionColumnName;
                worksheet.Cells[headerRow, 6].Value = "إجمالي الكمية";
                worksheet.Cells[headerRow, 7].Value = "إجمالي المبلغ";
                worksheet.Cells[headerRow, 8].Value = "متوسط السعر";
                worksheet.Cells[headerRow, 9].Value = lastTransactionColumnName;

                // Style headers
                using (var range = worksheet.Cells[headerRow, 1, headerRow, 9])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(68, 84, 106));
                    range.Style.Font.Color.SetColor(Color.White);
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                }

                // Data rows
                int row = 5;
                foreach (var product in products)
                {
                    var bgColor = product.Rank % 2 == 0 ? Color.FromArgb(242, 242, 242) : Color.White;
                    
                    worksheet.Cells[row, 1].Value = product.Rank;
                    worksheet.Cells[row, 2].Value = product.ProductCode;
                    worksheet.Cells[row, 3].Value = product.ProductNameAr;
                    worksheet.Cells[row, 4].Value = product.UnitName;
                    worksheet.Cells[row, 5].Value = product.TransactionCount;
                    worksheet.Cells[row, 6].Value = product.TotalQuantity;
                    worksheet.Cells[row, 6].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[row, 7].Value = product.TotalAmount;
                    worksheet.Cells[row, 7].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[row, 8].Value = product.AverageUnitPrice;
                    worksheet.Cells[row, 8].Style.Numberformat.Format = "#,##0.00";
                    worksheet.Cells[row, 9].Value = product.LastTransactionDate?.ToString("dd/MM/yyyy") ?? "-";

                    // Apply background color and borders to data row
                    using (var range = worksheet.Cells[row, 1, row, 9])
                    {
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(bgColor);
                        range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                        range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    }

                    row++;
                }

                // Summary row
                row++;
                worksheet.Cells[row, 1, row, 4].Merge = true;
                worksheet.Cells[row, 1].Value = "الإجمالي";
                worksheet.Cells[row, 1].Style.Font.Bold = true;
                worksheet.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                
                worksheet.Cells[row, 5].Value = products.Sum(p => p.TransactionCount);
                worksheet.Cells[row, 5].Style.Font.Bold = true;
                
                worksheet.Cells[row, 6].Value = products.Sum(p => p.TotalQuantity);
                worksheet.Cells[row, 6].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[row, 6].Style.Font.Bold = true;
                
                worksheet.Cells[row, 7].Value = products.Sum(p => p.TotalAmount);
                worksheet.Cells[row, 7].Style.Numberformat.Format = "#,##0.00";
                worksheet.Cells[row, 7].Style.Font.Bold = true;

                using (var range = worksheet.Cells[row, 1, row, 9])
                {
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(221, 235, 247));
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                }

                // Auto-fit columns
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                // Set minimum column widths for better readability
                worksheet.Column(1).Width = 8;   // م
                worksheet.Column(2).Width = 15;  // كود
                worksheet.Column(3).Width = 30;  // اسم المنتج
                worksheet.Column(4).Width = 12;  // الوحدة
                worksheet.Column(5).Width = 15;  // عدد المبيعات
                worksheet.Column(6).Width = 15;  // الكمية
                worksheet.Column(7).Width = 18;  // المبلغ
                worksheet.Column(8).Width = 15;  // متوسط السعر
                worksheet.Column(9).Width = 15;  // آخر بيع

                return package.GetAsByteArray();
            }
        }
    }
}
