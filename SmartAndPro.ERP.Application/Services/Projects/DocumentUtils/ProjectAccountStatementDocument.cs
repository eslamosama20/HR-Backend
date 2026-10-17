using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartAndPro.ERP.Domains.Entities.Projects;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Application.Services.PdfTemplateConfiguration;
using SmartAndPro.ERP.Domains.Entities;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.Domains.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartAndPro.ERP.Application.Services.Projects.DocumentUtils
{
    public class ProjectStatementEntry
    {
        public DateTime Date { get; set; }
        public string Type { get; set; } = string.Empty;
        public string ReferenceNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal IncomingAmount { get; set; }
        public decimal OutgoingAmount { get; set; }
    }

    public class ProjectAccountStatementDocument : IDocument
    {
        private readonly Project _project;
        private readonly List<ProjectStatementEntry> _entries;
        private readonly CompanyDetails _companyDetails;
        private readonly SmartAndPro.ERP.Domains.Entities.PdfTemplateConfiguration _pdfTemplateConfig;

        public ProjectAccountStatementDocument(
            Project project, 
            List<ProjectStatementEntry> entries, 
            CompanyDetails companyDetails,
            SmartAndPro.ERP.Domains.Entities.PdfTemplateConfiguration? pdfTemplateConfig = null)
        {
            _project = project;
            _entries = entries ?? new List<ProjectStatementEntry>();
            _companyDetails = companyDetails;
            _pdfTemplateConfig = pdfTemplateConfig ?? new SmartAndPro.ERP.Domains.Entities.PdfTemplateConfiguration();
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            try
            {
                container.Page(page =>
                {
                    // Header with Company Details (Sales Invoice Design Standard)
                    page.Header().Element(headerContainer =>
                    {
                        headerContainer.PaddingBottom(8).Row(row =>
                        {
                            // Column 1: English Company Details (Left)
                            row.RelativeItem(1.5f).AlignLeft().Column(col =>
                            {
                                if (!string.IsNullOrWhiteSpace(_companyDetails?.EnglishName))
                                {
                                    col.Item().AlignLeft().Text(_companyDetails.EnglishName).SemiBold().FontSize(12);
                                }
                                if (_companyDetails?.Address != null)
                                {
                                    if (!string.IsNullOrWhiteSpace(_companyDetails.Address.Street))
                                    {
                                        col.Item().PaddingTop(2).AlignLeft().Text(_companyDetails.Address.Street).FontSize(9);
                                    }
                                    if (!string.IsNullOrWhiteSpace(_companyDetails.Address.City))
                                    {
                                        col.Item().PaddingTop(2).AlignLeft().Text(_companyDetails.Address.City).FontSize(9);
                                    }
                                }
                                var primaryPhoneEng = _companyDetails?.Phones?.FirstOrDefault(p => p.IsPrimary);
                                if (primaryPhoneEng != null && !string.IsNullOrWhiteSpace(primaryPhoneEng.PhoneNumber))
                                {
                                    col.Item().PaddingTop(2).AlignLeft().Text($"Phone: {primaryPhoneEng.PhoneNumber}").FontSize(9);
                                }
                                if (!string.IsNullOrWhiteSpace(_companyDetails?.TaxNumber))
                                {
                                    col.Item().PaddingTop(2).AlignLeft().Text($"Tax ID: {_companyDetails.TaxNumber}").FontSize(9);
                                }
                            });

                            // Column 2: Logo (Center)
                            row.RelativeItem(1).AlignCenter().AlignMiddle().Column(col =>
                            {
                                if (_companyDetails?.Logo != null && _companyDetails.Logo.Length > 0)
                                {
                                    col.Item().AlignCenter().MaxHeight(70).MaxWidth(110).Image(_companyDetails.Logo).FitArea();
                                }
                            });

                            // Column 3: Arabic Company Details (Right)
                            row.RelativeItem(1.5f).AlignRight().Column(col =>
                            {
                                if (!string.IsNullOrWhiteSpace(_companyDetails?.ArabicName))
                                {
                                    col.Item().AlignRight().Text(_companyDetails.ArabicName).SemiBold().FontSize(12).FontColor(Colors.Blue.Darken2);
                                }
                                if (_companyDetails?.ArabicAddress != null)
                                {
                                    if (!string.IsNullOrWhiteSpace(_companyDetails.ArabicAddress.Street))
                                    {
                                        col.Item().PaddingTop(2).AlignRight().Text(_companyDetails.ArabicAddress.Street).FontSize(9);
                                    }
                                    if (!string.IsNullOrWhiteSpace(_companyDetails.ArabicAddress.City))
                                    {
                                        col.Item().PaddingTop(2).AlignRight().Text(_companyDetails.ArabicAddress.City).FontSize(9);
                                    }
                                }
                                var primaryPhoneAr = _companyDetails?.Phones?.FirstOrDefault(p => p.IsPrimary);
                                if (primaryPhoneAr != null && !string.IsNullOrWhiteSpace(primaryPhoneAr.PhoneNumber))
                                {
                                    col.Item().PaddingTop(2).AlignRight().Text($"الهاتف: {primaryPhoneAr.PhoneNumber}").FontSize(9);
                                }
                                if (!string.IsNullOrWhiteSpace(_companyDetails?.TaxNumber))
                                {
                                    col.Item().PaddingTop(2).AlignRight().Text($"الرقم الضريبي: {_companyDetails.TaxNumber}").FontSize(9);
                                }
                                if (!string.IsNullOrWhiteSpace(_companyDetails?.GaztNumber))
                                {
                                    col.Item().PaddingTop(2).AlignRight().Text($"السجل التجاري: {_companyDetails.GaztNumber}").FontSize(9);
                                }
                            });
                        });
                    });

                    page.Content().Column(col =>
                    {
                        // Header Title Block
                        col.Item().PaddingBottom(10).Border(1).BorderColor("#E2E8F0").Padding(8).Row(r =>
                        {
                            // من / إلى التاريخ في اليسار (Left)
                            r.RelativeItem().AlignLeft().Column(c =>
                            {
                                c.Item().AlignLeft().Text(t =>
                                {
                                    t.Span("من: ").SemiBold();
                                    t.Span(_entries.Count > 0 ? _entries.Min(e => e.Date).ToString("dd/MM/yyyy") : "-");
                                });
                                c.Item().PaddingTop(2).AlignLeft().Text(t =>
                                {
                                    t.Span("إلى: ").SemiBold();
                                    t.Span(_entries.Count > 0 ? _entries.Max(e => e.Date).ToString("dd/MM/yyyy") : "-");
                                });
                            });
                            
                            // العنوان في المنتصف (Center)
                            r.RelativeItem().AlignCenter().Text("كشف حساب المشروع").SemiBold().FontSize(16);
                            
                            // اسم المشروع في اليمين (Right)
                            r.RelativeItem().AlignRight().Column(c =>
                            {
                                c.Item().AlignRight().Text(t =>
                                {
                                    t.Span("المشروع: ").SemiBold();
                                    t.Span($"{_project.ProjectNumber ?? ""} - {_project.ProjectName}");
                                });
                            });
                        });

                        // Table - True RTL Order (Right to Left: التاريخ - رقم السند - نوع الحركة - البيان / ملاحظات - مقبوضات (+) - مدفوعات (-) - الرصيد الجاري)
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(85);  // التاريخ (Rightmost)
                                columns.ConstantColumn(75);  // رقم السند
                                columns.ConstantColumn(80);  // نوع الحركة
                                columns.RelativeColumn(1f);  // البيان / ملاحظات
                                columns.ConstantColumn(85);  // مقبوضات (+)
                                columns.ConstantColumn(85);  // مدفوعات (-)
                                columns.ConstantColumn(85);  // الرصيد الجاري (Leftmost)
                            });

                            table.Header(header =>
                            {
                                header.Cell().Element(HeaderStyle).AlignCenter().Text("التاريخ");
                                header.Cell().Element(HeaderStyle).AlignCenter().Text("رقم السند");
                                header.Cell().Element(HeaderStyle).AlignCenter().Text("نوع الحركة");
                                header.Cell().Element(HeaderStyle).AlignCenter().Text("البيان / ملاحظات");
                                header.Cell().Element(HeaderStyle).AlignCenter().Text("مقبوضات (+)");
                                header.Cell().Element(HeaderStyle).AlignCenter().Text("مدفوعات (-)");
                                header.Cell().Element(HeaderStyle).AlignCenter().Text("الرصيد الجاري");
                            });

                            // Data rows
                            decimal runningBalance = 0;
                            decimal totalDebit = 0;
                            decimal totalCredit = 0;

                            foreach (var entry in _entries)
                            {
                                runningBalance += entry.IncomingAmount - entry.OutgoingAmount;
                                totalDebit += entry.IncomingAmount;
                                totalCredit += entry.OutgoingAmount;

                                table.Cell().Element(DataStyle).AlignCenter().Text(entry.Date.ToString("dd/MM/yyyy"));
                                table.Cell().Element(DataStyle).AlignCenter().Text(string.IsNullOrWhiteSpace(entry.ReferenceNumber) ? "-" : entry.ReferenceNumber);
                                table.Cell().Element(DataStyle).AlignCenter().Text(string.IsNullOrWhiteSpace(entry.Type) ? "-" : entry.Type);
                                table.Cell().Element(DataStyle).AlignRight().Text(string.IsNullOrWhiteSpace(entry.Description) ? "-" : entry.Description);
                                table.Cell().Element(DataStyle).AlignCenter().Text(entry.IncomingAmount == 0 ? "-" : entry.IncomingAmount.ToString("N2"));
                                table.Cell().Element(DataStyle).AlignCenter().Text(entry.OutgoingAmount == 0 ? "-" : entry.OutgoingAmount.ToString("N2"));
                                table.Cell().Element(DataStyle).AlignCenter().Text(runningBalance.ToString("N2"));
                            }

                            // Totals Row - Matching RTL Columns
                            table.Cell().ColumnSpan(4).Element(TotalStyle).AlignCenter().Text("الإجمالي");
                            table.Cell().Element(TotalStyle).AlignCenter().Text(totalDebit.ToString("N2"));
                            table.Cell().Element(TotalStyle).AlignCenter().Text(totalCredit.ToString("N2"));
                            table.Cell().Element(TotalStyle).AlignCenter().Text(runningBalance.ToString("N2"));
                        });

                        // Signatures Section - Safe margins, Company on Right, Customer on Left
                        col.Item().PaddingTop(30).Row(r =>
                        {
                            // Company signature on the RIGHT (RTL)
                            r.RelativeItem(1).PaddingRight(10).Column(c =>
                            {
                                var companyName = _companyDetails.ArabicName ?? _companyDetails.EnglishName ?? "";
                                c.Item().AlignRight().Text(companyName).SemiBold().FontSize(11);
                                c.Item().PaddingTop(6).AlignRight().Text("التوقيع: ........................................").FontSize(9);
                                
                                if (_companyDetails.Stamp != null && PdfTemplateUtils.IsValidImageBytes(_companyDetails.Stamp))
                                {
                                    c.Item().PaddingTop(8).AlignRight().Height(70).Width(70).Image(_companyDetails.Stamp).FitArea();
                                }
                            });

                            // Customer signature on the LEFT (RTL)
                            r.RelativeItem(1).PaddingLeft(10).Column(c =>
                            {
                                c.Item().AlignLeft().Text("الجهة المستلمة / العميل").SemiBold().FontSize(11);
                                c.Item().PaddingTop(6).AlignLeft().Text("التوقيع: ........................................").FontSize(9);
                            });
                        });
                    });
                });
            }
            catch (Exception ex)
            {
                LogService.LogError($"Error in ProjectAccountStatementDocument.Compose: {ex.Message}");
                LogService.LogError($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        static IContainer HeaderStyle(IContainer c) => c.Background("#42acff").Border(1).BorderColor("#E2E8F0").Padding(4).DefaultTextStyle(t => t.SemiBold().FontColor("#ffffff"));
        static IContainer DataStyle(IContainer c) => c.Border(1).BorderColor("#E2E8F0").Padding(2);
        static IContainer TotalStyle(IContainer c) => c.Background("#E2E8F0").Border(1).BorderColor("#000000").Padding(4).DefaultTextStyle(t => t.SemiBold());
    }
}
