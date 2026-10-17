using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SmartAndPro.ERP.Application.Common.Dtos.Projects;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SmartAndPro.ERP.Application.Services.Projects.DocumentUtils
{
    public class ProjectArchiveDocument : IDocument
    {
        private readonly ProjectDetailsResponse _project;
        private readonly CompanyDetails _companyDetails;

        public ProjectArchiveDocument(ProjectDetailsResponse project, CompanyDetails companyDetails)
        {
            _project = project;
            _companyDetails = companyDetails;
        }

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.PageColor(Colors.White);
                page.MarginVertical(15);
                page.MarginHorizontal(20);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontFamily("Almarai").FontSize(10));
                page.ContentFromRightToLeft(); // Arabic layout

                // Header - Sales Invoice Design Standard
                page.Header().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        // Left Column (English Company Details)
                        row.RelativeItem(1.5f).Column(englishCol =>
                        {
                            englishCol.Item().PaddingBottom(3).AlignLeft().Text(_companyDetails?.EnglishName ?? "Project Execution Entity").FontSize(12).Bold().FontColor(Colors.Blue.Darken2);
                            if (_companyDetails?.Address != null)
                            {
                                if (!string.IsNullOrWhiteSpace(_companyDetails.Address.Street))
                                    englishCol.Item().PaddingBottom(1).AlignLeft().Text(_companyDetails.Address.Street).FontSize(9).FontColor(Colors.Grey.Darken2);
                                if (!string.IsNullOrWhiteSpace(_companyDetails.Address.City))
                                    englishCol.Item().PaddingBottom(1).AlignLeft().Text(_companyDetails.Address.City).FontSize(9).FontColor(Colors.Grey.Darken2);
                            }
                            var primaryPhoneEng = _companyDetails?.Phones?.FirstOrDefault(p => p.IsPrimary);
                            if (primaryPhoneEng != null && !string.IsNullOrWhiteSpace(primaryPhoneEng.PhoneNumber))
                                englishCol.Item().PaddingBottom(1).AlignLeft().Text($"Phone: {primaryPhoneEng.PhoneNumber}").FontSize(9).FontColor(Colors.Grey.Darken2);
                            if (!string.IsNullOrWhiteSpace(_companyDetails?.TaxNumber))
                                englishCol.Item().PaddingBottom(1).AlignLeft().Text($"Tax ID: {_companyDetails.TaxNumber}").FontSize(9).FontColor(Colors.Grey.Darken2);
                        });

                        // Center Logo
                        row.RelativeItem(1f).AlignCenter().AlignMiddle().Column(logoCol =>
                        {
                            if (_companyDetails?.Logo != null && _companyDetails.Logo.Length > 0)
                            {
                                logoCol.Item().AlignCenter().MaxHeight(70).MaxWidth(110).Image(_companyDetails.Logo).FitArea();
                            }
                        });

                        // Right Column (Arabic Company Details)
                        row.RelativeItem(1.5f).Column(arabicCol =>
                        {
                            arabicCol.Item().PaddingBottom(3).AlignRight().Text(_companyDetails?.ArabicName ?? "مؤسسة تنفيذ المشاريع").FontSize(12).Bold().FontColor(Colors.Blue.Darken2);
                            if (_companyDetails?.ArabicAddress != null)
                            {
                                if (!string.IsNullOrWhiteSpace(_companyDetails.ArabicAddress.Street))
                                    arabicCol.Item().PaddingBottom(1).AlignRight().Text(_companyDetails.ArabicAddress.Street).FontSize(9).FontColor(Colors.Grey.Darken2);
                                if (!string.IsNullOrWhiteSpace(_companyDetails.ArabicAddress.City))
                                    arabicCol.Item().PaddingBottom(1).AlignRight().Text(_companyDetails.ArabicAddress.City).FontSize(9).FontColor(Colors.Grey.Darken2);
                            }
                            var primaryPhoneAr = _companyDetails?.Phones?.FirstOrDefault(p => p.IsPrimary);
                            if (primaryPhoneAr != null && !string.IsNullOrWhiteSpace(primaryPhoneAr.PhoneNumber))
                                arabicCol.Item().PaddingBottom(1).AlignRight().Text($"الهاتف: {primaryPhoneAr.PhoneNumber}").FontSize(9).FontColor(Colors.Grey.Darken2);
                            if (!string.IsNullOrWhiteSpace(_companyDetails?.TaxNumber))
                                arabicCol.Item().PaddingBottom(1).AlignRight().Text($"الرقم الضريبي: {_companyDetails.TaxNumber}").FontSize(9).FontColor(Colors.Grey.Darken2);
                            if (!string.IsNullOrWhiteSpace(_companyDetails?.GaztNumber))
                                arabicCol.Item().PaddingBottom(1).AlignRight().Text($"السجل التجاري: {_companyDetails.GaztNumber}").FontSize(9).FontColor(Colors.Grey.Darken2);
                        });
                    });
                    col.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Blue.Darken2);
                });

                // Content
                page.Content().Column(col =>
                {
                    // Report Title
                    col.Item().AlignCenter().PaddingBottom(15).Text("الملف الأرشيفي المتكامل للمشروع").FontSize(16).Bold().FontColor(Colors.Blue.Darken3);

                    // 1. Basic Info Section
                    col.Item().PaddingBottom(5).Text("1. البيانات العامة للمشروع").FontSize(12).Bold().FontColor(Colors.Blue.Darken2);
                    col.Item().Border(1).BorderColor(Colors.Grey.Lighten1).Background(Colors.Grey.Lighten5).Padding(10).Column(basicCol =>
                    {
                        basicCol.Item().Row(row =>
                        {
                            row.RelativeItem().Text($"اسم المشروع: {_project.ProjectName}").Bold();
                            row.RelativeItem().Text($"رقم المشروع: {_project.ProjectNumber ?? "-"}");
                        });
                        basicCol.Item().PaddingTop(5).Row(row =>
                        {
                            row.RelativeItem().Text($"رقم الملف: {_project.FileNumber ?? "-"}");
                            row.RelativeItem().Text($"دولة التنفيذ: {_project.Country ?? "-"}");
                        });
                        basicCol.Item().PaddingTop(5).Row(row =>
                        {
                            row.RelativeItem().Text($"الحالة: {_project.Status}");
                            row.RelativeItem().Text($"تاريخ الإنشاء: {_project.CreatedAt:dd/MM/yyyy}");
                        });
                        basicCol.Item().PaddingTop(5).Row(row =>
                        {
                            row.RelativeItem().Text($"المبلغ الإجمالي: {(_project.TotalAmount?.ToString("N2") ?? "-")} SAR");
                            row.RelativeItem().Text($"عمولة التشغيل: {(_project.CommissionPercentage?.ToString() ?? "0")}%");
                        });
                    });

                    // 2. Assignment Phase Section
                    col.Item().PaddingTop(15).PaddingBottom(5).Text("2. مرحلة الإسناد والاعتماد").FontSize(12).Bold().FontColor(Colors.Blue.Darken2);
                    if (_project.AssignmentPhase != null)
                    {
                        col.Item().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(10).Column(assignCol =>
                        {
                            assignCol.Item().Row(row =>
                            {
                                row.RelativeItem().Text($"الجهة المنفذة: {_project.AssignmentPhase.ExecutingEntityName ?? "-"}");
                                row.RelativeItem().Text($"الجهة المشرفة: {_project.AssignmentPhase.SupervisingEntityName ?? "-"}");
                            });
                            assignCol.Item().PaddingTop(5).Row(row =>
                            {
                                row.RelativeItem().Text($"الإدارة المسؤولة: {_project.AssignmentPhase.DepartmentName ?? "-"}");
                                row.RelativeItem().Text($"مدة التنفيذ: {(_project.AssignmentPhase.ExecutionDuration?.ToString() ?? "-")} {_project.AssignmentPhase.ExecutionDurationUnit}");
                            });
                            assignCol.Item().PaddingTop(5).Row(row =>
                            {
                                row.RelativeItem().Text($"تاريخ الإسناد: {_project.AssignmentPhase.AssignmentDate?.ToString("dd/MM/yyyy") ?? "-"}");
                                row.RelativeItem().Text($"تاريخ اعتماد الإسناد: {_project.AssignmentPhase.AssignmentApprovalDate?.ToString("dd/MM/yyyy") ?? "-"}");
                            });
                            if (!string.IsNullOrEmpty(_project.AssignmentPhase.Notes))
                            {
                                assignCol.Item().PaddingTop(5).Text($"ملاحظات الإسناد: {_project.AssignmentPhase.Notes}");
                            }
                        });
                    }
                    else
                    {
                        col.Item().Text("لا توجد بيانات لمرحلة الإسناد.").FontColor(Colors.Grey.Darken1).Italic();
                    }

                    // 3. Execution Phase Section
                    col.Item().PaddingTop(15).PaddingBottom(5).Text("3. مرحلة التنفيذ والمتابعة").FontSize(12).Bold().FontColor(Colors.Blue.Darken2);
                    if (_project.ExecutionPhase != null)
                    {
                        col.Item().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(10).Column(execCol =>
                        {
                            execCol.Item().Row(row =>
                            {
                                row.RelativeItem().Text($"الشركة المنفذة: {_project.ExecutionPhase.CompanyName ?? "-"}");
                                row.RelativeItem().Text($"نسبة الإنجاز الفعلية: {_project.ExecutionPhase.CompletionPercentage ?? 0}%").Bold();
                            });
                            execCol.Item().PaddingTop(5).Row(row =>
                            {
                                row.RelativeItem().Text($"مبلغ المنحة: {(_project.ExecutionPhase.GrantAmount?.ToString("N2") ?? "-")} SAR");
                                row.RelativeItem().Text($"مبلغ التنفيذ: {(_project.ExecutionPhase.ExecutionAmount?.ToString("N2") ?? "-")} SAR");
                            });
                            execCol.Item().PaddingTop(5).Row(row =>
                            {
                                row.RelativeItem().Text($"المبلغ المتبقي: {(_project.ExecutionPhase.RemainingAmount?.ToString("N2") ?? "-")} SAR");
                                row.RelativeItem().Text($"حالة تحويل الدفعة: {(_project.ExecutionPhase.TransferIssued ? "تم تحويلها" : "معلّقة")}");
                            });
                            execCol.Item().PaddingTop(5).Row(row =>
                            {
                                row.RelativeItem().Text($"تاريخ إرسال العقد للمنفذ: {_project.ExecutionPhase.ContractSentDate?.ToString("dd/MM/yyyy") ?? "-"}");
                                row.RelativeItem().Text($"تاريخ إرسال التقرير النهائي: {_project.ExecutionPhase.FinalReportDate?.ToString("dd/MM/yyyy") ?? "-"}");
                            });
                            if (!string.IsNullOrEmpty(_project.ExecutionPhase.Notes))
                            {
                                execCol.Item().PaddingTop(5).Text($"ملاحظات التنفيذ: {_project.ExecutionPhase.Notes}");
                            }
                        });
                    }
                    else
                    {
                        col.Item().Text("لا توجد بيانات لمرحلة التنفيذ المالي والميداني بعد.").FontColor(Colors.Grey.Darken1).Italic();
                    }

                    // 4. Payments Section
                    col.Item().PaddingTop(15).PaddingBottom(5).Text("4. سجل الدفعات والتحصيلات").FontSize(12).Bold().FontColor(Colors.Blue.Darken2);
                    if (_project.Payments != null && _project.Payments.Count > 0)
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);
                                columns.ConstantColumn(100);
                                columns.ConstantColumn(100);
                                columns.ConstantColumn(120);
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("م").FontColor(Colors.White).Bold().FontSize(9);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("التاريخ").FontColor(Colors.White).Bold().FontSize(9);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("المبلغ").FontColor(Colors.White).Bold().FontSize(9);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("طريقة الدفع").FontColor(Colors.White).Bold().FontSize(9);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("ملاحظات").FontColor(Colors.White).Bold().FontSize(9);
                            });

                            for (int i = 0; i < _project.Payments.Count; i++)
                            {
                                var pay = _project.Payments[i];
                                var rowColor = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                                table.Cell().Background(rowColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text((i + 1).ToString()).FontSize(8);
                                table.Cell().Background(rowColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(pay.PaymentDate.ToString("dd/MM/yyyy")).FontSize(8);
                                table.Cell().Background(rowColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text($"{pay.PaymentAmount:N2} SAR").FontSize(8);
                                table.Cell().Background(rowColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(pay.PaymentMethod ?? "-").FontSize(8);
                                table.Cell().Background(rowColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text(pay.Notes ?? "-").FontSize(8);
                            }
                        });
                    }
                    else
                    {
                        col.Item().Text("لا توجد دفعات مالية مسجلة.").FontColor(Colors.Grey.Darken1).Italic();
                    }

                    // 5. Documents Section
                    col.Item().PaddingTop(15).PaddingBottom(5).Text("5. المرفقات والأرشيف الإلكتروني (المستندات)").FontSize(12).Bold().FontColor(Colors.Blue.Darken2);
                    if (_project.Documents != null && _project.Documents.Count > 0)
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);
                                columns.ConstantColumn(120);
                                columns.RelativeColumn();
                                columns.ConstantColumn(100);
                                columns.ConstantColumn(80);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("م").FontColor(Colors.White).Bold().FontSize(9);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("نوع المستند").FontColor(Colors.White).Bold().FontSize(9);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("اسم الملف").FontColor(Colors.White).Bold().FontSize(9);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("الحجم").FontColor(Colors.White).Bold().FontSize(9);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("تاريخ الرفع").FontColor(Colors.White).Bold().FontSize(9);
                            });

                            for (int i = 0; i < _project.Documents.Count; i++)
                            {
                                var doc = _project.Documents[i];
                                var rowColor = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                                table.Cell().Background(rowColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text((i + 1).ToString()).FontSize(8);
                                table.Cell().Background(rowColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(doc.DocumentType).FontSize(8);
                                table.Cell().Background(rowColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text(doc.DocumentName).FontSize(8);
                                table.Cell().Background(rowColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(doc.FileSize.HasValue ? $"{(doc.FileSize.Value / 1024D):N2} KB" : "-").FontSize(8);
                                table.Cell().Background(rowColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(doc.UploadedAt.ToString("dd/MM/yyyy")).FontSize(8);
                            }
                        });
                    }
                    else
                    {
                        col.Item().Text("لا توجد مستندات أو صور مرفقة بالأرشيف.").FontColor(Colors.Grey.Darken1).Italic();
                    }

                    // 6. Milestones Section
                    col.Item().PaddingTop(15).PaddingBottom(5).Text("6. مراحل تنفيذ المشروع (الخطوات / المعالم الميدانية)").FontSize(12).Bold().FontColor(Colors.Blue.Darken2);
                    if (_project.Milestones != null && _project.Milestones.Count > 0)
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);
                                columns.RelativeColumn();
                                columns.ConstantColumn(80);
                                columns.ConstantColumn(80);
                                columns.ConstantColumn(80);
                                columns.ConstantColumn(80);
                                columns.ConstantColumn(80);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("م").FontColor(Colors.White).Bold().FontSize(9);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("اسم المرحلة / الخطوة").FontColor(Colors.White).Bold().FontSize(9);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("المدة المقدرة").FontColor(Colors.White).Bold().FontSize(9);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("تاريخ البدء المتوقع").FontColor(Colors.White).Bold().FontSize(9);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("تاريخ الانتهاء المتوقع").FontColor(Colors.White).Bold().FontSize(9);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("تاريخ الإنجاز الفعلي").FontColor(Colors.White).Bold().FontSize(9);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("الحالة").FontColor(Colors.White).Bold().FontSize(9);
                            });

                            for (int i = 0; i < _project.Milestones.Count; i++)
                            {
                                var m = _project.Milestones[i];
                                var rowColor = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                                table.Cell().Background(rowColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text((i + 1).ToString()).FontSize(8);
                                table.Cell().Background(rowColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text(m.MilestoneName).FontSize(8);
                                table.Cell().Background(rowColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text($"{m.Duration} {m.DurationUnit}").FontSize(8);
                                table.Cell().Background(rowColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(m.TargetStartDate?.ToString("dd/MM/yyyy") ?? "-").FontSize(8);
                                table.Cell().Background(rowColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(m.TargetEndDate?.ToString("dd/MM/yyyy") ?? "-").FontSize(8);
                                table.Cell().Background(rowColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(m.CompletionDate?.ToString("dd/MM/yyyy") ?? "-").FontSize(8);
                                table.Cell().Background(rowColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(m.IsCompleted ? "مكتملة" : "تحت التنفيذ").FontSize(8);
                            }
                        });
                    }
                    else
                    {
                        col.Item().Text("لا توجد مراحل أو خطوات تنفيذ مسجلة.").FontColor(Colors.Grey.Darken1).Italic();
                    }

                    // 7. Assessments Section
                    col.Item().PaddingTop(15).PaddingBottom(5).Text("7. سجل التقييمات الفنية والميدانية للمشروع").FontSize(12).Bold().FontColor(Colors.Blue.Darken2);
                    if (_project.Assessments != null && _project.Assessments.Count > 0)
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);
                                columns.ConstantColumn(100);
                                columns.ConstantColumn(120);
                                columns.ConstantColumn(100);
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("م").FontColor(Colors.White).Bold().FontSize(9);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("التاريخ").FontColor(Colors.White).Bold().FontSize(9);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("المقيّم").FontColor(Colors.White).Bold().FontSize(9);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("المبلغ المقدّر").FontColor(Colors.White).Bold().FontSize(9);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("ملاحظات التقييم").FontColor(Colors.White).Bold().FontSize(9);
                            });

                            for (int i = 0; i < _project.Assessments.Count; i++)
                            {
                                var ass = _project.Assessments[i];
                                var rowColor = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                                table.Cell().Background(rowColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text((i + 1).ToString()).FontSize(8);
                                table.Cell().Background(rowColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(ass.AssessmentDate.ToString("dd/MM/yyyy")).FontSize(8);
                                table.Cell().Background(rowColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(ass.AssessedBy ?? "-").FontSize(8);
                                table.Cell().Background(rowColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(ass.AssessmentAmount.HasValue ? $"{ass.AssessmentAmount.Value:N2} SAR" : "-").FontSize(8);
                                table.Cell().Background(rowColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text(ass.AssessmentNotes ?? "-").FontSize(8);
                            }
                        });
                    }
                    else
                    {
                        col.Item().Text("لا توجد تقييمات ميدانية مسجلة للمشروع.").FontColor(Colors.Grey.Darken1).Italic();
                    }

                    // 8. Price Quotes Section
                    col.Item().PaddingTop(15).PaddingBottom(5).Text("8. عروض الأسعار والمنافسات المقدمة").FontSize(12).Bold().FontColor(Colors.Blue.Darken2);
                    if (_project.PriceQuotes != null && _project.PriceQuotes.Count > 0)
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);
                                columns.ConstantColumn(80);
                                columns.ConstantColumn(100);
                                columns.RelativeColumn();
                                columns.ConstantColumn(100);
                                columns.ConstantColumn(70);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("م").FontColor(Colors.White).Bold().FontSize(9);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("التاريخ").FontColor(Colors.White).Bold().FontSize(9);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("رقم العرض").FontColor(Colors.White).Bold().FontSize(9);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("اسم الشركة / المورد").FontColor(Colors.White).Bold().FontSize(9);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("مبلغ العرض").FontColor(Colors.White).Bold().FontSize(9);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("الحالة").FontColor(Colors.White).Bold().FontSize(9);
                            });

                            for (int i = 0; i < _project.PriceQuotes.Count; i++)
                            {
                                var quote = _project.PriceQuotes[i];
                                var rowColor = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                                table.Cell().Background(rowColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text((i + 1).ToString()).FontSize(8);
                                table.Cell().Background(rowColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(quote.QuoteDate.ToString("dd/MM/yyyy")).FontSize(8);
                                table.Cell().Background(rowColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(quote.QuoteNumber ?? "-").FontSize(8);
                                table.Cell().Background(rowColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text(quote.CompanyName ?? "-").FontSize(8);
                                table.Cell().Background(rowColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(quote.QuoteAmount.HasValue ? $"{quote.QuoteAmount.Value:N2} SAR" : "-").FontSize(8);
                                table.Cell().Background(rowColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(quote.IsAccepted ? "مقبول" : "تحت الدراسة").FontSize(8);
                            }
                        });
                    }
                    else
                    {
                        col.Item().Text("لا توجد عروض أسعار أو منافسات مسجلة للمشروع.").FontColor(Colors.Grey.Darken1).Italic();
                    }

                    // 9. Financial Account Statement Section
                    col.Item().PaddingTop(15).PaddingBottom(5).Text("9. كشف الحساب المالي للمشروع").FontSize(12).Bold().FontColor(Colors.Blue.Darken2);
                    if (_project.StatementEntries != null && _project.StatementEntries.Count > 0)
                    {
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(60);
                                columns.ConstantColumn(70);
                                columns.ConstantColumn(70);
                                columns.RelativeColumn();
                                columns.ConstantColumn(70);
                                columns.ConstantColumn(70);
                                columns.ConstantColumn(70);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("التاريخ").FontColor(Colors.White).Bold().FontSize(9);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("نوع الحركة").FontColor(Colors.White).Bold().FontSize(9);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("المرجع").FontColor(Colors.White).Bold().FontSize(9);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("البيان").FontColor(Colors.White).Bold().FontSize(9);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("الوارد (له)").FontColor(Colors.White).Bold().FontSize(9);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("الصادر (عليه)").FontColor(Colors.White).Bold().FontSize(9);
                                header.Cell().Background(Colors.Blue.Darken2).Padding(5).AlignCenter().Text("الرصيد").FontColor(Colors.White).Bold().FontSize(9);
                            });

                            decimal runningBalance = 0;
                            for (int i = 0; i < _project.StatementEntries.Count; i++)
                            {
                                var entry = _project.StatementEntries[i];
                                var rowColor = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;
                                runningBalance += (entry.IncomingAmount - entry.OutgoingAmount);

                                table.Cell().Background(rowColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(entry.Date.ToString("dd/MM/yyyy")).FontSize(8);
                                table.Cell().Background(rowColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(entry.Type).FontSize(8);
                                table.Cell().Background(rowColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(entry.ReferenceNumber ?? "-").FontSize(8);
                                table.Cell().Background(rowColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text(entry.Description ?? "-").FontSize(8);
                                table.Cell().Background(rowColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(entry.IncomingAmount > 0 ? $"{entry.IncomingAmount:N2} SAR" : "-").FontSize(8);
                                table.Cell().Background(rowColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text(entry.OutgoingAmount > 0 ? $"{entry.OutgoingAmount:N2} SAR" : "-").FontSize(8);
                                table.Cell().Background(rowColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignCenter().Text($"{runningBalance:N2} SAR").FontSize(8).Bold();
                            }
                        });

                        // Statement Summary Box
                        decimal totalIncome = _project.StatementEntries.Sum(e => e.IncomingAmount);
                        decimal totalExpenses = _project.StatementEntries.Sum(e => e.OutgoingAmount);
                        decimal commRate = (_project.CommissionPercentage ?? 0) / 100;
                        decimal operationalFee = totalIncome * commRate;
                        decimal rawBalance = totalIncome - totalExpenses;
                        decimal netBalance = rawBalance - operationalFee;
                        decimal totalBudget = _project.TotalAmount ?? 0;
                        decimal remainingBudget = Math.Max(0, totalBudget - totalIncome);

                        col.Item().PaddingTop(10).Row(row =>
                        {
                            row.RelativeItem().Text(""); // Spacer
                            row.ConstantItem(300).Border(1).BorderColor(Colors.Blue.Darken2).Background(Colors.Grey.Lighten5).Padding(8).Column(sumCol =>
                            {
                                sumCol.Item().Row(r => { r.RelativeItem().Text("إجمالي الوارد (التحصيلات):").FontSize(8); r.ConstantItem(100).AlignRight().Text($"{totalIncome:N2} SAR").FontSize(8).Bold(); });
                                sumCol.Item().Row(r => { r.RelativeItem().Text("إجمالي الصادر (المصروفات):").FontSize(8); r.ConstantItem(100).AlignRight().Text($"{totalExpenses:N2} SAR").FontSize(8).Bold(); });
                                sumCol.Item().Row(r => { r.RelativeItem().Text("العمولة التشغيلية للجمعية:").FontSize(8); r.ConstantItem(100).AlignRight().Text($"{operationalFee:N2} SAR").FontSize(8).Bold(); });
                                sumCol.Item().Row(r => { r.RelativeItem().Text("الرصيد المتاح للتشغيل:").FontSize(8).FontColor(Colors.Blue.Darken3); r.ConstantItem(100).AlignRight().Text($"{netBalance:N2} SAR").FontSize(8).Bold().FontColor(Colors.Blue.Darken3); });
                                sumCol.Item().PaddingVertical(2).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                                sumCol.Item().Row(r => { r.RelativeItem().Text("المتبقي من الميزانية المعتمدة:").FontSize(8).FontColor(Colors.Red.Darken2); r.ConstantItem(100).AlignRight().Text($"{remainingBudget:N2} SAR").FontSize(8).Bold().FontColor(Colors.Red.Darken2); });
                            });
                        });
                    }
                    else
                    {
                        col.Item().Text("لا توجد قيود أو حركات مالية مسجلة بكشف الحساب.").FontColor(Colors.Grey.Darken1).Italic();
                    }

                    // 10. Attachment Images Section
                    if (_project.Documents != null && _project.Documents.Count > 0)
                    {
                        var imageDocs = _project.Documents
                            .Where(d => !string.IsNullOrEmpty(d.FilePath) && 
                                        ((d.MimeType != null && d.MimeType.StartsWith("image/")) ||
                                         d.FilePath.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                                         d.FilePath.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                                         d.FilePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                                         d.FilePath.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) ||
                                         d.FilePath.EndsWith(".webp", StringComparison.OrdinalIgnoreCase) ||
                                         d.FilePath.EndsWith(".bmp", StringComparison.OrdinalIgnoreCase)))
                            .ToList();

                        if (imageDocs.Any())
                        {
                            col.Item().PageBreak();
                            col.Item().PaddingTop(15).PaddingBottom(5).Text("10. صور المستندات والمرفقات").FontSize(12).Bold().FontColor(Colors.Blue.Darken2);

                            foreach (var imgDoc in imageDocs)
                            {
                                col.Item().PaddingTop(10).Column(imgCol =>
                                {
                                    imgCol.Item().Text($"{imgDoc.DocumentType} - {imgDoc.DocumentName}").FontSize(10).Bold().FontColor(Colors.Grey.Darken3);
                                    
                                    byte[]? imageBytes = null;
                                    try
                                    {
                                        if (imgDoc.FilePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || 
                                            imgDoc.FilePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                                        {
                                            using (var webClient = new System.Net.Http.HttpClient())
                                            {
                                                imageBytes = webClient.GetByteArrayAsync(imgDoc.FilePath).GetAwaiter().GetResult();
                                            }
                                        }
                                        else
                                        {
                                            var localPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", imgDoc.FilePath.TrimStart('/'));
                                            if (System.IO.File.Exists(localPath))
                                            {
                                                imageBytes = System.IO.File.ReadAllBytes(localPath);
                                            }
                                        }
                                    }
                                    catch (Exception)
                                    {
                                        // Ignore
                                    }

                                    if (imageBytes != null)
                                    {
                                        imgCol.Item().MaxWidth(500).MaxHeight(350).Image(imageBytes);
                                    }
                                    else
                                    {
                                        imgCol.Item().Text($"تعذر تحميل الصورة: {imgDoc.FilePath}").FontSize(8).Italic().FontColor(Colors.Red.Medium);
                                    }
                                    
                                    imgCol.Item().PaddingVertical(5).LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                                });
                            }
                        }
                    }
                });

                // Footer
                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("صفحة ").FontSize(8);
                    text.CurrentPageNumber().FontSize(8);
                    text.Span(" من ").FontSize(8);
                    text.TotalPages().FontSize(8);
                });
            });
        }
    }
}
