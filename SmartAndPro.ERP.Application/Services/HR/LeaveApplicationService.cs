using SmartAndPro.ERP.Application.Common.Dtos;
using SmartAndPro.ERP.Application.Common.Dtos.HR.LeaveApplication;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.HR;
using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace SmartAndPro.ERP.Application.Services.HR;

public class LeaveApplicationService : ServiceBase<
    LeaveApplication,
    LeaveApplicationPostDto,
    LeaveApplicationPutDto,
    LeaveApplicationResponseDto,
    ParamFilter>
{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly IMapper _mapper;

    public LeaveApplicationService(RepositoriesFactory repositoriesFactory, AppSettings appSettings, IMapper mapper)
        : base(repositoriesFactory, appSettings, mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _mapper = mapper;
    }

    public async Task<OperationResponse<List<LeaveApplicationResponseDto>>> GetPendingApplicationsAsync()
    {
        try
        {
            var repo = _repositoriesFactory.GetRepository<LeaveApplication>();
            var query = await repo.Filter(x => x.Status == LeaveStatus.Pending);
            if (query.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<List<LeaveApplicationResponseDto>>(query.ErrorCode, null);

            var applications = await query.Data
                .Include(x => x.Employee)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            var response = _mapper.Map<List<LeaveApplicationResponseDto>>(applications);
            return new OperationResponse<List<LeaveApplicationResponseDto>>(ErrorCodes.Success, response);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return new OperationResponse<List<LeaveApplicationResponseDto>>(ErrorCodes.GeneralFail, null);
        }
    }

    public async Task<OperationResponse<List<LeaveApplicationResponseDto>>> GetByEmployeeAsync(Guid employeeId)
    {
        try
        {
            var repo = _repositoriesFactory.GetRepository<LeaveApplication>();
            var query = await repo.Filter(x => x.EmployeeId == employeeId);
            if (query.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<List<LeaveApplicationResponseDto>>(query.ErrorCode, null);

            var applications = await query.Data
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();

            var response = _mapper.Map<List<LeaveApplicationResponseDto>>(applications);
            return new OperationResponse<List<LeaveApplicationResponseDto>>(ErrorCodes.Success, response);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return new OperationResponse<List<LeaveApplicationResponseDto>>(ErrorCodes.GeneralFail, null);
        }
    }

    public async Task<OperationResponse<LeaveApplicationResponseDto>> ApproveAsync(Guid id, Guid approverId)
    {
        try
        {
            var repo = _repositoriesFactory.GetRepository<LeaveApplication>();
            var query = await repo.Filter(x => x.Id == id);
            var application = await query.Data.FirstOrDefaultAsync();

            if (application == null)
                return new OperationResponse<LeaveApplicationResponseDto>(ErrorCodes.NotFound, null);

            application.Status = LeaveStatus.Approved;
            application.ApproverId = approverId;
            application.ApprovedAt = DateTime.Now;

            var updateResult = await repo.Update(application);
            if (updateResult.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<LeaveApplicationResponseDto>(updateResult.ErrorCode, null);

            var response = _mapper.Map<LeaveApplicationResponseDto>(updateResult.Data);
            return new OperationResponse<LeaveApplicationResponseDto>(ErrorCodes.Success, response);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return new OperationResponse<LeaveApplicationResponseDto>(ErrorCodes.GeneralFail, null);
        }
    }

    public async Task<OperationResponse<LeaveApplicationResponseDto>> RejectAsync(Guid id, Guid approverId, string? rejectionReason)
    {
        try
        {
            var repo = _repositoriesFactory.GetRepository<LeaveApplication>();
            var query = await repo.Filter(x => x.Id == id);
            var application = await query.Data.FirstOrDefaultAsync();

            if (application == null)
                return new OperationResponse<LeaveApplicationResponseDto>(ErrorCodes.NotFound, null);

            application.Status = LeaveStatus.Rejected;
            application.ApproverId = approverId;
            application.ApprovedAt = DateTime.Now;
            application.RejectionReason = rejectionReason;

            var updateResult = await repo.Update(application);
            if (updateResult.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<LeaveApplicationResponseDto>(updateResult.ErrorCode, null);

            var response = _mapper.Map<LeaveApplicationResponseDto>(updateResult.Data);
            return new OperationResponse<LeaveApplicationResponseDto>(ErrorCodes.Success, response);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return new OperationResponse<LeaveApplicationResponseDto>(ErrorCodes.GeneralFail, null);
        }
    }
}
