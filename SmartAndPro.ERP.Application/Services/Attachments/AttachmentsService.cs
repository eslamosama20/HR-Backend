using AutoMapper;
using Microsoft.EntityFrameworkCore;
using SmartAndPro.ERP.Application.Common.Dtos.Attachments;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.Assets;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.Application.Common.Extensions;

namespace SmartAndPro.ERP.Application.Services.Attachments;


public class AttachmentsService
    : ServiceBase
    <
        Attachment,
        AttachmentPostDto,
        AttachmentPutDto,
        AttachmentResponseDto,
        AttachmentFiltersDto
    > 

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    
    public AttachmentsService(RepositoriesFactory repositoriesFactory, AppSettings appSettings , IMapper mapper)
        : base(repositoriesFactory, appSettings,mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }

    public override async Task<OperationResponse<List<AttachmentResponseDto>>> GetAllAsync(AttachmentFiltersDto filters,
        int? pageNumber = null, int? pageSize = null, string? sortField = null, int? sortOrder = null)
    {
        var result = new OperationResponse<List<AttachmentResponseDto>>(ErrorCodes.GeneralFail,
            null);
        try
        {
            var attachmentRepository = _repositoriesFactory.GetRepository<Attachment>();
            
            var attachmentQuery = await attachmentRepository.GetAll();
            if (attachmentQuery.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<List<AttachmentResponseDto>>(attachmentQuery.ErrorCode, null);
            
            if (attachmentQuery.Data is null)
                return new OperationResponse<List<AttachmentResponseDto>>(ErrorCodes.NoContent, null);

            IQueryable<Attachment> query = attachmentQuery.Data;

            // Apply SearchKeyword
            query = query.ApplySearchKeyword(filters.SearchKeyword);

            var totalRecords = await query.CountAsync();

            pageNumber ??= 1;
            pageSize ??= 50;
            if (pageSize > 100) pageSize = 100;

            if (pageNumber > 0 && pageSize > 0)
            {
                var skip = (pageNumber.Value - 1) * pageSize.Value;
                query = query.ApplySort(sortField, sortOrder).Skip(skip).Take(pageSize.Value);
            }

            var attachments = await query.Select(a => new AttachmentResponseDto
                {
                    Id = a.Id,
                    FileName = a.FileName,
                    FileType = a.FileType,
                    UploadedAt = a.UploadedAt,
                    ReferenceNumber = a.ReferenceNumber
                })
                .ToListAsync();

            if (pageNumber.HasValue && pageSize.HasValue)
            {
                return new OperationResponse<List<AttachmentResponseDto>>(
                    ErrorCodes.Success,
                    attachments,
                    totalCount: totalRecords,
                    isPaginated: true
                );
            }
            return new OperationResponse<List<AttachmentResponseDto>>(ErrorCodes.Success, attachments);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    public override async Task<OperationResponse<AttachmentResponseDto>> AddAsync(AttachmentPostDto attachmentPost)
    {
        var result = new OperationResponse<AttachmentResponseDto>(ErrorCodes.GeneralFail,
            null);
        try
        {
            byte[] fileData = Convert.FromBase64String(attachmentPost.File);
            var attachment = new Attachment
            {
                FileName = attachmentPost.FileName,
                FileType = attachmentPost.FileType,
                FileData =fileData,
                UploadedAt = DateTime.UtcNow,
                InventoryPermitId=attachmentPost.InventoryPermitId,
                PurchaseInvoiceId=attachmentPost.PurchaseInvoiceId,
                SaleInvoiceId=attachmentPost.SaleInvoiceId,
                SalesReturnId=attachmentPost.SalesReturnId,
                PurchaseReturnId=attachmentPost.PurchaseReturnId,
                PaymentReceiptId=attachmentPost.PaymentReceiptId,
                PaycheckReceiptId=attachmentPost.PaycheckReceiptId,
                DebitNoteId=attachmentPost.DebitNoteId,
                CreditNoteId=attachmentPost.CreditNoteId,
                InventoryStocktakingId=attachmentPost.InventoryStocktakingId,
                ReservationId=attachmentPost.ReservationId,
                TransactionId=attachmentPost.TransactionId
            };
            var attachmentRepository = _repositoriesFactory.GetRepository<Attachment>();
            var operation = await attachmentRepository.Add(attachment);
            
            if (operation.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<AttachmentResponseDto>(operation.ErrorCode, null);
            
            var responseData = _mapper.Map<AttachmentResponseDto>(operation.Data);
            return new OperationResponse<AttachmentResponseDto>(ErrorCodes.Success, responseData);
        }
        catch (FormatException ex)
        {
            LogService.LogError(ex);
            return new OperationResponse<AttachmentResponseDto>(ErrorCodes.BadRequest, null);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    public override async Task<OperationResponse<AttachmentResponseDto>> UpdateAsync(AttachmentPutDto attachmentPut)
    {
        var result = new OperationResponse<AttachmentResponseDto>(ErrorCodes.GeneralFail,
            null);
        try
        {
            var attachmentRepository = _repositoriesFactory.GetRepository<Attachment>();
            var attachmentQuery = await attachmentRepository.Filter(a => a.Id == attachmentPut.Id);
            if (attachmentQuery.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<AttachmentResponseDto>(attachmentQuery.ErrorCode, null);
            
            if (attachmentQuery.Data is null)
                return new OperationResponse<AttachmentResponseDto>(ErrorCodes.NotFound, null);
            
            var attachment = attachmentQuery.Data.FirstOrDefault();
            if (attachment is null)
                return new OperationResponse<AttachmentResponseDto>(ErrorCodes.NotFound, null);
            
            
            byte[] fileData = Convert.FromBase64String(attachmentPut.File);
            
            attachment.FileName = attachmentPut.FileName;
            attachment.FileType = attachmentPut.FileType;
            attachment.FileData = fileData;
            attachment.UploadedAt = DateTime.UtcNow;
            
            attachment.InventoryPermitId = attachmentPut.InventoryPermitId;
            attachment.PurchaseInvoiceId = attachmentPut.PurchaseInvoiceId;
            attachment.SaleInvoiceId = attachmentPut.SaleInvoiceId;
            attachment.SalesReturnId = attachmentPut.SalesReturnId;
            attachment.PurchaseReturnId = attachmentPut.PurchaseReturnId;
            attachment.PaymentReceiptId = attachmentPut.PaymentReceiptId;
            attachment.PaycheckReceiptId = attachmentPut.PaycheckReceiptId;
            attachment.DebitNoteId = attachmentPut.DebitNoteId;
            attachment.CreditNoteId = attachmentPut.CreditNoteId;
            attachment.InventoryStocktakingId = attachmentPut.InventoryStocktakingId;
            attachment.ReservationId = attachmentPut.ReservationId;
            
            var operation = await attachmentRepository.Update(attachment);
            
            if (operation.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<AttachmentResponseDto>(operation.ErrorCode, null);
            
            var responseData = _mapper.Map<AttachmentResponseDto>(operation.Data);
            return new OperationResponse<AttachmentResponseDto>(ErrorCodes.Success, responseData);
        }
        catch (FormatException ex)
        {
            LogService.LogError(ex);
            return new OperationResponse<AttachmentResponseDto>(ErrorCodes.BadRequest, null);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    public async Task<OperationResponse<AttachmentDownloadDto>> DownloadAsync(Guid id)
    {
        var result = new OperationResponse<AttachmentDownloadDto>(ErrorCodes.GeneralFail,
            null);
        try
        {
            var attachmentRepository = _repositoriesFactory.GetRepository<Attachment>();
            var attachmentQuery = await attachmentRepository.Filter(a => a.Id == id);
            if (attachmentQuery.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<AttachmentDownloadDto>(attachmentQuery.ErrorCode, null);
            
            if (attachmentQuery.Data is null)
                return new OperationResponse<AttachmentDownloadDto>(ErrorCodes.NotFound, null);
            
            var attachment = attachmentQuery.Data.FirstOrDefault();
            if (attachment is null)
                return new OperationResponse<AttachmentDownloadDto>(ErrorCodes.NotFound, null);
            
            var file = new AttachmentDownloadDto
            {
                FileName = attachment.FileName,
                FileType = attachment.FileType,
                Id = attachment.Id,
                FileData = Convert.ToBase64String(attachment.FileData),
                UploadedAt = attachment.UploadedAt,
                ReferenceNumber = attachment.ReferenceNumber,
                InventoryPermitId = attachment.InventoryPermitId,
                PurchaseInvoiceId = attachment.PurchaseInvoiceId,
                SaleInvoiceId = attachment.SaleInvoiceId,
                SalesReturnId = attachment.SalesReturnId,
                PurchaseReturnId = attachment.PurchaseReturnId,
                PaymentReceiptId = attachment.PaymentReceiptId,
                PaycheckReceiptId = attachment.PaycheckReceiptId,
                DebitNoteId = attachment.DebitNoteId,
                CreditNoteId = attachment.CreditNoteId,
                InventoryStocktakingId = attachment.InventoryStocktakingId,
                ReservationId = attachment.ReservationId,
                TransactionId = attachment.TransactionId
            };
            
            return new OperationResponse<AttachmentDownloadDto>(ErrorCodes.Success, file);

        }
        catch (Exception e)
        {
            LogService.LogError(e);
        }
        return result;
    }

    public async Task<OperationResponse<List<AttachmentResponseDto>>> GetAllNoneReferencedAsync(AttachmentFiltersDto filters = null)
    {
        var result = new OperationResponse<List<AttachmentResponseDto>>(ErrorCodes.GeneralFail, null);
        try
        {
            var attachmentRepository = _repositoriesFactory.GetRepository<Attachment>();
            var attachmentQuery = await attachmentRepository.GetAll();
            if (attachmentQuery.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<List<AttachmentResponseDto>>(attachmentQuery.ErrorCode, null);

            if (attachmentQuery.Data is null)
                return new OperationResponse<List<AttachmentResponseDto>>(ErrorCodes.NoContent, null);

            var query = attachmentQuery.Data
                .Where(a => a.InventoryPermitId == null && a.PurchaseInvoiceId == null && a.SaleInvoiceId == null && a.PaymentReceiptId == null && a.PaycheckReceiptId == null);

            var attachments = await query.Select(a => new AttachmentResponseDto
            {
                Id = a.Id,
                FileName = a.FileName,
                FileType = a.FileType,
                UploadedAt = a.UploadedAt,
                ReferenceNumber = a.ReferenceNumber
            }).ToListAsync();

            return new OperationResponse<List<AttachmentResponseDto>>(ErrorCodes.Success, attachments);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    public async Task<OperationResponse<AttachmentResponseDto>> UploadAsync(AttachmentUploadDto upload)
    {
        var result = new OperationResponse<AttachmentResponseDto>(ErrorCodes.GeneralFail, null);
        try
        {
            if (upload?.File == null || upload.File.Length == 0)
                return new OperationResponse<AttachmentResponseDto>(ErrorCodes.NoContent, null);

            using var memoryStream = new MemoryStream();
            await upload.File.CopyToAsync(memoryStream);
            byte[] fileData = memoryStream.ToArray();

            var attachment = new Attachment
            {
                FileName = upload.File.FileName,
                FileType = string.IsNullOrWhiteSpace(upload.File.ContentType) ? "application/octet-stream" : upload.File.ContentType,
                FileData = fileData,
                UploadedAt = DateTime.UtcNow,
                PaymentReceiptId = upload.PaymentReceiptId,
                PaycheckReceiptId = upload.PaycheckReceiptId
            };

            var attachmentRepository = _repositoriesFactory.GetRepository<Attachment>();
            var op = await attachmentRepository.Add(attachment);
            if (op.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<AttachmentResponseDto>(op.ErrorCode, null);

            return new OperationResponse<AttachmentResponseDto>(ErrorCodes.Success, new AttachmentResponseDto
            {
                Id = attachment.Id,
                FileName = attachment.FileName,
                FileType = attachment.FileType,
                UploadedAt = attachment.UploadedAt,
                ReferenceNumber = attachment.ReferenceNumber
            });
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    public async Task<OperationResponse<AttachmentFileStreamDto>> GetFileStreamAsync(Guid id)
    {
        var result = new OperationResponse<AttachmentFileStreamDto>(ErrorCodes.GeneralFail, null);
        try
        {
            var attachmentRepository = _repositoriesFactory.GetRepository<Attachment>();
            var attachmentQuery = await attachmentRepository.Filter(a => a.Id == id);
            if (attachmentQuery.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<AttachmentFileStreamDto>(attachmentQuery.ErrorCode, null);

            var attachment = attachmentQuery.Data.FirstOrDefault();
            if (attachment is null || attachment.FileData == null)
                return new OperationResponse<AttachmentFileStreamDto>(ErrorCodes.NotFound, null);

            var stream = new MemoryStream(attachment.FileData);
            var fileDto = new AttachmentFileStreamDto
            {
                Stream = stream,
                FileName = attachment.FileName,
                FileType = string.IsNullOrWhiteSpace(attachment.FileType) ? "application/octet-stream" : attachment.FileType,
                FileSize = attachment.FileData.Length
            };

            return new OperationResponse<AttachmentFileStreamDto>(ErrorCodes.Success, fileDto);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
}

