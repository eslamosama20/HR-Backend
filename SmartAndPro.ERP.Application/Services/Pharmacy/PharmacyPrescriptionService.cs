using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SmartAndPro.ERP.Application.IServices;
using SmartAndPro.ERP.Application.DTOs.Pharmacy;
using SmartAndPro.ERP.Domains.Entities.Pharmacy;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Enums;
using Microsoft.AspNetCore.Http;

namespace SmartAndPro.ERP.Application.Services.Pharmacy
{
    public class PharmacyPrescriptionService : IPharmacyPrescriptionService
    {
        private readonly RepositoriesFactory _repositoriesFactory;
        private readonly ILogger<PharmacyPrescriptionService> _logger;

        public PharmacyPrescriptionService(
            RepositoriesFactory repositoriesFactory,
            ILogger<PharmacyPrescriptionService> logger)
        {
            _repositoriesFactory = repositoriesFactory;
            _logger = logger;
        }

        private DbContext GetDbContext()
        {
            return _repositoriesFactory.GetRepository<PharmacyPrescription>().GetDbContext();
        }

        public async Task<PrescriptionListResponseDto> GetPrescriptionsAsync(PrescriptionListQueryDto query)
        {
            try
            {
                var repo = _repositoriesFactory.GetRepository<PharmacyPrescription>();
                var result = await repo.GetAll();
                var q = result.Data ?? Enumerable.Empty<PharmacyPrescription>().AsQueryable();

                if (!string.IsNullOrEmpty(query.Status))
                    q = q.Where(p => p.Status == query.Status);
                if (!string.IsNullOrEmpty(query.Priority))
                    q = q.Where(p => p.Priority == query.Priority);
                if (!string.IsNullOrEmpty(query.PaymentType))
                    q = q.Where(p => p.PaymentType == query.PaymentType);
                if (!string.IsNullOrEmpty(query.SearchTerm))
                {
                    var term = query.SearchTerm.ToLower();
                    q = q.Where(p => p.RxNo.ToLower().Contains(term) ||
                        p.PatientName.ToLower().Contains(term) ||
                        (p.DoctorName != null && p.DoctorName.ToLower().Contains(term)));
                }

                var totalCount = await q.CountAsync();
                var items = await q
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((query.Page - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .Select(p => new PrescriptionListItemDto
                    {
                        Id = p.Id,
                        RxNo = p.RxNo,
                        IssueDate = p.IssueDate,
                        PatientName = p.PatientName,
                        DoctorName = p.DoctorName ?? "",
                        Status = p.Status,
                        Priority = p.Priority,
                        PaymentType = p.PaymentType,
                        InsuranceName = p.InsuranceName,
                        ItemsCount = p.ItemsCount,
                        GrandTotal = p.GrandTotal,
                        PatientPayable = p.PatientPayable,
                        CreatedAt = p.CreatedAt
                    })
                    .ToListAsync();

                return new PrescriptionListResponseDto
                {
                    TotalCount = totalCount,
                    Page = query.Page,
                    PageSize = query.PageSize,
                    Items = items
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prescriptions list");
                throw;
            }
        }

        public async Task<PrescriptionDetailsDto?> GetPrescriptionByIdAsync(int id)
        {
            try
            {
                var repo = _repositoriesFactory.GetRepository<PharmacyPrescription>();
                var result = await repo.Filter(p => p.SeqNo == id);
                if (result.Data == null) return null;

                var prescription = await result.Data
                    .Include(p => p.Items)
                    .Include(p => p.Patient)
                    .Include(p => p.Doctor)
                    .Include(p => p.Attachments)
                    .Include(p => p.InsuranceCompany)
                    .FirstOrDefaultAsync();

                if (prescription == null) return null;

                return new PrescriptionDetailsDto
                {
                    Id = prescription.Id,
                    RxNo = prescription.RxNo,
                    IssueDate = prescription.IssueDate,
                    Status = prescription.Status,
                    Priority = prescription.Priority,
                    Patient = new PatientDto
                    {
                        Id = prescription.Patient?.Id ?? Guid.Empty,
                        Name = prescription.PatientName,
                        Mobile = prescription.Patient?.Mobile,
                        Dob = prescription.Patient?.Dob
                    },
                    Doctor = prescription.Doctor != null ? new DoctorDto
                    {
                        Id = prescription.Doctor.Id,
                        Name = prescription.DoctorName ?? prescription.Doctor.Name,
                        LicenseNo = prescription.Doctor.LicenseNo
                    } : null,
                    Branch = new BranchDto
                    {
                        Id = prescription.BranchId ?? "",
                        Name = prescription.BranchName ?? ""
                    },
                    Store = new StoreDto
                    {
                        Id = prescription.StoreId ?? "",
                        Name = prescription.StoreName ?? ""
                    },
                    PaymentType = prescription.PaymentType,
                    Notes = prescription.Notes,
                    Insurance = prescription.InsuranceEnabled ? new InsuranceDto
                    {
                        Enabled = true,
                        InsuranceCompanyId = prescription.InsuranceCompanyId,
                        InsuranceName = prescription.InsuranceName,
                        MemberNo = prescription.InsuranceMemberNo,
                        ApprovalNo = prescription.InsuranceApprovalNo,
                        CoveragePercentage = prescription.InsuranceCoveragePercentage,
                        PatientSharePercentage = prescription.InsurancePatientSharePercentage,
                        CoveredAmount = prescription.InsuranceCoveredAmount,
                        PatientPayableAmount = prescription.InsurancePatientPayableAmount,
                        Status = prescription.InsuranceStatus ?? "pending"
                    } : null,
                    Items = prescription.Items.Where(i => i.IsActive).Select(i => new PrescriptionItemDto
                    {
                        LineId = i.Id,
                        ItemId = i.ItemId,
                        ItemCode = i.ItemCode,
                        ItemName = i.ItemName,
                        RequestedQuantity = i.RequestedQty,
                        DispensedQuantity = i.DispensedQty,
                        StoreId = i.StoreId,
                        BatchId = i.BatchId,
                        BatchNo = i.BatchNo,
                        ExpiryDate = i.ExpiryDate,
                        UnitPrice = i.UnitPrice,
                        DiscountAmount = i.DiscountAmount,
                        TaxAmount = i.TaxAmount,
                        TotalAmount = i.LineTotal,
                        AllowAlternative = i.AllowSubstitute,
                        IsControlled = i.IsControlled,
                        Notes = i.Notes,
                        AvailabilityStatus = i.AvailabilityStatus
                    }).ToList(),
                    Attachments = prescription.Attachments.Where(a => a.IsActive).Select(a => new AttachmentDto
                    {
                        AttachmentId = a.SeqNo,
                        FileName = a.FileName,
                        ContentType = a.ContentType,
                        Size = a.Size,
                        UploadedAt = a.UploadedAt,
                        Notes = a.Notes
                    }).ToList(),
                    Alerts = new List<AlertDto>(),
                    Financials = new FinancialsDto
                    {
                        SubTotal = prescription.SubTotal,
                        DiscountTotal = prescription.DiscountTotal,
                        TaxTotal = prescription.TaxTotal,
                        GrandTotal = prescription.GrandTotal,
                        InsuranceCovered = prescription.InsuranceCoveredAmount,
                        PatientPayable = prescription.PatientPayable
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting prescription details for ID: {Id}", id);
                throw;
            }
        }

        public async Task<CreatePrescriptionResponse> CreatePrescriptionAsync(CreatePrescriptionRequest request, string userId)
        {
            try
            {
                var prescriptionRepo = _repositoriesFactory.GetRepository<PharmacyPrescription>();
                var itemRepo = _repositoriesFactory.GetRepository<PharmacyPrescriptionItem>();
                var createdBy = Guid.TryParse(userId, out var uid) ? uid : Guid.Empty;

                // Look up patient
                string patientName = "";
                Guid? patientGuid = null;
                if (request.PatientId != Guid.Empty)
                {
                    var patientRepo = _repositoriesFactory.GetRepository<PharmacyPatient>();
                    var patientResult = await patientRepo.Filter(p => p.Id == request.PatientId);
                    var patient = patientResult.Data?.FirstOrDefault();
                    if (patient != null)
                    {
                        patientName = patient.FullName;
                        patientGuid = patient.Id;
                    }
                }

                // Look up doctor
                string? doctorName = null;
                Guid? doctorGuid = null;
                if (request.DoctorId.HasValue && request.DoctorId.Value != Guid.Empty)
                {
                    var doctorRepo = _repositoriesFactory.GetRepository<PharmacyDoctor>();
                    var doctorResult = await doctorRepo.Filter(d => d.Id == request.DoctorId.Value);
                    var doctor = doctorResult.Data?.FirstOrDefault();
                    if (doctor != null)
                    {
                        doctorName = doctor.Name;
                        doctorGuid = doctor.Id;
                    }
                }

                var prescription = new PharmacyPrescription
                {
                    RxNo = request.RxNo,
                    IssueDate = request.IssueDate,
                    PatientId = patientGuid,
                    PatientName = patientName,
                    DoctorId = doctorGuid,
                    DoctorName = doctorName,
                    BranchId = request.BranchId,
                    StoreId = request.StoreId,
                    PaymentType = request.PaymentType,
                    Priority = request.Priority,
                    Status = "new",
                    Notes = request.Notes,
                    ItemsCount = request.Items.Count,
                    CreatedBy = createdBy
                };

                var addResult = await prescriptionRepo.Add(prescription);
                if (addResult.ErrorCode != ErrorCodes.Success || addResult.Data == null)
                    throw new InvalidOperationException("Failed to create prescription");

                // Create items
                var lineNo = 1;
                foreach (var item in request.Items)
                {
                    var prescItem = new PharmacyPrescriptionItem
                    {
                        PrescriptionId = addResult.Data.Id,
                        PrescriptionRxNo = request.RxNo,
                        LineNo = lineNo++,
                        ItemId = item.ItemId,
                        ItemCode = "",
                        ItemName = "",
                        RequestedQty = item.RequestedQuantity,
                        DispensedQty = item.DispensedQuantity,
                        StoreId = item.StoreId,
                        BatchId = item.BatchId,
                        AllowSubstitute = item.AllowAlternative,
                        Notes = item.Notes,
                        CreatedBy = createdBy
                    };
                    await itemRepo.Add(prescItem);
                }

                return new CreatePrescriptionResponse
                {
                    Id = addResult.Data.Id,
                    RxNo = addResult.Data.RxNo,
                    Status = addResult.Data.Status,
                    GrandTotal = addResult.Data.GrandTotal,
                    CreatedAt = addResult.Data.CreatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating prescription");
                throw;
            }
        }

        public async Task<UpdatePrescriptionResponse> UpdatePrescriptionAsync(int id, UpdatePrescriptionRequest request, string userId)
        {
            try
            {
                var db = GetDbContext();
                var prescription = await db.Set<PharmacyPrescription>()
                    .FirstOrDefaultAsync(p => p.SeqNo == id);
                if (prescription == null)
                    throw new KeyNotFoundException($"Prescription with ID {id} not found");

                // Look up patient
                if (request.PatientId != Guid.Empty)
                {
                    var patientRepo = _repositoriesFactory.GetRepository<PharmacyPatient>();
                    var patientResult = await patientRepo.Filter(p => p.Id == request.PatientId);
                    var patient = patientResult.Data?.FirstOrDefault();
                    if (patient != null)
                    {
                        prescription.PatientId = patient.Id;
                        prescription.PatientName = patient.FullName;
                    }
                }

                // Look up doctor
                if (request.DoctorId.HasValue && request.DoctorId.Value != Guid.Empty)
                {
                    var doctorRepo = _repositoriesFactory.GetRepository<PharmacyDoctor>();
                    var doctorResult = await doctorRepo.Filter(d => d.Id == request.DoctorId.Value);
                    var doctor = doctorResult.Data?.FirstOrDefault();
                    if (doctor != null)
                    {
                        prescription.DoctorId = doctor.Id;
                        prescription.DoctorName = doctor.Name;
                    }
                }

                prescription.RxNo = request.RxNo;
                prescription.IssueDate = request.IssueDate;
                prescription.BranchId = request.BranchId;
                prescription.StoreId = request.StoreId;
                prescription.PaymentType = request.PaymentType;
                prescription.Priority = request.Priority;
                prescription.Notes = request.Notes;
                if (!string.IsNullOrEmpty(request.Status))
                    prescription.Status = request.Status;

                // Replace items - soft delete old ones
                var oldItems = await db.Set<PharmacyPrescriptionItem>()
                    .Where(i => i.PrescriptionId == prescription.Id && i.IsActive)
                    .ToListAsync();
                foreach (var old in oldItems)
                {
                    old.SoftDelete();
                }

                var lineNo = 1;
                var createdBy = Guid.TryParse(userId, out var uid) ? uid : Guid.Empty;
                foreach (var item in request.Items)
                {
                    db.Set<PharmacyPrescriptionItem>().Add(new PharmacyPrescriptionItem
                    {
                        PrescriptionId = prescription.Id,
                        PrescriptionRxNo = prescription.RxNo,
                        LineNo = lineNo++,
                        ItemId = item.ItemId,
                        ItemCode = "",
                        ItemName = "",
                        RequestedQty = item.RequestedQuantity,
                        DispensedQty = item.DispensedQuantity,
                        StoreId = item.StoreId,
                        BatchId = item.BatchId,
                        AllowSubstitute = item.AllowAlternative,
                        Notes = item.Notes,
                        CreatedBy = createdBy
                    });
                }

                prescription.ItemsCount = request.Items.Count;

                await db.SaveChangesAsync();

                return new UpdatePrescriptionResponse
                {
                    Id = prescription.Id,
                    Status = prescription.Status,
                    UpdatedAt = DateTime.UtcNow,
                    GrandTotal = prescription.GrandTotal
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating prescription");
                throw;
            }
        }

        public async Task<UpdatePrescriptionStatusResponse> UpdatePrescriptionStatusAsync(int id, UpdatePrescriptionStatusRequest request, string userId)
        {
            try
            {
                var db = GetDbContext();
                var prescription = await db.Set<PharmacyPrescription>()
                    .FirstOrDefaultAsync(p => p.SeqNo == id);
                if (prescription == null)
                    throw new KeyNotFoundException($"Prescription with ID {id} not found");

                var oldStatus = prescription.Status;
                prescription.Status = request.Status;
                if (!string.IsNullOrEmpty(request.Notes))
                    prescription.Notes = request.Notes;

                await db.SaveChangesAsync();

                return new UpdatePrescriptionStatusResponse
                {
                    Id = prescription.Id,
                    OldStatus = oldStatus,
                    NewStatus = prescription.Status,
                    UpdatedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating prescription status");
                throw;
            }
        }

        public async Task<bool> DeletePrescriptionAsync(int id, string userId)
        {
            try
            {
                var db = GetDbContext();
                var prescription = await db.Set<PharmacyPrescription>()
                    .FirstOrDefaultAsync(p => p.SeqNo == id);
                if (prescription == null) return false;

                prescription.SoftDelete();
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting prescription");
                throw;
            }
        }

        public async Task<DrugInteractionCheckResponse> CheckDrugInteractionsAsync(DrugInteractionCheckRequest request)
        {
            try
            {
                // Drug interaction checking requires a specialized drug interaction database
                // Returning no interactions for now - integrate with drug interaction API when available
                return await Task.FromResult(new DrugInteractionCheckResponse
                {
                    HasInteractions = false,
                    Interactions = new List<InteractionDto>(),
                    Warnings = new List<string>()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking drug interactions");
                throw;
            }
        }

        public async Task<UpdateInsuranceResponse> UpdateInsuranceAsync(int id, UpdateInsuranceRequest request, string userId)
        {
            try
            {
                var db = GetDbContext();
                var prescription = await db.Set<PharmacyPrescription>()
                    .FirstOrDefaultAsync(p => p.SeqNo == id);
                if (prescription == null)
                    throw new KeyNotFoundException($"Prescription with ID {id} not found");

                prescription.InsuranceEnabled = request.Enabled;
                prescription.InsuranceMemberNo = request.MemberNo;
                prescription.InsuranceApprovalNo = request.ApprovalNo;
                prescription.InsuranceCoveragePercentage = request.CoveragePercentage ?? 0;
                prescription.InsurancePatientSharePercentage = request.PatientSharePercentage ?? 0;
                prescription.InsuranceStatus = request.Status;

                // Look up insurance company
                if (request.InsuranceCompanyId.HasValue && request.InsuranceCompanyId != Guid.Empty)
                {
                    var insCompany = await db.Set<PharmacyInsuranceCompany>()
                        .FirstOrDefaultAsync(c => c.Id == request.InsuranceCompanyId.Value);
                    if (insCompany != null)
                    {
                        prescription.InsuranceCompanyId = insCompany.Id;
                        prescription.InsuranceName = insCompany.Name;
                    }
                }

                // Recalculate insurance amounts
                if (request.Enabled)
                {
                    prescription.InsuranceCoveredAmount = prescription.GrandTotal * (prescription.InsuranceCoveragePercentage / 100);
                    prescription.InsurancePatientPayableAmount = prescription.GrandTotal - prescription.InsuranceCoveredAmount;
                    prescription.PatientPayable = prescription.InsurancePatientPayableAmount;
                }
                else
                {
                    prescription.InsuranceCoveredAmount = 0;
                    prescription.InsurancePatientPayableAmount = prescription.GrandTotal;
                    prescription.PatientPayable = prescription.GrandTotal;
                }

                await db.SaveChangesAsync();

                return new UpdateInsuranceResponse
                {
                    Insurance = new InsuranceDataDto
                    {
                        Enabled = prescription.InsuranceEnabled,
                        InsuranceCompanyId = request.InsuranceCompanyId,
                        InsuranceName = prescription.InsuranceName,
                        MemberNo = prescription.InsuranceMemberNo,
                        ApprovalNo = prescription.InsuranceApprovalNo,
                        CoveragePercentage = prescription.InsuranceCoveragePercentage,
                        PatientSharePercentage = prescription.InsurancePatientSharePercentage,
                        CoveredAmount = prescription.InsuranceCoveredAmount,
                        PatientPayableAmount = prescription.InsurancePatientPayableAmount,
                        Status = prescription.InsuranceStatus
                    },
                    Financials = new FinancialSummaryDto
                    {
                        SubTotal = prescription.SubTotal,
                        DiscountTotal = prescription.DiscountTotal,
                        TaxTotal = prescription.TaxTotal,
                        GrandTotal = prescription.GrandTotal,
                        InsuranceCovered = prescription.InsuranceCoveredAmount,
                        PatientPayable = prescription.PatientPayable
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating insurance");
                throw;
            }
        }

        public async Task<MarkReadyResponse> MarkAsReadyAsync(int id, MarkReadyRequest request, string userId)
        {
            try
            {
                var db = GetDbContext();
                var prescription = await db.Set<PharmacyPrescription>()
                    .FirstOrDefaultAsync(p => p.SeqNo == id);
                if (prescription == null)
                    throw new KeyNotFoundException($"Prescription with ID {id} not found");

                prescription.Status = "ready";
                if (!string.IsNullOrEmpty(request.Notes))
                    prescription.Notes = request.Notes;

                await db.SaveChangesAsync();

                return new MarkReadyResponse
                {
                    Id = prescription.Id,
                    Status = "ready",
                    UpdatedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking prescription as ready");
                throw;
            }
        }

        public async Task<SendToPosResponse> SendToPosAsync(int id, SendToPosRequest request, string userId)
        {
            try
            {
                var db = GetDbContext();
                var prescription = await db.Set<PharmacyPrescription>()
                    .FirstOrDefaultAsync(p => p.SeqNo == id);
                if (prescription == null)
                    throw new KeyNotFoundException($"Prescription with ID {id} not found");

                prescription.Status = "sent_to_pos";
                prescription.PosTerminalId = request.PosTerminalId;
                prescription.PosQueueNo = $"Q-{prescription.SeqNo}";

                await db.SaveChangesAsync();

                return new SendToPosResponse
                {
                    Id = prescription.Id,
                    Status = "sent_to_pos",
                    PosQueueNo = prescription.PosQueueNo,
                    UpdatedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending prescription to POS");
                throw;
            }
        }

        public async Task<CancelPrescriptionResponse> CancelPrescriptionAsync(int id, CancelPrescriptionRequest request, string userId)
        {
            try
            {
                var db = GetDbContext();
                var prescription = await db.Set<PharmacyPrescription>()
                    .FirstOrDefaultAsync(p => p.SeqNo == id);
                if (prescription == null)
                    throw new KeyNotFoundException($"Prescription with ID {id} not found");

                prescription.Status = "cancelled";
                prescription.CancelReason = request.Reason;

                await db.SaveChangesAsync();

                return new CancelPrescriptionResponse
                {
                    Id = prescription.Id,
                    Status = "cancelled",
                    CancelReason = request.Reason,
                    UpdatedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling prescription");
                throw;
            }
        }

        public async Task<UploadAttachmentResponse> UploadAttachmentAsync(int id, IFormFile file, string? notes, string userId)
        {
            try
            {
                var prescriptionRepo = _repositoriesFactory.GetRepository<PharmacyPrescription>();
                var prescResult = await prescriptionRepo.Filter(p => p.SeqNo == id);
                var prescription = prescResult.Data?.FirstOrDefault();
                if (prescription == null)
                    throw new KeyNotFoundException($"Prescription with ID {id} not found");

                var attachmentRepo = _repositoriesFactory.GetRepository<PharmacyAttachment>();
                var attachment = new PharmacyAttachment
                {
                    PrescriptionId = prescription.Id,
                    FileName = file.FileName,
                    ContentType = file.ContentType,
                    Size = file.Length,
                    UploadedAt = DateTime.UtcNow,
                    Notes = notes,
                    CreatedBy = Guid.TryParse(userId, out var uid) ? uid : Guid.Empty
                };

                var addResult = await attachmentRepo.Add(attachment);
                if (addResult.ErrorCode != ErrorCodes.Success || addResult.Data == null)
                    throw new InvalidOperationException("Failed to create attachment record");

                return new UploadAttachmentResponse
                {
                    AttachmentId = addResult.Data.SeqNo,
                    FileName = addResult.Data.FileName,
                    ContentType = addResult.Data.ContentType,
                    Size = addResult.Data.Size,
                    UploadedAt = addResult.Data.UploadedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading attachment");
                throw;
            }
        }

        public async Task<List<AttachmentDto>> GetAttachmentsAsync(int id)
        {
            try
            {
                var prescriptionRepo = _repositoriesFactory.GetRepository<PharmacyPrescription>();
                var prescResult = await prescriptionRepo.Filter(p => p.SeqNo == id);
                var prescription = prescResult.Data?.FirstOrDefault();
                if (prescription == null) return new List<AttachmentDto>();

                var attachmentRepo = _repositoriesFactory.GetRepository<PharmacyAttachment>();
                var result = await attachmentRepo.Filter(a => a.PrescriptionId == prescription.Id);
                var attachments = await (result.Data ?? Enumerable.Empty<PharmacyAttachment>().AsQueryable())
                    .Select(a => new AttachmentDto
                    {
                        AttachmentId = a.SeqNo,
                        FileName = a.FileName,
                        ContentType = a.ContentType,
                        Size = a.Size,
                        UploadedAt = a.UploadedAt,
                        Notes = a.Notes
                    })
                    .ToListAsync();

                return attachments;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting attachments");
                throw;
            }
        }

        public async Task<bool> DeleteAttachmentAsync(int prescriptionId, int attachmentId, string userId)
        {
            try
            {
                var db = GetDbContext();
                var attachment = await db.Set<PharmacyAttachment>()
                    .FirstOrDefaultAsync(a => a.SeqNo == attachmentId);
                if (attachment == null) return false;

                attachment.SoftDelete();
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting attachment");
                throw;
            }
        }

        public async Task<ReadyPrescriptionsResponseDto> GetReadyPrescriptionsAsync(ReadyPrescriptionsQueryDto query)
        {
            try
            {
                var repo = _repositoriesFactory.GetRepository<PharmacyPrescription>();
                var result = await repo.Filter(p => p.Status == "ready");
                var q = result.Data ?? Enumerable.Empty<PharmacyPrescription>().AsQueryable();

                if (!string.IsNullOrEmpty(query.SearchTerm))
                {
                    var term = query.SearchTerm.ToLower();
                    q = q.Where(p => p.RxNo.ToLower().Contains(term) ||
                        p.PatientName.ToLower().Contains(term));
                }

                var totalCount = await q.CountAsync();
                var items = await q
                    .OrderByDescending(p => p.CreatedAt)
                    .Skip((query.Page - 1) * query.PageSize)
                    .Take(query.PageSize)
                    .Select(p => new ReadyPrescriptionItemDto
                    {
                        PrescriptionId = p.Id,
                        RxNo = p.RxNo,
                        PatientName = p.PatientName,
                        GrandTotal = p.GrandTotal,
                        PatientPayable = p.PatientPayable,
                        Status = "ready"
                    })
                    .ToListAsync();

                return new ReadyPrescriptionsResponseDto
                {
                    TotalCount = totalCount,
                    Items = items
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting ready prescriptions");
                throw;
            }
        }
    }
}
