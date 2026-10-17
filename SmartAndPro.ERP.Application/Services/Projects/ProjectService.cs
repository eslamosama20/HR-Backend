using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.Projects;
using SmartAndPro.ERP.Application.IServices.Projects;
using SmartAndPro.ERP.Domains.Entities.Projects;
using SmartAndPro.ERP.Domains.Entities.Inventory.CompanyManagement;
using SmartAndPro.ERP.Domains.Entities.Receipts;
using SmartAndPro.ERP.Domains.Entities.Accounting;
using SmartAndPro.ERP.Domains.Entities.Treasuries;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.Infrastructure.Contexts;

namespace SmartAndPro.ERP.Application.Services.Projects;

public class ProjectService : IProjectService
{
    private readonly ERPContext _context;

    public ProjectService(ERPContext context)
    {
        _context = context;
    }

    public async Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest request, Guid currentUserId)
    {
        decimal sumChecks = request.Checks?.Sum(c => c.CheckAmount) ?? 0;
        if (request.TotalAmount.HasValue && sumChecks > request.TotalAmount.Value)
        {
            throw new System.Exception("مجموع مبالغ الشيكات لا يمكن أن يتجاوز المبلغ الإجمالي للمشروع");
        }

        var creatorName = await GetUserNameAsync(currentUserId);
        var project = new Project
        {
            Id = Guid.NewGuid(),
            ProjectName = request.ProjectName,
            ProjectNumber = request.ProjectNumber,
            FileNumber = request.FileNumber,
            Status = request.Status,
            CreatedBy = !string.IsNullOrWhiteSpace(request.CreatedBy) ? request.CreatedBy : creatorName,
            IsActive = true,
            IsFinancialSolvency = request.IsFinancialSolvency,
            TotalAmount = request.TotalAmount,
            ExecutionAmount = request.ExecutionAmount,
            ProfitPercentage = request.ProfitPercentage,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            Country = request.Country,
            CommissionPercentage = request.CommissionPercentage,
            CheckNumber = request.CheckNumber,
            CheckReceiptDate = request.CheckReceiptDate,
            CheckAmount = request.CheckAmount,
            ClearingDetails = request.ClearingDetails,
            CompanyDetailsId = request.CompanyDetailsId,
            ShareToken = string.IsNullOrEmpty(request.ShareToken) ? Guid.NewGuid().ToString("N") : request.ShareToken,
            DepartmentName = request.DepartmentName,
            SupervisingEntityName = request.SupervisingEntityName,
            ExecutingCompanyName = request.ExecutingCompanyName,
            QuoteNumber = request.QuoteNumber,
            QuoteDate = request.QuoteDate,
            AssignmentNumber = request.AssignmentNumber,
            AssignmentApprovalDate = request.AssignmentApprovalDate,
            ExecutionDuration = request.ExecutionDuration,
            ExecutionDurationUnit = request.ExecutionDurationUnit
        };

        _context.Projects.Add(project);

        if (request.Milestones != null && request.Milestones.Count > 0)
        {
            ValidateMilestonesDuration(request.ExecutionDuration, request.ExecutionDurationUnit, request.Milestones.Select(m => m.Duration).ToList(), request.Milestones.Select(m => m.DurationUnit).ToList());

            int seq = 1;
            foreach (var m in request.Milestones)
            {
                var milestone = new ProjectExecutionMilestone
                {
                    Id = Guid.NewGuid(),
                    ProjectId = project.Id,
                    MilestoneName = m.MilestoneName,
                    Duration = m.Duration,
                    DurationUnit = m.DurationUnit,
                    SequenceOrder = m.SequenceOrder == 0 ? seq++ : m.SequenceOrder,
                    IsCompleted = false,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = currentUserId
                };
                _context.ProjectExecutionMilestones.Add(milestone);
            }
        }
        
        // Add history entry
        var historyEntry = new ProjectHistory
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            ActionType = "إنشاء",
            ActionBy = await GetUserNameAsync(currentUserId),
            ActionDate = DateTime.UtcNow,
            NewValue = $"تم إنشاء المشروع: {project.ProjectName}",
            Notes = "إنشاء مشروع جديد"
        };
        _context.ProjectHistories.Add(historyEntry);

        if (request.Checks != null && request.Checks.Count > 0)
        {
            await SyncProjectChecksAsync(project.Id, request.Checks, null, currentUserId);
        }

        await _context.SaveChangesAsync();

        return MapToProjectResponse(project);
    }

    public async Task<ProjectResponse> UpdateProjectAsync(UpdateProjectRequest request, Guid currentUserId)
    {
        decimal sumChecks = request.Checks?.Sum(c => c.CheckAmount) ?? 0;
        if (request.TotalAmount.HasValue && sumChecks > request.TotalAmount.Value)
        {
            throw new System.Exception("مجموع مبالغ الشيكات لا يمكن أن يتجاوز المبلغ الإجمالي للمشروع");
        }

        var project = await _context.Projects.FindAsync(request.Id);
        if (project == null)
            throw new Exception("المشروع غير موجود");

        var changesOld = new List<string>();
        var changesNew = new List<string>();

        if (project.ProjectName != request.ProjectName)
        {
            changesOld.Add($"اسم المشروع: {project.ProjectName}");
            changesNew.Add($"اسم المشروع: {request.ProjectName}");
        }
        if (project.ProjectNumber != request.ProjectNumber)
        {
            changesOld.Add($"رقم المشروع: {project.ProjectNumber}");
            changesNew.Add($"رقم المشروع: {request.ProjectNumber}");
        }
        if (project.FileNumber != request.FileNumber)
        {
            changesOld.Add($"رقم الملف: {project.FileNumber}");
            changesNew.Add($"رقم الملف: {request.FileNumber}");
        }
        if (project.Status != request.Status)
        {
            changesOld.Add($"حالة المشروع: {project.Status}");
            changesNew.Add($"حالة المشروع: {request.Status}");
        }
        if (project.TotalAmount != request.TotalAmount)
        {
            changesOld.Add($"إجمالي الميزانية: {project.TotalAmount:N2}");
            changesNew.Add($"إجمالي الميزانية: {request.TotalAmount:N2}");
        }
        if (project.CommissionPercentage != request.CommissionPercentage)
        {
            changesOld.Add($"نسبة تشغيل الهيئة: {project.CommissionPercentage}");
            changesNew.Add($"نسبة تشغيل الهيئة: {request.CommissionPercentage}");
        }
        if (project.ExecutionAmount != request.ExecutionAmount)
        {
            changesOld.Add($"صافي ميزانية التنفيذ: {project.ExecutionAmount:N2}");
            changesNew.Add($"صافي ميزانية التنفيذ: {request.ExecutionAmount:N2}");
        }
        if (project.IsActive != request.IsActive)
        {
            changesOld.Add($"نشط: {project.IsActive}");
            changesNew.Add($"نشط: {request.IsActive}");
        }

        var oldValue = changesOld.Count > 0 ? string.Join("\n", changesOld) : $"الاسم: {project.ProjectName}, الحالة: {project.Status}";
        var newValue = changesNew.Count > 0 ? string.Join("\n", changesNew) : $"الاسم: {request.ProjectName}, الحالة: {request.Status}";

        project.ProjectName = request.ProjectName;
        project.ProjectNumber = request.ProjectNumber;
        project.FileNumber = request.FileNumber;

        // اعتماد الحالة المختارة يدويًا من القائمة المنسدلة بشاشة التعديل وتثبيتها بالمشروع
        var requestedStatus = request.Status?.Trim();
        if (!string.IsNullOrWhiteSpace(requestedStatus))
        {
            project.Status = requestedStatus;
        }
        else
        {
            project.Status = DetermineCalculatedStatus(project);
        }

        project.IsActive = request.IsActive;
        project.IsFinancialSolvency = request.IsFinancialSolvency;
        project.TotalAmount = request.TotalAmount;
        project.ExecutionAmount = request.ExecutionAmount;
        project.ProfitPercentage = request.ProfitPercentage;
        project.Country = request.Country;
        project.CommissionPercentage = request.CommissionPercentage;
        project.CheckNumber = request.CheckNumber;
        project.CheckReceiptDate = request.CheckReceiptDate;
        project.CheckAmount = request.CheckAmount;
        project.ClearingDetails = request.ClearingDetails;
        project.CompanyDetailsId = request.CompanyDetailsId;
        project.DepartmentName = request.DepartmentName;
        project.SupervisingEntityName = request.SupervisingEntityName;
        project.ExecutingCompanyName = request.ExecutingCompanyName;
        project.QuoteNumber = request.QuoteNumber;
        project.QuoteDate = request.QuoteDate;
        project.AssignmentNumber = request.AssignmentNumber;
        project.AssignmentApprovalDate = request.AssignmentApprovalDate;
        project.ExecutionDuration = request.ExecutionDuration;
        project.ExecutionDurationUnit = request.ExecutionDurationUnit;

        // 2-Way Sync to Assignment Phase entity
        var assignmentPhase = await _context.ProjectAssignmentPhases
            .FirstOrDefaultAsync(ap => ap.ProjectId == project.Id && ap.IsActive);
        if (assignmentPhase != null)
        {
            assignmentPhase.ExecutingEntityName = request.ExecutingCompanyName;
            assignmentPhase.SupervisingEntityName = request.SupervisingEntityName;
            assignmentPhase.DepartmentName = request.DepartmentName;
            assignmentPhase.AssignmentApprovalDate = request.AssignmentApprovalDate;
            assignmentPhase.ExecutionDuration = request.ExecutionDuration;
            assignmentPhase.ExecutionDurationUnit = request.ExecutionDurationUnit;
        }

        var milestonesToSync = await _context.ProjectExecutionMilestones
            .Where(m => m.ProjectId == project.Id && m.IsActive)
            .ToListAsync();
        if (milestonesToSync.Any() && !string.IsNullOrWhiteSpace(request.ExecutionDurationUnit))
        {
            foreach (var m in milestonesToSync)
            {
                m.DurationUnit = request.ExecutionDurationUnit;
            }
        }

        // 2-Way Sync to Execution Phase entity
        var executionPhase = await _context.ProjectExecutionPhases
            .FirstOrDefaultAsync(ep => ep.ProjectId == project.Id && ep.IsActive);
        if (executionPhase != null)
        {
            if (!string.IsNullOrWhiteSpace(request.ExecutingCompanyName))
            {
                executionPhase.CompanyName = request.ExecutingCompanyName;
            }
            if (request.TotalAmount.HasValue)
            {
                executionPhase.GrantAmount = request.TotalAmount.Value;
            }
            if (request.ExecutionAmount.HasValue)
            {
                executionPhase.ExecutionAmount = request.ExecutionAmount.Value;
            }
            if (request.TotalAmount.HasValue && request.ExecutionAmount.HasValue)
            {
                executionPhase.RemainingAmount = request.TotalAmount.Value - request.ExecutionAmount.Value;
            }
        }

        if (string.IsNullOrEmpty(project.ShareToken))
        {
            project.ShareToken = Guid.NewGuid().ToString("N");
        }

        // Add history entry
        var historyEntry = new ProjectHistory
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            ActionType = "تعديل",
            ActionDate = DateTime.UtcNow,
            OldValue = oldValue,
            NewValue = newValue,
            ActionBy = await GetUserNameAsync(currentUserId),
            Notes = "تعديل بيانات المشروع"
        };
        _context.ProjectHistories.Add(historyEntry);

        if (request.Checks != null)
        {
            await SyncProjectChecksAsync(project.Id, request.Checks, request.DeletedChecks, currentUserId);
        }

        if (request.Milestones != null)
        {
            if (request.Milestones.Count > 0)
            {
                ValidateMilestonesDuration(request.ExecutionDuration, request.ExecutionDurationUnit, request.Milestones.Select(m => m.Duration).ToList(), request.Milestones.Select(m => m.DurationUnit).ToList());
            }

            var existingMilestones = await _context.ProjectExecutionMilestones
                .Where(m => m.ProjectId == project.Id && m.IsActive)
                .ToListAsync();

            // Deletion
            var requestIds = request.Milestones.Where(m => m.Id != Guid.Empty).Select(m => m.Id).ToList();
            foreach (var existing in existingMilestones)
            {
                if (!requestIds.Contains(existing.Id))
                {
                    existing.IsActive = false;
                }
            }

            // Updates & Additions
            int seq = 1;
            foreach (var m in request.Milestones)
            {
                if (m.Id != Guid.Empty)
                {
                    var existing = existingMilestones.FirstOrDefault(em => em.Id == m.Id);
                    if (existing != null)
                    {
                        existing.MilestoneName = m.MilestoneName;
                        existing.Duration = m.Duration;
                        existing.DurationUnit = m.DurationUnit;
                        existing.SequenceOrder = m.SequenceOrder == 0 ? seq++ : m.SequenceOrder;
                    }
                }
                else
                {
                    var newMilestone = new ProjectExecutionMilestone
                    {
                        Id = Guid.NewGuid(),
                        ProjectId = project.Id,
                        MilestoneName = m.MilestoneName,
                        Duration = m.Duration,
                        DurationUnit = m.DurationUnit,
                        SequenceOrder = m.SequenceOrder == 0 ? seq++ : m.SequenceOrder,
                        IsCompleted = false,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = currentUserId
                    };
                    _context.ProjectExecutionMilestones.Add(newMilestone);
                }
            }

            var assignment = await _context.ProjectAssignmentPhases.FirstOrDefaultAsync(ap => ap.ProjectId == project.Id && ap.IsActive);
            await RecalculateMilestoneDatesAsync(project.Id, assignment?.ExecutionStartDate);
        }

        await _context.SaveChangesAsync();

        return MapToProjectResponse(project);
    }

    public async Task<bool> DeleteProjectAsync(Guid id, Guid currentUserId)
    {
        var project = await _context.Projects.FindAsync(id);
        if (project == null)
            return false;

        project.IsDeleted = true;
        project.IsActive = false;

        // Add history entry
        var historyEntry = new ProjectHistory
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            ActionType = "حذف",
            ActionBy = await GetUserNameAsync(currentUserId),
            ActionDate = DateTime.UtcNow,
            NewValue = "تم حذف المشروع",
            Notes = "حذف المشروع"
        };
        _context.ProjectHistories.Add(historyEntry);

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ProjectResponse?> GetProjectByIdAsync(Guid id)
    {
        var project = await _context.Projects
            .Include(p => p.CompanyDetails)
            .Include(p => p.AssignmentPhase)
            .Include(p => p.ExecutionPhase)
            .Include(p => p.Documents)
            .Include(p => p.Payments)
            .Include(p => p.History)
            .Include(p => p.Milestones)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (project == null)
            return null;

        var response = MapToProjectResponse(project);
        response.CreatedBy = await ResolveCreatedByNameAsync(project);
        response.DocumentsCount = project.Documents?.Count ?? 0;
        response.PaymentsCount = project.Payments?.Count ?? 0;
        response.HistoryCount = project.History?.Count ?? 0;
        response.HasAssignmentPhase = project.AssignmentPhase != null;
        response.HasExecutionPhase = project.ExecutionPhase != null;

        return response;
    }

    public async Task<ProjectDetailsResponse?> GetProjectDetailsAsync(Guid id)
    {
        var project = await _context.Projects
            .AsNoTracking()
            .AsSplitQuery()
            .Include(p => p.CompanyDetails)
            .Include(p => p.AssignmentPhase)
                .ThenInclude(a => a.ExecutingEntity)
            .Include(p => p.AssignmentPhase)
                .ThenInclude(a => a.SupervisingEntity)
            .Include(p => p.AssignmentPhase)
                .ThenInclude(a => a.Department)
            .Include(p => p.ExecutionPhase)
                .ThenInclude(e => e.Company)
            .Include(p => p.Documents)
            .Include(p => p.Payments)
            .Include(p => p.Assessments)
                .ThenInclude(a => a.Document)
            .Include(p => p.PriceQuotes)
                .ThenInclude(q => q.Company)
            .Include(p => p.PriceQuotes)
                .ThenInclude(q => q.Document)
            .Include(p => p.PriceQuotes)
                .ThenInclude(q => q.SaleInvoice)
                    .ThenInclude(si => si.CustomerProfile)
                        .ThenInclude(cp => cp.Departments)
            .Include(p => p.History)
            .Include(p => p.Milestones)
            .FirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

        if (project == null)
            return null;

        var response = MapToProjectDetailsResponse(project);
        response.CreatedBy = await ResolveCreatedByNameAsync(project);

        // Fetch connected vouchers (Receipts & Payments connections)
        var connections = await _context.ReceiptConnections
            .AsNoTracking()
            .Include(rc => rc.PaycheckReceipt)
                .ThenInclude(pr => pr.PaycheckReceiptAccounts)
                    .ThenInclude(pra => pra.MainAccount)
            .Include(rc => rc.PaycheckReceipt)
                .ThenInclude(pr => pr.Treasury)
            .Include(rc => rc.PaymentReceipt)
                .ThenInclude(pr => pr.PaymentReceiptAccounts)
                    .ThenInclude(pra => pra.MainAccount)
            .Include(rc => rc.PaymentReceipt)
                .ThenInclude(pr => pr.Treasury)
            .Where(rc => rc.ProjectId == id && rc.IsActive == true)
            .ToListAsync();

        var allTreasuriesList = await _context.Treasuries
            .AsNoTracking()
            .ToListAsync();

        var bankAccountsList = await _context.GeneralBankAccounts
            .AsNoTracking()
            .ToListAsync();

        var checksList = new List<ProjectCheckDto>();
        foreach (var rc in connections.Where(c => c.PaycheckReceipt != null))
        {
            var pr = rc.PaycheckReceipt;
            var acc = pr.PaycheckReceiptAccounts?.FirstOrDefault();

            string treasuryOrBankName = pr.Treasury?.Name;
            if (string.IsNullOrWhiteSpace(treasuryOrBankName) && pr.TreasuryId != Guid.Empty)
            {
                treasuryOrBankName = allTreasuriesList.FirstOrDefault(t => t.Id == pr.TreasuryId)?.Name;
            }
            Guid returnedTreasuryId = pr.TreasuryId;

            if (pr.PaymentMethodType != Domains.Enums.PaymentMethodType.Cash)
            {
                var matchedBank = bankAccountsList.FirstOrDefault(b => 
                    b.Id == pr.TreasuryId || 
                    (!string.IsNullOrWhiteSpace(pr.BankName) && ((b.AccountName + " - " + b.BankName) == pr.BankName || b.AccountName == pr.BankName || b.BankName == pr.BankName))
                );

                if (matchedBank != null)
                {
                    returnedTreasuryId = matchedBank.Id;
                    treasuryOrBankName = string.IsNullOrWhiteSpace(matchedBank.BankName)
                        ? matchedBank.AccountName
                        : $"{matchedBank.AccountName} - {matchedBank.BankName}";
                }
                else if (!string.IsNullOrWhiteSpace(pr.BankName))
                {
                    treasuryOrBankName = pr.BankName;
                }
            }

            checksList.Add(new ProjectCheckDto
            {
                Id = pr.Id,
                CheckNumber = pr.ReferenceNumber,
                CheckReceiptDate = pr.ReceiptDate.ToDateTime(TimeOnly.MinValue),
                CheckAmount = rc.LinkedAmount,
                TreasuryId = returnedTreasuryId,
                TreasuryName = treasuryOrBankName,
                MainAccountId = acc?.MainAccountId ?? Guid.Empty,
                MainAccountName = acc?.MainAccount?.ArabicName,
                PaymentMethodType = (int?)pr.PaymentMethodType,
                BankName = pr.BankName,
                BankReferenceNumber = pr.BankReferenceNumber,
                VoucherNumber = pr.VoucherNumber,
                TransactionNumber = pr.TransactionNumber,
                CheckNumberDetail = pr.CheckNumber
            });
        }
        response.Checks = checksList;

        var entries = new List<ProjectStatementEntryResponse>();

        foreach (var rc in connections)
        {
            if (rc.PaycheckReceipt != null)
            {
                var pr = rc.PaycheckReceipt;
                string treasuryOrBankName = pr.Treasury?.Name;
                if (string.IsNullOrWhiteSpace(treasuryOrBankName) && pr.TreasuryId != Guid.Empty)
                {
                    treasuryOrBankName = allTreasuriesList.FirstOrDefault(t => t.Id == pr.TreasuryId)?.Name;
                }

                if (pr.PaymentMethodType != Domains.Enums.PaymentMethodType.Cash)
                {
                    var matchedBank = bankAccountsList.FirstOrDefault(b => 
                        b.Id == pr.TreasuryId || 
                        (!string.IsNullOrWhiteSpace(pr.BankName) && ((b.AccountName + " - " + b.BankName) == pr.BankName || b.AccountName == pr.BankName || b.BankName == pr.BankName))
                    );

                    if (matchedBank != null)
                    {
                        treasuryOrBankName = string.IsNullOrWhiteSpace(matchedBank.BankName)
                            ? matchedBank.AccountName
                            : $"{matchedBank.AccountName} - {matchedBank.BankName}";
                    }
                    else if (!string.IsNullOrWhiteSpace(pr.BankName))
                    {
                        treasuryOrBankName = pr.BankName;
                    }
                }
                if (string.IsNullOrWhiteSpace(treasuryOrBankName))
                {
                    treasuryOrBankName = "الخزينة العامة";
                }

                string paymentMethodStr = pr.PaymentMethodType switch
                {
                    Domains.Enums.PaymentMethodType.Cash => "نقدي",
                    Domains.Enums.PaymentMethodType.Cheque => "شيك",
                    Domains.Enums.PaymentMethodType.BankTransfer => "تحويل بنكي",
                    Domains.Enums.PaymentMethodType.Credit => "بطاقة",
                    Domains.Enums.PaymentMethodType.Deposit => "إيداع",
                    _ => "نقدي"
                };

                entries.Add(new ProjectStatementEntryResponse
                {
                    Date = pr.ReceiptDate.ToDateTime(TimeOnly.MinValue),
                    Type = "سند قبض",
                    ReferenceNumber = pr.ReferenceNumber ?? "",
                    Description = pr.Description ?? "سند قبض مالي مرتبط بالمشروع",
                    IncomingAmount = rc.LinkedAmount,
                    OutgoingAmount = 0,
                    TreasuryOrBankName = treasuryOrBankName,
                    PaymentMethod = paymentMethodStr,
                    PaymentMethodType = (int?)pr.PaymentMethodType
                });
            }
            else if (rc.PaymentReceipt != null)
            {
                var pr = rc.PaymentReceipt;
                string treasuryOrBankName = pr.Treasury?.Name;
                if (string.IsNullOrWhiteSpace(treasuryOrBankName) && pr.TreasuryId != Guid.Empty)
                {
                    treasuryOrBankName = allTreasuriesList.FirstOrDefault(t => t.Id == pr.TreasuryId)?.Name;
                }

                if (pr.PaymentMethodType != Domains.Enums.PaymentMethodType.Cash)
                {
                    var matchedBank = bankAccountsList.FirstOrDefault(b => 
                        b.Id == pr.TreasuryId || 
                        (!string.IsNullOrWhiteSpace(pr.BankName) && ((b.AccountName + " - " + b.BankName) == pr.BankName || b.AccountName == pr.BankName || b.BankName == pr.BankName))
                    );

                    if (matchedBank != null)
                    {
                        treasuryOrBankName = string.IsNullOrWhiteSpace(matchedBank.BankName)
                            ? matchedBank.AccountName
                            : $"{matchedBank.AccountName} - {matchedBank.BankName}";
                    }
                    else if (!string.IsNullOrWhiteSpace(pr.BankName))
                    {
                        treasuryOrBankName = pr.BankName;
                    }
                }
                if (string.IsNullOrWhiteSpace(treasuryOrBankName))
                {
                    treasuryOrBankName = "الخزينة العامة";
                }

                string paymentMethodStr = pr.PaymentMethodType switch
                {
                    Domains.Enums.PaymentMethodType.Cash => "نقدي",
                    Domains.Enums.PaymentMethodType.Cheque => "شيك",
                    Domains.Enums.PaymentMethodType.BankTransfer => "تحويل بنكي",
                    Domains.Enums.PaymentMethodType.Credit => "بطاقة",
                    Domains.Enums.PaymentMethodType.Deposit => "إيداع",
                    _ => "نقدي"
                };

                entries.Add(new ProjectStatementEntryResponse
                {
                    Date = pr.ReceiptDate.ToDateTime(TimeOnly.MinValue),
                    Type = "سند صرف",
                    ReferenceNumber = pr.ReferenceNumber ?? "",
                    Description = pr.Description ?? "سند صرف مالي مرتبط بالمشروع",
                    IncomingAmount = 0,
                    OutgoingAmount = rc.LinkedAmount,
                    TreasuryOrBankName = treasuryOrBankName,
                    PaymentMethod = paymentMethodStr,
                    PaymentMethodType = (int?)pr.PaymentMethodType
                });
            }
        }

        // Fetch direct payments
        var directPayments = await _context.ProjectPayments
            .Where(p => p.ProjectId == id && p.IsActive == true)
            .ToListAsync();

        foreach (var dp in directPayments)
        {
            entries.Add(new ProjectStatementEntryResponse
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
        response.StatementEntries = entries.OrderBy(e => e.Date).ToList();

        return response;
    }

    public async Task<List<ProjectResponse>> GetAllProjectsAsync(bool includeInactive = false)
    {
        var query = _context.Projects
            .Include(p => p.AssignmentPhase)
            .Include(p => p.ExecutionPhase)
            .Include(p => p.Milestones)
            .Where(p => !p.IsDeleted && p.Status != "Archived");

        if (!includeInactive)
            query = query.Where(p => p.IsActive);

        var projectsList = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();

        var resultList = new List<ProjectResponse>();
        bool hasChanges = false;

        foreach (var project in projectsList)
        {
            var response = MapToProjectResponse(project);
            response.CreatedBy = await ResolveCreatedByNameAsync(project);
            response.CompanyName = project.CompanyDetails?.ArabicName;
            response.DocumentsCount = project.Documents?.Count ?? 0;
            response.PaymentsCount = project.Payments?.Count ?? 0;
            response.HistoryCount = project.History?.Count ?? 0;
            response.HasAssignmentPhase = project.AssignmentPhase != null;
            response.HasExecutionPhase = project.ExecutionPhase != null;
            resultList.Add(response);
        }

        if (hasChanges)
        {
            await _context.SaveChangesAsync();
        }

        return resultList;
    }

    public async Task<List<ProjectResponse>> GetProjectsByStatusAsync(string status)
    {
        var projects = await _context.Projects
            .AsNoTracking()
            .Where(p => !p.IsDeleted && p.Status == status)
            .Select(p => new 
            {
                Project = p,
                CompanyName = p.CompanyDetails != null ? p.CompanyDetails.ArabicName : null,
                DocumentsCount = p.Documents.Count,
                PaymentsCount = p.Payments.Count,
                HistoryCount = p.History.Count,
                HasAssignmentPhase = p.AssignmentPhase != null,
                HasExecutionPhase = p.ExecutionPhase != null
            })
            .OrderByDescending(x => x.Project.CreatedAt)
            .ToListAsync();

        var resultList = new List<ProjectResponse>();
        foreach (var x in projects)
        {
            var response = MapToProjectResponse(x.Project);
            response.CreatedBy = await ResolveCreatedByNameAsync(x.Project);
            response.CompanyName = x.CompanyName;
            response.DocumentsCount = x.DocumentsCount;
            response.PaymentsCount = x.PaymentsCount;
            response.HistoryCount = x.HistoryCount;
            response.HasAssignmentPhase = x.HasAssignmentPhase;
            response.HasExecutionPhase = x.HasExecutionPhase;
            resultList.Add(response);
        }
        return resultList;
    }

    public async Task<bool> ChangeProjectStatusAsync(Guid id, string newStatus, Guid currentUserId)
    {
        var project = await _context.Projects.FindAsync(id);
        if (project == null)
            return false;

        var oldStatus = project.Status;
        project.Status = newStatus;

        // Add history entry
        var historyEntry = new ProjectHistory
        {
            Id = Guid.NewGuid(),
            ProjectId = project.Id,
            ActionType = "تغيير حالة",
            ActionBy = await GetUserNameAsync(currentUserId),
            ActionDate = DateTime.UtcNow,
            OldValue = $"الحالة السابقة: {oldStatus}",
            NewValue = $"الحالة الجديدة: {newStatus}",
            Notes = "تغيير حالة المشروع"
        };
        _context.ProjectHistories.Add(historyEntry);

        await _context.SaveChangesAsync();
        return true;
    }

    private string DetermineCalculatedStatus(Project project)
    {
        // 1. إذا كانت هناك مرحلة تنفيذ فعلية أو معالم مضافة أو مبلغ تنفيذ مسجل أكبر من صفر
        if (project.ExecutionPhase != null || (project.ExecutionAmount.HasValue && project.ExecutionAmount.Value > 0))
        {
            if (project.ExecutionPhase?.CompletionPercentage >= 100)
                return "منفذ";
            return "قيد التنفيذ";
        }

        if (project.Milestones != null && project.Milestones.Any(m => m.IsActive))
        {
            var activeMilestones = project.Milestones.Where(m => m.IsActive).ToList();
            if (activeMilestones.All(m => m.IsCompleted))
                return "منفذ";
            return "قيد التنفيذ";
        }

        // 2. إذا كانت هناك مرحلة إسناد فعلية أو تم تسجيل بيانات التعميد/الإسناد في المشروع
        if (project.AssignmentPhase != null || !string.IsNullOrWhiteSpace(project.ExecutingCompanyName) || project.AssignmentApprovalDate.HasValue || !string.IsNullOrWhiteSpace(project.AssignmentNumber))
        {
            return "قيد الإسناد";
        }

        // 3. إذا تضمنت حالة المشروع الحالية خيارات خاصة يدويًا كـ معلق/ملغي/مؤرشف
        if (project.Status == "معلق" || project.Status == "ملغي" || project.Status == "مؤرشف")
        {
            return project.Status;
        }

        // 4. الحالة الافتراضية لأي مشروع مضاف بدون مراحل إسناد أو تنفيذ
        return "جديد";
    }

    private ProjectResponse MapToProjectResponse(Project project)
    {
        return new ProjectResponse
        {
            Id = project.Id,
            ProjectName = project.ProjectName,
            ProjectNumber = project.ProjectNumber,
            FileNumber = project.FileNumber,
            Status = project.Status,
            CreatedBy = project.CreatedBy,
            IsActive = project.IsActive,
            IsFinancialSolvency = project.IsFinancialSolvency,
            TotalAmount = project.TotalAmount,
            ExecutionAmount = project.ExecutionAmount,
            ProfitPercentage = project.ProfitPercentage,
            CreatedAt = project.CreatedAt,
            Country = project.Country,
            CommissionPercentage = project.CommissionPercentage,
            CheckNumber = project.CheckNumber,
            CheckReceiptDate = project.CheckReceiptDate,
            CheckAmount = project.CheckAmount,
            ClearingDetails = project.ClearingDetails,
            CompanyDetailsId = project.CompanyDetailsId,
            CompanyName = project.CompanyDetails?.ArabicName,
            ShareToken = project.ShareToken,
            DepartmentName = project.DepartmentName,
            SupervisingEntityName = project.SupervisingEntityName,
            ExecutingCompanyName = project.ExecutingCompanyName,
            QuoteNumber = project.QuoteNumber,
            QuoteDate = project.QuoteDate,
            AssignmentNumber = project.AssignmentNumber,
            AssignmentApprovalDate = project.AssignmentApprovalDate,
            ExecutionDuration = project.ExecutionDuration,
            ExecutionDurationUnit = project.ExecutionDurationUnit,
            Milestones = project.Milestones?.Where(m => m.IsActive).Select(m => new ProjectExecutionMilestoneResponse
            {
                Id = m.Id,
                ProjectId = m.ProjectId,
                MilestoneName = m.MilestoneName,
                Duration = m.Duration,
                DurationUnit = m.DurationUnit,
                SequenceOrder = m.SequenceOrder,
                IsCompleted = m.IsCompleted,
                CompletionDate = m.CompletionDate,
                TargetStartDate = m.TargetStartDate,
                TargetEndDate = m.TargetEndDate
            }).ToList() ?? new List<ProjectExecutionMilestoneResponse>()
        };
    }

    private ProjectDetailsResponse MapToProjectDetailsResponse(Project project)
    {
        return new ProjectDetailsResponse
        {
            Id = project.Id,
            ProjectName = project.ProjectName,
            ProjectNumber = project.ProjectNumber,
            FileNumber = project.FileNumber,
            Status = project.Status,
            CreatedBy = project.CreatedBy,
            IsActive = project.IsActive,
            IsFinancialSolvency = project.IsFinancialSolvency,
            TotalAmount = project.TotalAmount,
            ExecutionAmount = project.ExecutionAmount,
            ProfitPercentage = project.ProfitPercentage,
            CreatedAt = project.CreatedAt,
            Country = project.Country,
            CommissionPercentage = project.CommissionPercentage,
            CheckNumber = project.CheckNumber,
            CheckReceiptDate = project.CheckReceiptDate,
            CheckAmount = project.CheckAmount,
            ClearingDetails = project.ClearingDetails,
            CompanyDetailsId = project.CompanyDetailsId,
            CompanyName = project.CompanyDetails?.ArabicName,
            ShareToken = project.ShareToken,
            DepartmentName = project.DepartmentName,
            SupervisingEntityName = project.SupervisingEntityName,
            ExecutingCompanyName = project.ExecutingCompanyName,
            QuoteNumber = project.QuoteNumber,
            QuoteDate = project.QuoteDate,
            AssignmentNumber = project.AssignmentNumber,
            AssignmentApprovalDate = project.AssignmentApprovalDate,
            ExecutionDuration = project.ExecutionDuration,
            ExecutionDurationUnit = project.ExecutionDurationUnit,
            AssignmentPhase = project.AssignmentPhase != null ? MapToAssignmentPhaseResponse(project.AssignmentPhase) : null,
            ExecutionPhase = project.ExecutionPhase != null ? MapToExecutionPhaseResponse(project.ExecutionPhase) : null,
            Documents = project.Documents?.Select(MapToDocumentResponse).ToList() ?? new List<ProjectDocumentResponse>(),
            Payments = project.Payments?.Select(MapToPaymentResponse).ToList() ?? new List<ProjectPaymentResponse>(),
            Assessments = project.Assessments?.Select(MapToAssessmentResponse).ToList() ?? new List<ProjectAssessmentResponse>(),
            PriceQuotes = project.PriceQuotes?.Select(MapToPriceQuoteResponse).ToList() ?? new List<ProjectPriceQuoteResponse>(),
            History = project.History?.Select(MapToHistoryResponse).ToList() ?? new List<ProjectHistoryResponse>(),
            Milestones = project.Milestones?.Where(m => m.IsActive).Select(m => new ProjectExecutionMilestoneResponse
            {
                Id = m.Id,
                ProjectId = m.ProjectId,
                MilestoneName = m.MilestoneName,
                Duration = m.Duration,
                DurationUnit = m.DurationUnit,
                SequenceOrder = m.SequenceOrder,
                IsCompleted = m.IsCompleted,
                CompletionDate = m.CompletionDate,
                TargetStartDate = m.TargetStartDate,
                TargetEndDate = m.TargetEndDate
            }).ToList() ?? new List<ProjectExecutionMilestoneResponse>()
        };
    }

    private ProjectAssignmentPhaseResponse MapToAssignmentPhaseResponse(ProjectAssignmentPhase phase)
    {
        return new ProjectAssignmentPhaseResponse
        {
            Id = phase.Id,
            ProjectId = phase.ProjectId,
            ExecutingEntityId = phase.ExecutingEntityId,
            ExecutingEntityName = phase.ExecutingEntityName ?? phase.ExecutingEntity?.CompanyName,
            SupervisingEntityId = phase.SupervisingEntityId,
            SupervisingEntityName = phase.SupervisingEntityName ?? phase.SupervisingEntity?.CompanyName,
            DepartmentId = phase.DepartmentId,
            DepartmentName = phase.DepartmentName ?? phase.Department?.DepartmentName,
            AssignmentDate = phase.AssignmentDate,
            AssignmentApprovalDate = phase.AssignmentApprovalDate,
            ExecutionStartDate = phase.ExecutionStartDate,
            ExecutionDuration = phase.ExecutionDuration,
            ExecutionDurationUnit = phase.ExecutionDurationUnit,
            Notes = phase.Notes,
            IsActive = phase.IsActive,
            CreatedAt = phase.CreatedAt
        };
    }

    private ProjectExecutionPhaseResponse MapToExecutionPhaseResponse(ProjectExecutionPhase phase)
    {
        return new ProjectExecutionPhaseResponse
        {
            Id = phase.Id,
            ProjectId = phase.ProjectId,
            GrantAmount = phase.GrantAmount,
            ExecutionAmount = phase.ExecutionAmount,
            RemainingAmount = phase.RemainingAmount,
            CompletionPercentage = phase.CompletionPercentage,
            TransferIssued = phase.TransferIssued,
            TransferDate = phase.TransferDate,
            ContractDataSent = phase.ContractDataSent,
            ContractSentDate = phase.ContractSentDate,
            FinalReportSent = phase.FinalReportSent,
            FinalReportDate = phase.FinalReportDate,
            CompanyId = phase.CompanyId,
            CompanyName = phase.CompanyName ?? phase.Company?.CompanyName,
            Notes = phase.Notes,
            IsActive = phase.IsActive,
            CreatedAt = phase.CreatedAt
        };
    }

    private ProjectDocumentResponse MapToDocumentResponse(ProjectDocument doc)
    {
        string? uploadedByResolved = doc.UploadedBy;
        if (!string.IsNullOrWhiteSpace(uploadedByResolved) && Guid.TryParse(uploadedByResolved, out var userGuid))
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == userGuid);
            if (user != null)
            {
                uploadedByResolved = (!string.IsNullOrEmpty(user.FirstName) || !string.IsNullOrEmpty(user.LastName))
                    ? $"{user.FirstName} {user.LastName}".Trim()
                    : (user.UserName ?? uploadedByResolved);
            }
        }

        return new ProjectDocumentResponse
        {
            Id = doc.Id,
            ProjectId = doc.ProjectId,
            DocumentType = doc.DocumentType,
            DocumentName = doc.DocumentName,
            FilePath = doc.FilePath,
            FileSize = doc.FileSize,
            MimeType = doc.MimeType,
            UploadedBy = uploadedByResolved,
            UploadedAt = doc.UploadedAt,
            MetaData = doc.MetaData,
            IsActive = doc.IsActive
        };
    }

    private ProjectPaymentResponse MapToPaymentResponse(ProjectPayment payment)
    {
        return new ProjectPaymentResponse
        {
            Id = payment.Id,
            ProjectId = payment.ProjectId,
            PaymentDate = payment.PaymentDate,
            PaymentAmount = payment.PaymentAmount,
            PaymentMethod = payment.PaymentMethod,
            PaymentReference = payment.PaymentReference,
            Notes = payment.Notes,
            IsActive = payment.IsActive,
            CreatedAt = payment.CreatedAt
        };
    }

    private ProjectAssessmentResponse MapToAssessmentResponse(ProjectAssessment assessment)
    {
        return new ProjectAssessmentResponse
        {
            Id = assessment.Id,
            ProjectId = assessment.ProjectId,
            AssessmentDate = assessment.AssessmentDate,
            AssessedBy = assessment.AssessedBy,
            AssessmentAmount = assessment.AssessmentAmount,
            AssessmentNotes = assessment.AssessmentNotes,
            DocumentId = assessment.DocumentId,
            DocumentName = assessment.Document?.DocumentName,
            IsActive = assessment.IsActive,
            CreatedAt = assessment.CreatedAt
        };
    }

    private ProjectPriceQuoteResponse MapToPriceQuoteResponse(ProjectPriceQuote quote)
    {
        return new ProjectPriceQuoteResponse
        {
            Id = quote.Id,
            ProjectId = quote.ProjectId,
            QuoteDate = quote.QuoteDate,
            QuoteNumber = quote.QuoteNumber,
            QuoteAmount = quote.QuoteAmount,
            ValidUntil = quote.ValidUntil,
            CompanyId = quote.CompanyId,
            CompanyName = quote.Company?.CompanyName,
            DocumentId = quote.DocumentId,
            DocumentName = quote.Document?.DocumentName,
            FilePath = quote.Document?.FilePath,
            IsAccepted = quote.IsAccepted,
            IsActive = quote.IsActive,
            CreatedAt = quote.CreatedAt,
            SaleInvoiceId = quote.SaleInvoiceId,
            CustomerName = quote.SaleInvoice?.CustomerProfile != null
                ? (quote.SaleInvoice.CustomerProfile.ArabicName ?? quote.SaleInvoice.CustomerProfile.EnglishName)
                : null,
            DepartmentName = quote.SaleInvoice?.CustomerProfile != null && quote.SaleInvoice.CustomerDepartmentId.HasValue
                ? quote.SaleInvoice.CustomerProfile.Departments?.FirstOrDefault(d => d.Id == quote.SaleInvoice.CustomerDepartmentId.Value)?.NameAr
                : null
        };
    }

    private ProjectHistoryResponse MapToHistoryResponse(ProjectHistory history)
    {
        return new ProjectHistoryResponse
        {
            Id = history.Id,
            ProjectId = history.ProjectId,
            ActionType = history.ActionType,
            ActionBy = history.ActionBy,
            ActionDate = history.ActionDate,
            OldValue = history.OldValue,
            NewValue = history.NewValue,
            Notes = history.Notes
        };
    }

    private async Task<string> GetUserNameAsync(Guid userId)
    {
        if (userId == Guid.Empty) return "مستخدم النظام";
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            if (!string.IsNullOrEmpty(user.FirstName) || !string.IsNullOrEmpty(user.LastName))
                return $"{user.FirstName} {user.LastName}".Trim();
            return user.UserName ?? "مستخدم النظام";
        }
        return "مستخدم النظام";
    }

    private async Task<string?> ResolveCreatedByNameAsync(Project project)
    {
        if (!string.IsNullOrWhiteSpace(project.CreatedBy) && project.CreatedBy.Trim() != "-")
        {
            return await ResolveNameOrGuidAsync(project.CreatedBy);
        }

        // Search in loaded history for explicit creation or earliest record
        if (project.History != null && project.History.Count > 0)
        {
            var createHistory = project.History
                .FirstOrDefault(h => !string.IsNullOrWhiteSpace(h.ActionBy) && 
                    (h.ActionType == "إنشاء" || h.ActionType == "انشاء" || h.ActionType == "Create" || h.ActionType == "إضافة" || (h.Notes != null && h.Notes.Contains("إنشاء"))));

            if (createHistory != null && !string.IsNullOrWhiteSpace(createHistory.ActionBy))
            {
                return await ResolveNameOrGuidAsync(createHistory.ActionBy);
            }

            var earliestHistory = project.History
                .Where(h => !string.IsNullOrWhiteSpace(h.ActionBy))
                .OrderBy(h => h.ActionDate)
                .FirstOrDefault();

            if (earliestHistory != null && !string.IsNullOrWhiteSpace(earliestHistory.ActionBy))
            {
                return await ResolveNameOrGuidAsync(earliestHistory.ActionBy);
            }
        }

        // Search DB histories table as fallback
        var dbCreateHistory = await _context.ProjectHistories
            .AsNoTracking()
            .Where(h => h.ProjectId == project.Id && !string.IsNullOrWhiteSpace(h.ActionBy))
            .OrderBy(h => h.ActionDate)
            .FirstOrDefaultAsync();

        if (dbCreateHistory != null && !string.IsNullOrWhiteSpace(dbCreateHistory.ActionBy))
        {
            return await ResolveNameOrGuidAsync(dbCreateHistory.ActionBy);
        }

        // Fallback to SupervisingEntityName if populated
        if (!string.IsNullOrWhiteSpace(project.SupervisingEntityName))
        {
            return project.SupervisingEntityName;
        }

        return project.CreatedBy;
    }

    private async Task<string?> ResolveNameOrGuidAsync(string rawNameOrGuid)
    {
        if (Guid.TryParse(rawNameOrGuid, out var userGuid))
        {
            var name = await GetUserNameAsync(userGuid);
            if (!string.IsNullOrWhiteSpace(name) && name != "مستخدم النظام")
                return name;
        }
        return rawNameOrGuid;
    }

    // ========== Assignment Phase Methods ==========

    public async Task<object> AddAssignmentPhaseAsync(CreateProjectAssignmentPhaseRequest request, Guid currentUserId)
    {
        var project = await _context.Projects.FindAsync(request.ProjectId);
        if (project == null)
            throw new Exception("المشروع غير موجود");

        var phase = new ProjectAssignmentPhase
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            ExecutingEntityName = request.ExecutingEntityName,
            SupervisingEntityName = request.SupervisingEntityName,
            DepartmentName = request.DepartmentName,
            AssignmentDate = request.AssignmentDate,
            AssignmentApprovalDate = request.AssignmentApprovalDate,
            ExecutionStartDate = request.ExecutionStartDate,
            ExecutionDuration = request.ExecutionDuration,
            ExecutionDurationUnit = request.ExecutionDurationUnit,
            Notes = request.Notes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.ProjectAssignmentPhases.Add(phase);

        // Sync to main Project entity basic data
        project.ExecutingCompanyName = request.ExecutingEntityName;
        project.SupervisingEntityName = request.SupervisingEntityName;
        project.DepartmentName = request.DepartmentName;
        project.AssignmentApprovalDate = request.AssignmentApprovalDate;
        if (request.ExecutionDuration.HasValue)
        {
            project.ExecutionDuration = request.ExecutionDuration.Value;
        }
        if (!string.IsNullOrWhiteSpace(request.ExecutionDurationUnit))
        {
            project.ExecutionDurationUnit = request.ExecutionDurationUnit;
        }

        // تحديث حالة المشروع تلقائياً إلى "قيد الإسناد" إذا لم يكن قيد التنفيذ أو مكتمل/منفذ
        if (project.Status != "قيد التنفيذ" && project.Status != "منفذ" && project.Status != "مكتمل")
        {
            var oldStatus = project.Status;
            project.Status = "قيد الإسناد";
            _context.ProjectHistories.Add(new ProjectHistory
            {
                Id = Guid.NewGuid(),
                ProjectId = project.Id,
                ActionType = "تغيير حالة تلقائي",
                ActionBy = await GetUserNameAsync(currentUserId),
                ActionDate = DateTime.UtcNow,
                OldValue = $"الحالة السابقة: {oldStatus}",
                NewValue = "الحالة الجديدة: قيد الإسناد",
                Notes = "تغيير الحالة تلقائياً عند إضافة مرحلة الإسناد"
            });
        }

        await RecalculateMilestoneDatesAsync(request.ProjectId, request.ExecutionStartDate);

        // Add history
        var history = new ProjectHistory
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            ActionType = "إضافة مرحلة إسناد",
            ActionBy = await GetUserNameAsync(currentUserId),
            ActionDate = DateTime.UtcNow,
            NewValue = $"تم إضافة مرحلة الإسناد",
            Notes = "إضافة مرحلة إسناد جديدة"
        };
        _context.ProjectHistories.Add(history);

        await _context.SaveChangesAsync();
        return phase;
    }

    public async Task<object> UpdateAssignmentPhaseAsync(CreateProjectAssignmentPhaseRequest request, Guid currentUserId)
    {
        var phase = await _context.ProjectAssignmentPhases
            .FirstOrDefaultAsync(p => p.ProjectId == request.ProjectId);
        
        if (phase == null)
            throw new Exception("مرحلة الإسناد غير موجودة");

        phase.ExecutingEntityName = request.ExecutingEntityName;
        phase.SupervisingEntityName = request.SupervisingEntityName;
        phase.DepartmentName = request.DepartmentName;
        phase.AssignmentDate = request.AssignmentDate;
        phase.AssignmentApprovalDate = request.AssignmentApprovalDate;
        phase.ExecutionStartDate = request.ExecutionStartDate;
        phase.ExecutionDuration = request.ExecutionDuration;
        phase.ExecutionDurationUnit = request.ExecutionDurationUnit;
        phase.Notes = request.Notes;

        // Sync to main Project entity basic data
        var project = await _context.Projects.FindAsync(request.ProjectId);
        if (project != null)
        {
            project.ExecutingCompanyName = request.ExecutingEntityName;
            project.SupervisingEntityName = request.SupervisingEntityName;
            project.DepartmentName = request.DepartmentName;
            project.AssignmentApprovalDate = request.AssignmentApprovalDate;
            if (request.ExecutionDuration.HasValue)
            {
                project.ExecutionDuration = request.ExecutionDuration.Value;
            }
            if (!string.IsNullOrWhiteSpace(request.ExecutionDurationUnit))
            {
                project.ExecutionDurationUnit = request.ExecutionDurationUnit;

                var projectMilestones = await _context.ProjectExecutionMilestones
                    .Where(m => m.ProjectId == request.ProjectId && m.IsActive)
                    .ToListAsync();

                foreach (var m in projectMilestones)
                {
                    m.DurationUnit = request.ExecutionDurationUnit;
                }
            }

            // Sync to Execution Phase if exists
            var execPhase = await _context.ProjectExecutionPhases.FirstOrDefaultAsync(ep => ep.ProjectId == request.ProjectId && ep.IsActive);
            if (execPhase != null && !string.IsNullOrWhiteSpace(request.ExecutingEntityName))
            {
                execPhase.CompanyName = request.ExecutingEntityName;
            }
        }

        await RecalculateMilestoneDatesAsync(request.ProjectId, request.ExecutionStartDate);

        // Add history
        var history = new ProjectHistory
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            ActionType = "تحديث مرحلة إسناد",
            ActionBy = await GetUserNameAsync(currentUserId),
            ActionDate = DateTime.UtcNow,
            NewValue = "تم تحديث مرحلة الإسناد",
            Notes = "تحديث بيانات مرحلة الإسناد"
        };
        _context.ProjectHistories.Add(history);

        await _context.SaveChangesAsync();
        return phase;
    }

    // ========== Execution Phase Methods ==========

    public async Task<object> AddExecutionPhaseAsync(CreateProjectExecutionPhaseRequest request, Guid currentUserId)
    {
        var project = await _context.Projects.FindAsync(request.ProjectId);
        if (project == null)
            throw new Exception("المشروع غير موجود");

        var phase = new ProjectExecutionPhase
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            CompanyName = request.CompanyName,
            GrantAmount = request.GrantAmount,
            ExecutionAmount = request.ExecutionAmount,
            RemainingAmount = request.RemainingAmount,
            CompletionPercentage = request.CompletionPercentage,
            TransferIssued = request.TransferIssued ?? false,
            TransferDate = request.TransferDate,
            ContractDataSent = request.ContractDataSent ?? false,
            ContractSentDate = request.ContractSentDate,
            FinalReportSent = request.FinalReportSent ?? false,
            FinalReportDate = request.FinalReportDate,
            Notes = request.Notes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.ProjectExecutionPhases.Add(phase);

        // Sync to main Project entity basic data
        project.ExecutingCompanyName = request.CompanyName;
        if (request.GrantAmount.HasValue)
        {
            project.TotalAmount = request.GrantAmount.Value;
        }
        if (request.ExecutionAmount.HasValue)
        {
            project.ExecutionAmount = request.ExecutionAmount.Value;
        }

        // تحديث حالة المشروع تلقائياً
        var oldStatus = project.Status;
        if (request.CompletionPercentage >= 100)
        {
            if (project.Status != "منفذ" && project.Status != "مكتمل")
            {
                project.Status = "منفذ";
                _context.ProjectHistories.Add(new ProjectHistory
                {
                    Id = Guid.NewGuid(),
                    ProjectId = project.Id,
                    ActionType = "تغيير حالة تلقائي",
                    ActionBy = await GetUserNameAsync(currentUserId),
                    ActionDate = DateTime.UtcNow,
                    OldValue = $"الحالة السابقة: {oldStatus}",
                    NewValue = "الحالة الجديدة: منفذ",
                    Notes = "تغيير الحالة تلقائياً لاكتمال مرحلة التنفيذ (100%)"
                });
            }
        }
        else
        {
            if (project.Status != "قيد التنفيذ")
            {
                project.Status = "قيد التنفيذ";
                _context.ProjectHistories.Add(new ProjectHistory
                {
                    Id = Guid.NewGuid(),
                    ProjectId = project.Id,
                    ActionType = "تغيير حالة تلقائي",
                    ActionBy = await GetUserNameAsync(currentUserId),
                    ActionDate = DateTime.UtcNow,
                    OldValue = $"الحالة السابقة: {oldStatus}",
                    NewValue = "الحالة الجديدة: قيد التنفيذ",
                    Notes = "تغيير الحالة تلقائياً عند إضافة مرحلة التنفيذ"
                });
            }
        }

        // Add history
        var history = new ProjectHistory
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            ActionType = "إضافة مرحلة تنفيذ",
            ActionBy = await GetUserNameAsync(currentUserId),
            ActionDate = DateTime.UtcNow,
            NewValue = $"تم إضافة مرحلة التنفيذ - نسبة الإنجاز: {request.CompletionPercentage}%",
            Notes = "إضافة مرحلة تنفيذ جديدة"
        };
        _context.ProjectHistories.Add(history);

        await _context.SaveChangesAsync();
        return phase;
    }

    public async Task<object> UpdateExecutionPhaseAsync(CreateProjectExecutionPhaseRequest request, Guid currentUserId)
    {
        var phase = await _context.ProjectExecutionPhases
            .FirstOrDefaultAsync(p => p.ProjectId == request.ProjectId);
        
        if (phase == null)
            throw new Exception("مرحلة التنفيذ غير موجودة");

        phase.CompanyName = request.CompanyName;
        phase.GrantAmount = request.GrantAmount;
        phase.ExecutionAmount = request.ExecutionAmount;
        phase.RemainingAmount = request.RemainingAmount;
        phase.CompletionPercentage = request.CompletionPercentage;
        phase.TransferIssued = request.TransferIssued ?? false;
        phase.TransferDate = request.TransferDate;
        phase.ContractDataSent = request.ContractDataSent ?? false;
        phase.ContractSentDate = request.ContractSentDate;
        phase.FinalReportSent = request.FinalReportSent ?? false;
        phase.FinalReportDate = request.FinalReportDate;
        phase.Notes = request.Notes;

        // Sync to main Project entity basic data
        var project = await _context.Projects.FindAsync(request.ProjectId);
        if (project != null)
        {
            if (!string.IsNullOrWhiteSpace(request.CompanyName))
            {
                project.ExecutingCompanyName = request.CompanyName;
            }
            if (request.GrantAmount.HasValue)
            {
                project.TotalAmount = request.GrantAmount.Value;
            }
            if (request.ExecutionAmount.HasValue)
            {
                project.ExecutionAmount = request.ExecutionAmount.Value;
            }

            // Sync to Assignment Phase if exists
            var assignPhase = await _context.ProjectAssignmentPhases.FirstOrDefaultAsync(ap => ap.ProjectId == request.ProjectId && ap.IsActive);
            if (assignPhase != null && !string.IsNullOrWhiteSpace(request.CompanyName))
            {
                assignPhase.ExecutingEntityName = request.CompanyName;
            }
        }

        // Add history
        var history = new ProjectHistory
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            ActionType = "تحديث مرحلة تنفيذ",
            ActionBy = await GetUserNameAsync(currentUserId),
            ActionDate = DateTime.UtcNow,
            NewValue = $"تم تحديث مرحلة التنفيذ - نسبة الإنجاز: {request.CompletionPercentage}%",
            Notes = "تحديث بيانات مرحلة التنفيذ"
        };
        _context.ProjectHistories.Add(history);

        await _context.SaveChangesAsync();
        return phase;
    }

    // ========== Document Methods ==========

    public async Task<object> AddDocumentAsync(CreateProjectDocumentRequest request, Guid currentUserId)
    {
        var project = await _context.Projects.FindAsync(request.ProjectId);
        if (project == null)
            throw new Exception("المشروع غير موجود");

        var userName = await GetUserNameAsync(currentUserId);
        if (string.IsNullOrWhiteSpace(userName) || userName == "مستخدم النظام")
        {
            userName = currentUserId != Guid.Empty ? currentUserId.ToString() : string.Empty;
        }

        var document = new ProjectDocument
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            DocumentType = request.DocumentType,
            DocumentName = request.DocumentName,
            FilePath = request.FilePath,
            FileSize = request.FileSize,
            MimeType = request.MimeType,
            MetaData = request.MetaData,
            UploadedBy = userName,
            UploadedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.ProjectDocuments.Add(document);

        // Add history
        var history = new ProjectHistory
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            ActionType = "إضافة مستند",
            ActionBy = userName,
            ActionDate = DateTime.UtcNow,
            NewValue = $"تم إضافة مستند: {request.DocumentName} ({request.DocumentType})",
            Notes = "إضافة مستند جديد"
        };
        _context.ProjectHistories.Add(history);

        await _context.SaveChangesAsync();
        return MapToDocumentResponse(document);
    }

    public async Task<bool> DeleteDocumentAsync(Guid documentId, Guid currentUserId)
    {
        var document = await _context.ProjectDocuments.FindAsync(documentId);
        if (document == null)
            return false;

        _context.ProjectDocuments.Remove(document);

        // Add history
        var history = new ProjectHistory
        {
            Id = Guid.NewGuid(),
            ProjectId = document.ProjectId,
            ActionType = "حذف مستند",
            ActionBy = await GetUserNameAsync(currentUserId),
            ActionDate = DateTime.UtcNow,
            NewValue = $"تم حذف مستند: {document.DocumentName}",
            Notes = "حذف مستند"
        };
        _context.ProjectHistories.Add(history);

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateDocumentNameAsync(Guid documentId, string newDocumentName, Guid currentUserId)
    {
        var document = await _context.ProjectDocuments.FindAsync(documentId);
        if (document == null)
            return false;

        var oldName = document.DocumentName;
        document.DocumentName = newDocumentName;

        var history = new ProjectHistory
        {
            Id = Guid.NewGuid(),
            ProjectId = document.ProjectId,
            ActionType = "تعديل اسم المستند",
            ActionBy = await GetUserNameAsync(currentUserId),
            ActionDate = DateTime.UtcNow,
            OldValue = oldName,
            NewValue = newDocumentName,
            Notes = "تحديث اسم المستند"
        };
        _context.ProjectHistories.Add(history);

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<object> UploadDocumentAsync(Guid projectId, string documentType, string documentName, string fileName, byte[] fileBytes, string contentType, Guid currentUserId)
    {
        var project = await _context.Projects.FindAsync(projectId);
        if (project == null)
            throw new Exception("المشروع غير موجود");

        var uploadsFolderPath = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        if (!System.IO.Directory.Exists(uploadsFolderPath))
        {
            System.IO.Directory.CreateDirectory(uploadsFolderPath);
        }

        var uniqueFileName = $"{Guid.NewGuid()}_{System.IO.Path.GetFileName(fileName)}";
        var filePath = System.IO.Path.Combine(uploadsFolderPath, uniqueFileName);

        await System.IO.File.WriteAllBytesAsync(filePath, fileBytes);

        var webPath = $"/uploads/{uniqueFileName}";

        var document = new ProjectDocument
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            DocumentType = documentType,
            DocumentName = documentName,
            FilePath = webPath,
            FileSize = fileBytes.Length,
            MimeType = contentType,
            UploadedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.ProjectDocuments.Add(document);

        // Add history
        var history = new ProjectHistory
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            ActionType = "إضافة مستند",
            ActionBy = await GetUserNameAsync(currentUserId),
            ActionDate = DateTime.UtcNow,
            NewValue = $"تم إضافة مستند: {documentName} ({documentType})",
            Notes = "تم رفع مستند جديد وتخزينه"
        };
        _context.ProjectHistories.Add(history);

        await _context.SaveChangesAsync();
        return document;
    }

    // ========== Payment Methods ==========

    public async Task<object> AddPaymentAsync(CreateProjectPaymentRequest request, Guid currentUserId)
    {
        var project = await _context.Projects.FindAsync(request.ProjectId);
        if (project == null)
            throw new Exception("المشروع غير موجود");

        // Validate that total payments don't exceed the project budget
        var existingPaymentsSum = await _context.ProjectPayments
            .Where(p => p.ProjectId == request.ProjectId && p.IsActive)
            .SumAsync(p => p.PaymentAmount);

        if (project.TotalAmount.HasValue && project.TotalAmount.Value > 0 && (existingPaymentsSum + request.PaymentAmount) > project.TotalAmount.Value)
        {
            throw new Exception($"لا يمكن إضافة دفعة تتجاوز ميزانية المشروع المتبقية. الميزانية المتبقية: {(project.TotalAmount.Value - existingPaymentsSum):N2} ريال");
        }

        var payment = new ProjectPayment
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            PaymentDate = request.PaymentDate,
            PaymentAmount = request.PaymentAmount,
            PaymentMethod = request.PaymentMethod,
            PaymentReference = request.PaymentReference,
            Notes = request.Notes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.ProjectPayments.Add(payment);

        // Add history
        var history = new ProjectHistory
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            ActionType = "إضافة دفعة",
            ActionBy = await GetUserNameAsync(currentUserId),
            ActionDate = DateTime.UtcNow,
            NewValue = $"تم إضافة دفعة بمبلغ: {request.PaymentAmount:N2} ريال",
            Notes = "إضافة دفعة جديدة"
        };
        _context.ProjectHistories.Add(history);

        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task<object> UpdatePaymentAsync(UpdateProjectPaymentRequest request, Guid currentUserId)
    {
        var payment = await _context.ProjectPayments.FindAsync(request.Id);
        if (payment == null)
            throw new Exception("الدفعة غير موجودة");

        var project = await _context.Projects.FindAsync(request.ProjectId);
        if (project == null)
            throw new Exception("المشروع غير موجود");

        // Validate that total payments don't exceed the project budget (excluding this payment's old amount)
        var existingPaymentsSum = await _context.ProjectPayments
            .Where(p => p.ProjectId == request.ProjectId && p.Id != request.Id && p.IsActive)
            .SumAsync(p => p.PaymentAmount);

        if (project.TotalAmount.HasValue && project.TotalAmount.Value > 0 && (existingPaymentsSum + request.PaymentAmount) > project.TotalAmount.Value)
        {
            throw new Exception($"لا يمكن تعديل الدفعة لتتجاوز ميزانية المشروع المتبقية. الميزانية المتبقية: {(project.TotalAmount.Value - existingPaymentsSum):N2} ريال");
        }

        var oldValue = $"المبلغ: {payment.PaymentAmount:N2}, المرجع: {payment.PaymentReference}, الطريقة: {payment.PaymentMethod}";

        payment.PaymentDate = request.PaymentDate;
        payment.PaymentAmount = request.PaymentAmount;
        payment.PaymentMethod = request.PaymentMethod;
        payment.PaymentReference = request.PaymentReference;
        payment.Notes = request.Notes;

        var history = new ProjectHistory
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            ActionType = "تعديل دفعة",
            ActionBy = await GetUserNameAsync(currentUserId),
            ActionDate = DateTime.UtcNow,
            OldValue = oldValue,
            NewValue = $"المبلغ الجديد: {request.PaymentAmount:N2} ريال، المرجع الجديد: {request.PaymentReference}",
            Notes = "تعديل بيانات الدفعة"
        };
        _context.ProjectHistories.Add(history);

        await _context.SaveChangesAsync();
        return payment;
    }

    public async Task<bool> DeletePaymentAsync(Guid paymentId, Guid currentUserId)
    {
        var payment = await _context.ProjectPayments.FindAsync(paymentId);
        if (payment == null)
            return false;

        _context.ProjectPayments.Remove(payment);

        // Add history
        var history = new ProjectHistory
        {
            Id = Guid.NewGuid(),
            ProjectId = payment.ProjectId,
            ActionType = "حذف دفعة",
            ActionBy = await GetUserNameAsync(currentUserId),
            ActionDate = DateTime.UtcNow,
            NewValue = $"تم حذف دفعة بمبلغ: {payment.PaymentAmount:N2} ريال",
            Notes = "حذف دفعة"
        };
        _context.ProjectHistories.Add(history);

        await _context.SaveChangesAsync();
        return true;
    }

    // ========== Price Quote Methods ==========

    public async Task<object> AddPriceQuoteAsync(CreateProjectPriceQuoteRequest request, Guid currentUserId)
    {
        var project = await _context.Projects.FindAsync(request.ProjectId);
        if (project == null)
            throw new Exception("المشروع غير موجود");

        Guid? finalCompanyId = null;
        if (request.CompanyId.HasValue && request.CompanyId.Value != Guid.Empty)
        {
            var companyDetails = await _context.Set<CompanyDetails>().FindAsync(request.CompanyId.Value);
            if (companyDetails != null)
            {
                var companyName = companyDetails.ArabicName ?? companyDetails.EnglishName ?? "شركة غير معروفة";
                var projectCompany = await _context.ProjectCompanies
                    .FirstOrDefaultAsync(c => c.CompanyName == companyName && !c.IsDeleted);

                if (projectCompany == null)
                {
                    projectCompany = new Company
                    {
                        Id = Guid.NewGuid(),
                        CompanyName = companyName,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = currentUserId == Guid.Empty ? Guid.NewGuid() : currentUserId
                    };
                    _context.ProjectCompanies.Add(projectCompany);
                    await _context.SaveChangesAsync();
                }
                finalCompanyId = projectCompany.Id;
            }
        }

        // Check if quote already exists for this project/invoice and update it
        ProjectPriceQuote? existingQuote = null;
        if (request.SaleInvoiceId.HasValue && request.SaleInvoiceId.Value > 0)
        {
            existingQuote = await _context.ProjectPriceQuotes
                .FirstOrDefaultAsync(q => q.SaleInvoiceId == request.SaleInvoiceId.Value);
        }
        if (existingQuote == null && !string.IsNullOrWhiteSpace(request.QuoteNumber))
        {
            existingQuote = await _context.ProjectPriceQuotes
                .FirstOrDefaultAsync(q => q.ProjectId == request.ProjectId && q.QuoteNumber == request.QuoteNumber);
        }

        if (existingQuote != null)
        {
            existingQuote.QuoteDate = request.QuoteDate;
            if (!string.IsNullOrWhiteSpace(request.QuoteNumber)) existingQuote.QuoteNumber = request.QuoteNumber;
            if (request.QuoteAmount > 0) existingQuote.QuoteAmount = request.QuoteAmount;
            if (request.ValidUntil.HasValue) existingQuote.ValidUntil = request.ValidUntil;
            if (finalCompanyId.HasValue) existingQuote.CompanyId = finalCompanyId;
            if (request.DocumentId.HasValue && request.DocumentId.Value != Guid.Empty)
            {
                existingQuote.DocumentId = request.DocumentId.Value;
            }

            var updateHistory = new ProjectHistory
            {
                Id = Guid.NewGuid(),
                ProjectId = existingQuote.ProjectId,
                ActionType = "تعديل عرض سعر",
                ActionBy = await GetUserNameAsync(currentUserId),
                ActionDate = DateTime.UtcNow,
                NewValue = $"تم تحديث عرض سعر رقم: {existingQuote.QuoteNumber} والمرفق الخاص به",
                Notes = "تحديث عرض السعر والمرفق"
            };
            _context.ProjectHistories.Add(updateHistory);

            await _context.SaveChangesAsync();
            return existingQuote;
        }

        var priceQuote = new ProjectPriceQuote
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            QuoteDate = request.QuoteDate,
            QuoteNumber = request.QuoteNumber,
            QuoteAmount = request.QuoteAmount,
            ValidUntil = request.ValidUntil,
            CompanyId = finalCompanyId,
            DocumentId = request.DocumentId,
            SaleInvoiceId = request.SaleInvoiceId,
            IsAccepted = false,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUserId == Guid.Empty ? Guid.NewGuid() : currentUserId
        };

        _context.ProjectPriceQuotes.Add(priceQuote);

        // Add history
        var history = new ProjectHistory
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            ActionType = "إضافة عرض سعر",
            ActionBy = await GetUserNameAsync(currentUserId),
            ActionDate = DateTime.UtcNow,
            NewValue = $"تم إضافة عرض سعر رقم: {request.QuoteNumber} بمبلغ: {request.QuoteAmount:N2} ريال",
            Notes = "إضافة عرض سعر جديد للمشروع"
        };
        _context.ProjectHistories.Add(history);

        await _context.SaveChangesAsync();
        return priceQuote;
    }

    public async Task<object> UpdatePriceQuoteAsync(UpdateProjectPriceQuoteRequest request, Guid currentUserId)
    {
        var priceQuote = await _context.ProjectPriceQuotes
            .FirstOrDefaultAsync(q => q.Id == request.Id || (request.SaleInvoiceId.HasValue && q.SaleInvoiceId == request.SaleInvoiceId.Value));
        if (priceQuote == null && !string.IsNullOrWhiteSpace(request.QuoteNumber))
        {
            priceQuote = await _context.ProjectPriceQuotes
                .FirstOrDefaultAsync(q => q.ProjectId == request.ProjectId && q.QuoteNumber == request.QuoteNumber);
        }

        if (priceQuote == null)
            throw new Exception("عرض السعر غير موجود");

        if (priceQuote.IsAccepted)
            throw new Exception("لا يمكن تعديل عرض سعر تم قبوله واعتماده بالفعل");

        Guid? finalCompanyId = null;
        if (request.CompanyId.HasValue && request.CompanyId.Value != Guid.Empty)
        {
            var companyDetails = await _context.Set<CompanyDetails>().FindAsync(request.CompanyId.Value);
            if (companyDetails != null)
            {
                var companyName = companyDetails.ArabicName ?? companyDetails.EnglishName ?? "شركة غير معروفة";
                var projectCompany = await _context.ProjectCompanies
                    .FirstOrDefaultAsync(c => c.CompanyName == companyName && !c.IsDeleted);

                if (projectCompany == null)
                {
                    projectCompany = new Company
                    {
                        Id = Guid.NewGuid(),
                        CompanyName = companyName,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = currentUserId == Guid.Empty ? Guid.NewGuid() : currentUserId
                    };
                    _context.ProjectCompanies.Add(projectCompany);
                    await _context.SaveChangesAsync();
                }
                finalCompanyId = projectCompany.Id;
            }
        }

        priceQuote.QuoteDate = request.QuoteDate;
        priceQuote.QuoteNumber = request.QuoteNumber;
        priceQuote.QuoteAmount = request.QuoteAmount;
        priceQuote.ValidUntil = request.ValidUntil;
        if (finalCompanyId.HasValue) priceQuote.CompanyId = finalCompanyId;
        if (request.DocumentId.HasValue && request.DocumentId.Value != Guid.Empty)
        {
            priceQuote.DocumentId = request.DocumentId.Value;
        }

        // Add history
        var history = new ProjectHistory
        {
            Id = Guid.NewGuid(),
            ProjectId = priceQuote.ProjectId,
            ActionType = "تعديل عرض سعر",
            ActionDate = DateTime.UtcNow,
            NewValue = $"تم تعديل عرض سعر رقم: {request.QuoteNumber} بمبلغ: {request.QuoteAmount:N2} ريال",
            ActionBy = await GetUserNameAsync(currentUserId),
            Notes = "تعديل بيانات عرض السعر"
        };
        _context.ProjectHistories.Add(history);

        await _context.SaveChangesAsync();
        return priceQuote;
    }

    public async Task<bool> DeletePriceQuoteAsync(Guid quoteId, Guid currentUserId)
    {
        var priceQuote = await _context.ProjectPriceQuotes
            .FirstOrDefaultAsync(q => q.Id == quoteId || q.DocumentId == quoteId);
        if (priceQuote == null)
            return false;

        var projectId = priceQuote.ProjectId;
        var quoteNumber = priceQuote.QuoteNumber;
        var quoteAmount = priceQuote.QuoteAmount;

        _context.ProjectPriceQuotes.Remove(priceQuote);

        // Also check if this quote's details are on the Project record itself and clear them
        var project = await _context.Projects.FindAsync(projectId);
        if (project != null)
        {
            if (project.QuoteNumber == quoteNumber)
            {
                project.QuoteNumber = null;
                project.QuoteDate = null;
            }
        }

        // Add history
        var history = new ProjectHistory
        {
            Id = Guid.NewGuid(),
            ProjectId = projectId,
            ActionType = "حذف عرض سعر",
            ActionDate = DateTime.UtcNow,
            NewValue = $"تم حذف عرض سعر رقم: {quoteNumber} بمبلغ: {quoteAmount:N2} ريال",
            ActionBy = await GetUserNameAsync(currentUserId),
            Notes = "حذف عرض سعر"
        };
        _context.ProjectHistories.Add(history);

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> AcceptPriceQuoteAsync(Guid quoteId, Guid currentUserId)
    {
        var priceQuote = await _context.ProjectPriceQuotes.FindAsync(quoteId);
        if (priceQuote == null)
            return false;

        // Reset other accepted quotes for this project to false
        var otherQuotes = await _context.ProjectPriceQuotes
            .Where(q => q.ProjectId == priceQuote.ProjectId && q.Id != quoteId)
            .ToListAsync();

        foreach (var quote in otherQuotes)
        {
            quote.IsAccepted = false;
        }

        priceQuote.IsAccepted = true;

        // Add history
        var history = new ProjectHistory
        {
            Id = Guid.NewGuid(),
            ProjectId = priceQuote.ProjectId,
            ActionType = "قبول عرض سعر",
            ActionDate = DateTime.UtcNow,
            NewValue = $"تم قبول عرض سعر رقم: {priceQuote.QuoteNumber} بمبلغ: {priceQuote.QuoteAmount:N2} ريال كعرض معتمد للمشروع",
            ActionBy = await GetUserNameAsync(currentUserId),
            Notes = "تغيير حالة عرض السعر إلى مقبول"
        };
        _context.ProjectHistories.Add(history);

        await _context.SaveChangesAsync();
        return true;
    }

    // ========== Project Assessment Methods ==========

    public async Task<object> AddAssessmentAsync(CreateProjectAssessmentRequest request, Guid currentUserId)
    {
        var project = await _context.Projects.FindAsync(request.ProjectId);
        if (project == null)
            throw new Exception("المشروع غير موجود");

        var assessment = new ProjectAssessment
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            AssessmentDate = request.AssessmentDate,
            AssessedBy = request.AssessedBy,
            AssessmentAmount = request.AssessmentAmount,
            AssessmentNotes = request.AssessmentNotes,
            DocumentId = request.DocumentId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUserId == Guid.Empty ? Guid.NewGuid() : currentUserId
        };

        _context.ProjectAssessments.Add(assessment);

        // Add history
        var history = new ProjectHistory
        {
            Id = Guid.NewGuid(),
            ProjectId = request.ProjectId,
            ActionType = "إضافة تقييم",
            ActionDate = DateTime.UtcNow,
            NewValue = $"تم إضافة تقييم بواسطة: {request.AssessedBy} بمبلغ: {request.AssessmentAmount:N2} ريال",
            ActionBy = await GetUserNameAsync(currentUserId),
            Notes = "إضافة تقييم جديد للمشروع"
        };
        _context.ProjectHistories.Add(history);

        await _context.SaveChangesAsync();
        return assessment;
    }

    public async Task<object> UpdateAssessmentAsync(UpdateProjectAssessmentRequest request, Guid currentUserId)
    {
        var assessment = await _context.ProjectAssessments.FindAsync(request.Id);
        if (assessment == null)
            throw new Exception("التقييم غير موجود");

        assessment.AssessmentDate = request.AssessmentDate;
        assessment.AssessedBy = request.AssessedBy;
        assessment.AssessmentAmount = request.AssessmentAmount;
        assessment.AssessmentNotes = request.AssessmentNotes;
        assessment.DocumentId = request.DocumentId;

        // Add history
        var history = new ProjectHistory
        {
            Id = Guid.NewGuid(),
            ProjectId = assessment.ProjectId,
            ActionType = "تعديل تقييم",
            ActionDate = DateTime.UtcNow,
            NewValue = $"تم تعديل تقييم بواسطة: {request.AssessedBy} بمبلغ: {request.AssessmentAmount:N2} ريال",
            ActionBy = await GetUserNameAsync(currentUserId),
            Notes = "تعديل بيانات التقييم"
        };
        _context.ProjectHistories.Add(history);

        await _context.SaveChangesAsync();
        return assessment;
    }

    public async Task<bool> DeleteAssessmentAsync(Guid assessmentId, Guid currentUserId)
    {
        var assessment = await _context.ProjectAssessments.FindAsync(assessmentId);
        if (assessment == null)
            return false;

        _context.ProjectAssessments.Remove(assessment);

        // Add history
        var history = new ProjectHistory
        {
            Id = Guid.NewGuid(),
            ProjectId = assessment.ProjectId,
            ActionType = "حذف تقييم",
            ActionDate = DateTime.UtcNow,
            NewValue = $"تم حذف التقييم الخاص بـ: {assessment.AssessedBy} بمبلغ: {assessment.AssessmentAmount:N2} ريال",
            ActionBy = await GetUserNameAsync(currentUserId),
            Notes = "حذف التقييم"
        };
        _context.ProjectHistories.Add(history);

        await _context.SaveChangesAsync();
        return true;
    }

    private async Task SyncProjectChecksAsync(Guid projectId, List<ProjectCheckDto> checkDtos, List<DeletedCheckDto> deletedChecks, Guid currentUserId)
    {
        // 1. Get existing connections
        var existingConnections = await _context.ReceiptConnections
            .Include(rc => rc.PaycheckReceipt)
                .ThenInclude(pr => pr.PaycheckReceiptAccounts)
            .Where(rc => rc.ProjectId == projectId && rc.PaycheckReceiptId != null && rc.IsActive == true)
            .ToListAsync();

        // 2. Deletions
        var incomingIds = checkDtos.Where(c => c.Id.HasValue).Select(c => c.Id!.Value).ToList();
        var deletedConnections = existingConnections.Where(c => !incomingIds.Contains(c.PaycheckReceiptId!.Value)).ToList();

        foreach (var conn in deletedConnections)
        {
            conn.IsActive = false;
            if (conn.PaycheckReceipt != null)
            {
                var reason = deletedChecks?.FirstOrDefault(dc => dc.Id == conn.PaycheckReceiptId)?.Reason ?? "تم حذف الدفعة من المشروع";

                conn.PaycheckReceipt.IsActive = false;
                
                // 1. Soft delete associated transaction instead of hard deleting it
                if (conn.PaycheckReceipt.TransactionId.HasValue)
                {
                    var transaction = await _context.Transactions.FindAsync(conn.PaycheckReceipt.TransactionId.Value);
                    if (transaction != null)
                    {
                        transaction.IsActive = false;
                        var entries = await _context.TransactionEntries.Where(te => te.TransactionId == transaction.Id).ToListAsync();
                        foreach (var entry in entries)
                        {
                            entry.IsActive = false;
                        }
                    }
                }

                // 2. Create reverse PaymentReceipt
                var reverseRef = "REV-" + conn.PaycheckReceipt.ReferenceNumber;
                var paymentReceipt = new PaymentReceipt
                {
                    Id = Guid.NewGuid(),
                    ReferenceNumber = reverseRef,
                    Description = $"عملية عكسية بسبب إلغاء دفعة المشروع {conn.PaycheckReceipt.ReferenceNumber}: {reason}",
                    Amount = conn.PaycheckReceipt.Amount,
                    Tax = conn.PaycheckReceipt.Tax,
                    TreasuryId = conn.PaycheckReceipt.TreasuryId,
                    ReceiptDate = DateOnly.FromDateTime(DateTime.UtcNow),
                    PaymentMethodType = conn.PaycheckReceipt.PaymentMethodType,
                    CheckNumber = conn.PaycheckReceipt.CheckNumber,
                    BankName = conn.PaycheckReceipt.BankName,
                    BankReferenceNumber = conn.PaycheckReceipt.BankReferenceNumber,
                    VoucherNumber = conn.PaycheckReceipt.VoucherNumber,
                    TransactionNumber = conn.PaycheckReceipt.TransactionNumber,
                    IsFrontal = conn.PaycheckReceipt.IsFrontal,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = currentUserId
                };

                var paymentAccounts = new List<PaymentReceiptAccount>();
                foreach (var payAccount in conn.PaycheckReceipt.PaycheckReceiptAccounts)
                {
                    paymentAccounts.Add(new PaymentReceiptAccount
                    {
                        Id = Guid.NewGuid(),
                        PaymentReceiptId = paymentReceipt.Id,
                        MainAccountId = payAccount.MainAccountId,
                        Amount = payAccount.Amount,
                        Tax = payAccount.Tax,
                        Description = payAccount.Description ?? paymentReceipt.Description,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }
                paymentReceipt.PaymentReceiptAccounts = paymentAccounts;
                _context.Set<PaymentReceipt>().Add(paymentReceipt);

                // Create reverse connection for project
                var reverseConnection = new ReceiptConnection
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectId,
                    PaymentReceiptId = paymentReceipt.Id,
                    LinkedAmount = paymentReceipt.Amount,
                    ReceiptConnectionType = conn.ReceiptConnectionType,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.ReceiptConnections.Add(reverseConnection);

                // 3. Create transaction entries for reverse PaymentReceipt
                var transactionTypeId = await _context.TransactionTypes
                    .IgnoreQueryFilters()
                    .Where(t => t.ArabicName == "سند صرف" || t.EnglishName == "PaymentReceipt")
                    .Select(t => t.Id)
                    .FirstOrDefaultAsync();

                if (transactionTypeId != Guid.Empty)
                {
                    var treasury = await _context.Treasuries.FindAsync(paymentReceipt.TreasuryId);
                    if (treasury != null && treasury.MainAccountId != Guid.Empty)
                    {
                        var reverseTransaction = new Transaction
                        {
                            Id = Guid.NewGuid(),
                            TransactionTypeId = transactionTypeId,
                            TransactionDate = paymentReceipt.ReceiptDate,
                            Description = paymentReceipt.Description,
                            ReferenceNumber = paymentReceipt.ReferenceNumber,
                            IsOpeningTransaction = false,
                            IsFrontal = paymentReceipt.IsFrontal,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        };

                        var reverseEntries = new List<TransactionEntry>();

                        foreach (var acc in paymentReceipt.PaymentReceiptAccounts)
                        {
                            reverseEntries.Add(new TransactionEntry
                            {
                                Id = Guid.NewGuid(),
                                TransactionId = reverseTransaction.Id,
                                MainAccountId = acc.MainAccountId,
                                DebitAmount = acc.Amount,
                                CreditAmount = 0,
                                Description = acc.Description,
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow
                            });
                        }

                        reverseEntries.Add(new TransactionEntry
                        {
                            Id = Guid.NewGuid(),
                            TransactionId = reverseTransaction.Id,
                            MainAccountId = treasury.MainAccountId,
                            DebitAmount = 0,
                            CreditAmount = paymentReceipt.Amount,
                            Description = paymentReceipt.Description,
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        });

                        reverseTransaction.TransactionEntries = reverseEntries;
                        _context.Transactions.Add(reverseTransaction);

                        paymentReceipt.TransactionId = reverseTransaction.Id;
                    }
                }

                // Add history entry for deleting the check
                var deleteHistory = new ProjectHistory
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectId,
                    ActionType = "إلغاء دفعة",
                    ActionBy = await GetUserNameAsync(currentUserId),
                    ActionDate = DateTime.UtcNow,
                    NewValue = $"تم إلغاء دفعة (شيك/تحويل) رقم: {conn.PaycheckReceipt.ReferenceNumber} بمبلغ: {conn.PaycheckReceipt.Amount:N2} ريال. السبب: {reason}. وتم إنشاء سند صرف عكسي رقم: {paymentReceipt.ReferenceNumber}",
                    Notes = "تعديل دفعات المشروع"
                };
                _context.ProjectHistories.Add(deleteHistory);
            }
        }

        // 3. Add or Update
        foreach (var dto in checkDtos)
        {
            if (dto.Id.HasValue)
            {
                // Update
                var conn = existingConnections.FirstOrDefault(c => c.PaycheckReceiptId == dto.Id.Value);
                if (conn != null && conn.PaycheckReceipt != null)
                {
                    // Check if values actually changed
                    var isChanged = conn.PaycheckReceipt.Amount != dto.CheckAmount || 
                                    conn.PaycheckReceipt.ReferenceNumber != dto.CheckNumber ||
                                    (conn.PaycheckReceipt.PaymentMethodType != null && (int)conn.PaycheckReceipt.PaymentMethodType != dto.PaymentMethodType);

                    if (isChanged)
                    {
                        var oldCheckVal = $"الرقم: {conn.PaycheckReceipt.ReferenceNumber}، المبلغ: {conn.PaycheckReceipt.Amount:N2} ريال، الطريقة: {conn.PaycheckReceipt.PaymentMethodType}";
                        var newCheckVal = $"الرقم الجديد: {dto.CheckNumber}، المبلغ الجديد: {dto.CheckAmount:N2} ريال، الطريقة الجديدة: {dto.PaymentMethodType}";

                        var updateHistory = new ProjectHistory
                        {
                            Id = Guid.NewGuid(),
                            ProjectId = projectId,
                            ActionType = "تعديل دفعة",
                            ActionBy = await GetUserNameAsync(currentUserId),
                            ActionDate = DateTime.UtcNow,
                            OldValue = oldCheckVal,
                            NewValue = newCheckVal,
                            Notes = "تعديل دفعات المشروع"
                        };
                        _context.ProjectHistories.Add(updateHistory);
                    }

                    conn.LinkedAmount = dto.CheckAmount;
                    conn.PaycheckReceipt.ReferenceNumber = dto.CheckNumber;
                    conn.PaycheckReceipt.Amount = dto.CheckAmount;
                    conn.PaycheckReceipt.ReceiptDate = DateOnly.FromDateTime(dto.CheckReceiptDate);
                    var isTreasuryValidForUpdate = await _context.Treasuries.AnyAsync(t => t.Id == dto.TreasuryId);
                    Guid validTreasuryIdForUpdate = dto.TreasuryId;
                    string? bankNameFromAccountForUpdate = dto.BankName;

                    if (!isTreasuryValidForUpdate)
                    {
                        var bankAcc = await _context.GeneralBankAccounts.FindAsync(dto.TreasuryId);
                        if (bankAcc != null)
                        {
                            bankNameFromAccountForUpdate = string.IsNullOrWhiteSpace(bankAcc.BankName) 
                                ? bankAcc.AccountName 
                                : $"{bankAcc.AccountName} - {bankAcc.BankName}";
                        }

                        var fallbackTreasuryId = await _context.Treasuries
                            .Where(t => t.IsActive == true)
                            .Select(t => t.Id)
                            .FirstOrDefaultAsync();

                        if (fallbackTreasuryId != Guid.Empty)
                        {
                            validTreasuryIdForUpdate = fallbackTreasuryId;
                        }
                    }

                    conn.PaycheckReceipt.TreasuryId = validTreasuryIdForUpdate;
                    conn.PaycheckReceipt.Description = $"تعديل دفعة رقم {dto.CheckNumber} للمشروع";
                    conn.PaycheckReceipt.PaymentMethodType = dto.PaymentMethodType.HasValue ? (Domains.Enums.PaymentMethodType)dto.PaymentMethodType.Value : null;
                    conn.PaycheckReceipt.CheckNumber = dto.CheckNumberDetail;
                    conn.PaycheckReceipt.BankName = bankNameFromAccountForUpdate;
                    conn.PaycheckReceipt.BankReferenceNumber = dto.BankReferenceNumber;
                    conn.PaycheckReceipt.VoucherNumber = dto.VoucherNumber;
                    conn.PaycheckReceipt.TransactionNumber = dto.TransactionNumber;

                    // Update accounts
                    var acc = conn.PaycheckReceipt.PaycheckReceiptAccounts?.FirstOrDefault();
                    if (acc != null)
                    {
                        acc.Amount = dto.CheckAmount;
                        acc.MainAccountId = dto.MainAccountId;
                    }
                    else
                    {
                        conn.PaycheckReceipt.PaycheckReceiptAccounts = new List<PaycheckReceiptAccount>
                        {
                            new PaycheckReceiptAccount
                            {
                                Id = Guid.NewGuid(),
                                Amount = dto.CheckAmount,
                                MainAccountId = dto.MainAccountId,
                                Description = conn.PaycheckReceipt.Description,
                                IsActive = true,
                                CreatedAt = DateTime.UtcNow
                            }
                        };
                    }

                    // Update accounting transaction
                    await UpdateTransactionForPaycheckReceiptInContextAsync(conn.PaycheckReceipt);
                }
            }
            else
            {
                var checkRef = string.IsNullOrWhiteSpace(dto.CheckNumber) ? $"PRJ-{Guid.NewGuid().ToString("N")[..6]}" : dto.CheckNumber;
                if (await _context.PaycheckReceipts.AnyAsync(pr => pr.ReferenceNumber == checkRef))
                {
                    checkRef = $"PRJ-{dto.CheckNumber}-{Guid.NewGuid().ToString("N")[..4]}";
                }

                var isTreasuryValid = await _context.Treasuries.AnyAsync(t => t.Id == dto.TreasuryId);
                Guid validTreasuryId = dto.TreasuryId;
                string? bankNameFromAccount = dto.BankName;

                if (!isTreasuryValid)
                {
                    var bankAcc = await _context.GeneralBankAccounts.FindAsync(dto.TreasuryId);
                    if (bankAcc != null)
                    {
                        bankNameFromAccount = string.IsNullOrWhiteSpace(bankAcc.BankName) 
                            ? bankAcc.AccountName 
                            : $"{bankAcc.AccountName} - {bankAcc.BankName}";
                    }

                    var fallbackTreasuryId = await _context.Treasuries
                        .Where(t => t.IsActive == true)
                        .Select(t => t.Id)
                        .FirstOrDefaultAsync();

                    if (fallbackTreasuryId != Guid.Empty)
                    {
                        validTreasuryId = fallbackTreasuryId;
                    }
                }

                // Add new PaycheckReceipt
                var receipt = new PaycheckReceipt
                {
                    Id = Guid.NewGuid(),
                    ReferenceNumber = checkRef,
                    Amount = dto.CheckAmount,
                    ReceiptDate = DateOnly.FromDateTime(dto.CheckReceiptDate),
                    TreasuryId = validTreasuryId,
                    Description = $"دفعة رقم {dto.CheckNumber} للمشروع",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    PaymentMethodType = dto.PaymentMethodType.HasValue ? (Domains.Enums.PaymentMethodType)dto.PaymentMethodType.Value : null,
                    CheckNumber = dto.CheckNumberDetail,
                    BankName = bankNameFromAccount,
                    BankReferenceNumber = dto.BankReferenceNumber,
                    VoucherNumber = dto.VoucherNumber,
                    TransactionNumber = dto.TransactionNumber,
                    PaycheckReceiptAccounts = new List<PaycheckReceiptAccount>
                    {
                        new PaycheckReceiptAccount
                        {
                            Id = Guid.NewGuid(),
                            Amount = dto.CheckAmount,
                            MainAccountId = dto.MainAccountId,
                            Description = $"دفعة رقم {dto.CheckNumber} للمشروع",
                            IsActive = true,
                            CreatedAt = DateTime.UtcNow
                        }
                    }
                };

                _context.PaycheckReceipts.Add(receipt);

                // Add history entry for adding the check
                var addHistory = new ProjectHistory
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectId,
                    ActionType = "إضافة دفعة",
                    ActionBy = await GetUserNameAsync(currentUserId),
                    ActionDate = DateTime.UtcNow,
                    NewValue = $"تم إضافة دفعة جديدة (شيك/تحويل) رقم: {dto.CheckNumber} بمبلغ: {dto.CheckAmount:N2} ريال",
                    Notes = "تعديل دفعات المشروع"
                };
                _context.ProjectHistories.Add(addHistory);

                // Create connection
                var conn = new ReceiptConnection
                {
                    Id = Guid.NewGuid(),
                    ProjectId = projectId,
                    PaycheckReceiptId = receipt.Id,
                    LinkedAmount = dto.CheckAmount,
                    ReceiptConnectionType = ReceiptConnectionType.Project,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.ReceiptConnections.Add(conn);

                // Create transaction
                await CreateTransactionForPaycheckReceiptInContextAsync(receipt);
            }
        }
    }

    private async Task CreateTransactionForPaycheckReceiptInContextAsync(PaycheckReceipt paycheckReceipt)
    {
        var transactionType = await _context.TransactionTypes
            .FirstOrDefaultAsync(tt => tt.ArabicName.Contains("سند قبض"));
        if (transactionType == null) return;

        var treasury = await _context.Treasuries.FindAsync(paycheckReceipt.TreasuryId);
        Guid debitAccountId = Guid.Empty;

        if (treasury != null && treasury.MainAccountId != Guid.Empty)
        {
            debitAccountId = treasury.MainAccountId;
        }
        else
        {
            var bankAccount = await _context.GeneralBankAccounts.FindAsync(paycheckReceipt.TreasuryId);
            if (bankAccount != null && bankAccount.MainAccountId != Guid.Empty)
            {
                debitAccountId = bankAccount.MainAccountId;
            }
        }

        if (debitAccountId == Guid.Empty && treasury != null)
        {
            debitAccountId = treasury.MainAccountId;
        }

        if (debitAccountId == Guid.Empty) return;

        var transactionEntries = new List<TransactionEntry>();

        foreach (var account in paycheckReceipt.PaycheckReceiptAccounts)
        {
            var entry = new TransactionEntry
            {
                Id = Guid.NewGuid(),
                CreditAmount = account.Amount,
                DebitAmount = 0,
                MainAccountId = account.MainAccountId,
                Description = account.Description ?? paycheckReceipt.Description
            };
            transactionEntries.Add(entry);
        }

        var mainEntry = new TransactionEntry
        {
            Id = Guid.NewGuid(),
            CreditAmount = 0,
            DebitAmount = paycheckReceipt.Amount,
            MainAccountId = debitAccountId,
            Description = paycheckReceipt.Description
        };
        transactionEntries.Add(mainEntry);

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            TransactionTypeId = transactionType.Id,
            Description = paycheckReceipt.Description,
            TransactionDate = paycheckReceipt.ReceiptDate,
            TransactionEntries = transactionEntries,
            IsFrontal = paycheckReceipt.IsFrontal,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Transactions.Add(transaction);
        paycheckReceipt.TransactionId = transaction.Id;
    }

    private async Task UpdateTransactionForPaycheckReceiptInContextAsync(PaycheckReceipt paycheckReceipt)
    {
        if (paycheckReceipt.TransactionId == null)
        {
            await CreateTransactionForPaycheckReceiptInContextAsync(paycheckReceipt);
            return;
        }

        var transaction = await _context.Transactions
            .Include(t => t.TransactionEntries)
            .FirstOrDefaultAsync(t => t.Id == paycheckReceipt.TransactionId);

        if (transaction != null)
        {
            // Remove existing entries
            if (transaction.TransactionEntries != null)
            {
                _context.TransactionEntries.RemoveRange(transaction.TransactionEntries);
                transaction.TransactionEntries.Clear();
            }
            else
            {
                transaction.TransactionEntries = new List<TransactionEntry>();
            }

            var treasury = await _context.Treasuries.FindAsync(paycheckReceipt.TreasuryId);
            if (treasury == null) return;

            var transactionEntries = new List<TransactionEntry>();

            foreach (var account in paycheckReceipt.PaycheckReceiptAccounts)
            {
                var entry = new TransactionEntry
                {
                    Id = Guid.NewGuid(),
                    CreditAmount = account.Amount,
                    DebitAmount = 0,
                    MainAccountId = account.MainAccountId,
                    Description = account.Description ?? paycheckReceipt.Description,
                    TransactionId = transaction.Id
                };
                transactionEntries.Add(entry);
            }

            var mainEntry = new TransactionEntry
            {
                Id = Guid.NewGuid(),
                CreditAmount = 0,
                DebitAmount = paycheckReceipt.Amount,
                MainAccountId = treasury.MainAccountId,
                Description = paycheckReceipt.Description,
                TransactionId = transaction.Id
            };
            transactionEntries.Add(mainEntry);

            transaction.Description = paycheckReceipt.Description;
            transaction.TransactionDate = paycheckReceipt.ReceiptDate;
            
            foreach (var entry in transactionEntries)
            {
                transaction.TransactionEntries.Add(entry);
                _context.Entry(entry).State = EntityState.Added;
            }
        }
    }

    public async Task<ProjectExecutionMilestoneResponse> ToggleMilestoneCompletionAsync(Guid milestoneId, bool isCompleted, Guid currentUserId)
    {
        var milestone = await _context.ProjectExecutionMilestones
            .FirstOrDefaultAsync(m => m.Id == milestoneId);
        if (milestone == null)
            throw new Exception("المرحلة غير موجودة");

        milestone.IsCompleted = isCompleted;
        milestone.CompletionDate = isCompleted ? DateTime.UtcNow : null;

        // Recalculate completion percentage for the project
        var milestones = await _context.ProjectExecutionMilestones
            .Where(m => m.ProjectId == milestone.ProjectId && m.IsActive)
            .ToListAsync();

        if (milestones.Count > 0)
        {
            var completedCount = milestones.Count(m => m.IsCompleted);
            decimal completionPercentage = ((decimal)completedCount / milestones.Count) * 100;

            var executionPhase = await _context.ProjectExecutionPhases
                .FirstOrDefaultAsync(ep => ep.ProjectId == milestone.ProjectId);

            if (executionPhase == null)
            {
                executionPhase = new ProjectExecutionPhase
                {
                    Id = Guid.NewGuid(),
                    ProjectId = milestone.ProjectId,
                    CompletionPercentage = completionPercentage,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = currentUserId
                };
                _context.ProjectExecutionPhases.Add(executionPhase);
            }
            else
            {
                executionPhase.CompletionPercentage = completionPercentage;
            }

            // تحديث حالة المشروع الرئيسية تلقائياً عند اكتمال كافة مراحل/معالم المشروع
            var projectEntity = await _context.Projects.FindAsync(milestone.ProjectId);
            if (projectEntity != null)
            {
                if (completedCount == milestones.Count && milestones.Count > 0)
                {
                    if (projectEntity.Status != "منفذ" && projectEntity.Status != "مكتمل")
                    {
                        var oldStatus = projectEntity.Status;
                        projectEntity.Status = "منفذ";
                        _context.ProjectHistories.Add(new ProjectHistory
                        {
                            Id = Guid.NewGuid(),
                            ProjectId = projectEntity.Id,
                            ActionType = "تغيير حالة تلقائي",
                            ActionBy = await GetUserNameAsync(currentUserId),
                            ActionDate = DateTime.UtcNow,
                            OldValue = $"الحالة السابقة: {oldStatus}",
                            NewValue = "الحالة الجديدة: منفذ",
                            Notes = "تم إكتمال جميع مراحل/معالم المشروع وتغيير الحالة تلقائياً إلى منفذ"
                        });
                    }
                }
                else if (projectEntity.Status == "منفذ" || projectEntity.Status == "مكتمل")
                {
                    if (!isCompleted)
                    {
                        var oldStatus = projectEntity.Status;
                        projectEntity.Status = "قيد التنفيذ";
                        _context.ProjectHistories.Add(new ProjectHistory
                        {
                            Id = Guid.NewGuid(),
                            ProjectId = projectEntity.Id,
                            ActionType = "تغيير حالة تلقائي",
                            ActionBy = await GetUserNameAsync(currentUserId),
                            ActionDate = DateTime.UtcNow,
                            OldValue = $"الحالة السابقة: {oldStatus}",
                            NewValue = "الحالة الجديدة: قيد التنفيذ",
                            Notes = "تغيير الحالة تلقائياً لعدم اكتمال كافة المراحل"
                        });
                    }
                }
            }
        }

        // Add history entry
        var history = new ProjectHistory
        {
            Id = Guid.NewGuid(),
            ProjectId = milestone.ProjectId,
            ActionType = isCompleted ? "إكمال مرحلة" : "إلغاء إكمال مرحلة",
            ActionBy = await GetUserNameAsync(currentUserId),
            ActionDate = DateTime.UtcNow,
            NewValue = isCompleted ? $"تم إكمال المرحلة: {milestone.MilestoneName}" : $"تم إلغاء إكمال المرحلة: {milestone.MilestoneName}",
            Notes = $"تحديث حالة اكتمال المرحلة"
        };
        _context.ProjectHistories.Add(history);

        await _context.SaveChangesAsync();

        return new ProjectExecutionMilestoneResponse
        {
            Id = milestone.Id,
            ProjectId = milestone.ProjectId,
            MilestoneName = milestone.MilestoneName,
            Duration = milestone.Duration,
            DurationUnit = milestone.DurationUnit,
            SequenceOrder = milestone.SequenceOrder,
            IsCompleted = milestone.IsCompleted,
            CompletionDate = milestone.CompletionDate,
            TargetStartDate = milestone.TargetStartDate,
            TargetEndDate = milestone.TargetEndDate
        };
    }

    private async Task RecalculateMilestoneDatesAsync(Guid projectId, DateTime? executionStartDate)
    {
        var milestones = await _context.ProjectExecutionMilestones
            .Where(m => m.ProjectId == projectId && m.IsActive)
            .OrderBy(m => m.SequenceOrder)
            .ThenBy(m => m.CreatedAt)
            .ToListAsync();

        if (milestones.Count == 0) return;

        if (!executionStartDate.HasValue)
        {
            foreach (var m in milestones)
            {
                m.TargetStartDate = null;
                m.TargetEndDate = null;
            }
        }
        else
        {
            DateTime currentStart = executionStartDate.Value;
            foreach (var m in milestones)
            {
                m.TargetStartDate = currentStart;
                m.TargetEndDate = AddDuration(currentStart, m.Duration, m.DurationUnit);
                currentStart = m.TargetEndDate.Value;
            }
        }
    }

    private DateTime AddDuration(DateTime start, int duration, string unit)
    {
        switch (unit?.ToLower() ?? "month")
        {
            case "day":
            case "days":
            case "يوم":
                return start.AddDays(duration);
            case "week":
            case "weeks":
            case "أسبوع":
            case "اسبوع":
                return start.AddDays(duration * 7);
            case "year":
            case "years":
            case "سنة":
            case "عام":
                return start.AddYears(duration);
            case "month":
            case "months":
            case "شهر":
            default:
                return start.AddMonths(duration);
        }
    }

    private void ValidateMilestonesDuration(int? totalDuration, string? totalDurationUnit, List<int> durations, List<string> units)
    {
        if (!totalDuration.HasValue || totalDuration == 0) return;

        double totalDays = ToDays(totalDuration.Value, totalDurationUnit);
        double milestonesDays = 0;
        for (int i = 0; i < durations.Count; i++)
        {
            milestonesDays += ToDays(durations[i], units[i]);
        }

        if (Math.Abs(totalDays - milestonesDays) > 5)
        {
            throw new Exception($"مجموع مدد المراحل ({milestonesDays / 30:F1} شهر تقريباً) يجب أن يساوي مدة تنفيذ المشروع الإجمالية ({totalDays / 30:F1} شهر تقريباً)");
        }
    }

    private double ToDays(int value, string? unit)
    {
        switch (unit?.ToLower() ?? "month")
        {
            case "day":
            case "days":
                return value;
            case "week":
            case "weeks":
                return value * 7;
            case "year":
            case "years":
                return value * 365;
            case "month":
            case "months":
            default:
                return value * 30;
        }
    }

    public async Task<List<string>> GetProjectStatusesAsync()
    {
        var defaultStatuses = new List<string>
        {
            "جديد",
            "قيد الدراسة",
            "قيد التخطيط",
            "قيد التنفيذ",
            "منفذ",
            "معلق",
            "ملغي",
            "مؤرشف"
        };

        var dbProjectStatuses = await _context.Projects
            .Where(p => !p.IsDeleted && !string.IsNullOrWhiteSpace(p.Status))
            .Select(p => p.Status.Trim())
            .Distinct()
            .ToListAsync();

        var systemSetting = await _context.Set<SmartAndPro.ERP.Domains.Entities.SystemSetting>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync();

        var customList = new List<string>();
        if (systemSetting != null && systemSetting.DefaultReservationUncheckedPolicyIds != null)
        {
            customList = systemSetting.DefaultReservationUncheckedPolicyIds
                .Where(s => s.StartsWith("STATUS:"))
                .Select(s => s.Substring("STATUS:".Length).Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
        }

        var result = new List<string>(defaultStatuses);
        foreach (var st in dbProjectStatuses.Concat(customList))
        {
            if (!result.Contains(st, StringComparer.OrdinalIgnoreCase))
            {
                result.Add(st);
            }
        }

        return result;
    }

    public async Task<List<string>> AddCustomProjectStatusAsync(string status)
    {
        var trimmed = status?.Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return await GetProjectStatusesAsync();
        }

        var systemSetting = await _context.Set<SmartAndPro.ERP.Domains.Entities.SystemSetting>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync();

        if (systemSetting == null)
        {
            systemSetting = new SmartAndPro.ERP.Domains.Entities.SystemSetting
            {
                Id = Guid.NewGuid(),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                TimeZone = "Asia/Riyadh",
                DefaultReservationUncheckedPolicyIds = new List<string>()
            };
            _context.Set<SmartAndPro.ERP.Domains.Entities.SystemSetting>().Add(systemSetting);
        }

        if (systemSetting.DefaultReservationUncheckedPolicyIds == null)
        {
            systemSetting.DefaultReservationUncheckedPolicyIds = new List<string>();
        }

        var statusKey = $"STATUS:{trimmed}";
        if (!systemSetting.DefaultReservationUncheckedPolicyIds.Contains(statusKey, StringComparer.OrdinalIgnoreCase))
        {
            systemSetting.DefaultReservationUncheckedPolicyIds.Add(statusKey);
            await _context.SaveChangesAsync();
        }

        return await GetProjectStatusesAsync();
    }
}
