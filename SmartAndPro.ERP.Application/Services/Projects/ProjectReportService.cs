using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using SmartAndPro.ERP.Application.IServices.Projects;
using SmartAndPro.ERP.Application.Services.Projects.DocumentUtils;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Domains.Entities.Projects;
using SmartAndPro.ERP.Infrastructure.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartAndPro.ERP.Application.Services.Projects
{
    public class ProjectReportService : IProjectReportService
    {
        private readonly ERPContext _context;
        private readonly IProjectService _projectService;

        public ProjectReportService(ERPContext context, IProjectService projectService)
        {
            _context = context;
            _projectService = projectService;
        }

        public async Task<byte[]> GenerateAccountStatementPdfAsync(Guid projectId)
        {
            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == projectId && !p.IsDeleted);

            if (project == null)
                throw new Exception("المشروع غير موجود");

            // Fetch company details associated with the project, or fallback to the first active company
            CompanyDetails? companyDetails = null;
            if (project.CompanyDetailsId.HasValue)
            {
                companyDetails = await _context.CompanyDetails.FirstOrDefaultAsync(c => c.Id == project.CompanyDetailsId.Value);
            }
            if (companyDetails == null)
            {
                companyDetails = await _context.CompanyDetails.FirstOrDefaultAsync();
            }
            if (companyDetails == null)
            {
                companyDetails = new CompanyDetails
                {
                    ArabicName = "مؤسسة تنفيذ المشاريع",
                    EnglishName = "Project Execution Entity"
                };
            }

            // Fetch connected vouchers (Receipts & Payments connections)
            var connections = await _context.ReceiptConnections
                .Include(rc => rc.PaycheckReceipt)
                .Include(rc => rc.PaymentReceipt)
                .Where(rc => rc.ProjectId == projectId && rc.IsActive == true)
                .ToListAsync();

            var entries = new List<ProjectStatementEntry>();

            foreach (var rc in connections)
            {
                if (rc.PaycheckReceipt != null)
                {
                    entries.Add(new ProjectStatementEntry
                    {
                        Date = rc.PaycheckReceipt.ReceiptDate.ToDateTime(TimeOnly.MinValue),
                        Type = "سند قبض",
                        ReferenceNumber = rc.PaycheckReceipt.ReferenceNumber ?? "",
                        Description = rc.PaycheckReceipt.Description ?? "سند قبض مالي مرتبط بالمشروع",
                        IncomingAmount = rc.LinkedAmount,
                        OutgoingAmount = 0
                    });
                }
                else if (rc.PaymentReceipt != null)
                {
                    entries.Add(new ProjectStatementEntry
                    {
                        Date = rc.PaymentReceipt.ReceiptDate.ToDateTime(TimeOnly.MinValue),
                        Type = "سند صرف",
                        ReferenceNumber = rc.PaymentReceipt.ReferenceNumber ?? "",
                        Description = rc.PaymentReceipt.Description ?? "سند صرف مالي مرتبط بالمشروع",
                        IncomingAmount = 0,
                        OutgoingAmount = rc.LinkedAmount
                    });
                }
            }

            // Fetch direct payments
            var directPayments = await _context.ProjectPayments
                .Where(p => p.ProjectId == projectId && p.IsActive == true)
                .ToListAsync();

            foreach (var dp in directPayments)
            {
                entries.Add(new ProjectStatementEntry
                {
                    Date = dp.PaymentDate,
                    Type = "دفعة مصروفة",
                    ReferenceNumber = dp.PaymentReference ?? "",
                    Description = dp.Notes ?? "دفعة مسجلة مباشرة بالمشروع",
                    IncomingAmount = 0,
                    OutgoingAmount = dp.PaymentAmount
                });
            }

            // Sort entries by date
            entries = entries.OrderBy(e => e.Date).ToList();

            SmartAndPro.ERP.Domains.Entities.PdfTemplateConfiguration? pdfTemplateConfig = null;
            try
            {
                pdfTemplateConfig = await _context.Set<SmartAndPro.ERP.Domains.Entities.PdfTemplateConfiguration>().FirstOrDefaultAsync();
            }
            catch (Exception)
            {
                // Fallback
            }

            var document = new ProjectAccountStatementDocument(project, entries, companyDetails!, pdfTemplateConfig);
            return document.GeneratePdf();
        }

        public async Task<byte[]> GenerateProjectArchivePdfAsync(Guid projectId)
        {
            var projectDetails = await _projectService.GetProjectDetailsAsync(projectId);
            if (projectDetails == null)
                throw new Exception("المشروع غير موجود");

            // Fetch company details associated with the project, or fallback to the first active company
            CompanyDetails? companyDetails = null;
            if (projectDetails.CompanyDetailsId.HasValue)
            {
                companyDetails = await _context.CompanyDetails.FirstOrDefaultAsync(c => c.Id == projectDetails.CompanyDetailsId.Value);
            }
            if (companyDetails == null)
            {
                companyDetails = await _context.CompanyDetails.FirstOrDefaultAsync();
            }
            if (companyDetails == null)
            {
                companyDetails = new CompanyDetails
                {
                    ArabicName = "مؤسسة تنفيذ المشاريع",
                    EnglishName = "Project Execution Entity"
                };
            }

            var document = new ProjectArchiveDocument(projectDetails, companyDetails!);
            return document.GeneratePdf();
        }
    }
}
