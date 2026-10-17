using SmartAndPro.ERP.Application.Common.Dtos;
using SmartAndPro.ERP.Application.Common.Dtos.HR.Attendance;
using SmartAndPro.ERP.Application.Common.Dtos.HR.Employee;
using SmartAndPro.ERP.Application.Common.Dtos.HR.EmploymentType;
using SmartAndPro.ERP.Application.Common.Dtos.HR.Shift;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.HR;
using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace SmartAndPro.ERP.Application.Services.HR;

public class EmployeeService 
    : ServiceBase
    <
        Employee,
        EmployeePostDto,
        EmployeePutDto,
        EmployeeResponseDto,
        ParamFilter
    > 

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    
    public EmployeeService(RepositoriesFactory repositoriesFactory, AppSettings appSettings , IMapper mapper) 
        : base(repositoriesFactory, appSettings,mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }

    public override async Task<OperationResponse<EmployeeResponseDto>> GetAsync(Guid id)
    {
        var employeeQuery = await _repositoriesFactory.GetRepository<Employee>().Filter(x => x.Id == id);
        var employee = employeeQuery.Data
            .IgnoreAutoIncludes()
            .Include(x => x.EmploymentType)
            .Include(x => x.Shift)
            .Include(x => x.AttendanceRecords)
            .Select(x => new EmployeeResponseDto
            {
                Id = x.Id,
                ReferenceNumber = x.ReferenceNumber,
                Name = x.Name,
                Phone = x.Phone,
                EmploymentTypeId = x.EmploymentTypeId,
                EmploymentType = new EmploymentTypeResponseDto
                {
                    Id = x.EmploymentTypeId,
                    Code = x.EmploymentType.Code,
                    Description = x.EmploymentType.Description,
                },
                ShiftId = x.ShiftId,
                Shift = new ShiftResponseDto
                {
                    Id = x.ShiftId,
                    ReferenceNumber = x.Shift.ReferenceNumber,
                    Name = x.Shift.Name,
                    StartTime = x.Shift.StartTime,
                    EndTime = x.Shift.EndTime,
                    GracePeriodInMinutes = x.Shift.GracePeriodInMinutes,
                },
                StartDate = x.StartDate,
                EndDate = x.EndDate,
                DeviceId = x.DeviceId,
                AttendanceRecords = x.AttendanceRecords.Select(a => new AttendanceResponseDto
                {
                    Id = a.Id,
                    EmployeeId = a.EmployeeId,
                    AttendanceDate = a.AttendanceDate,
                    AttendanceFlag = a.AttendanceFlag.ToString(),
                }).ToList(),
                MonthlyRate = x.MonthlyRate,
                SupplementaryRate = x.SupplementaryRate,
                FactoryType = x.FactoryType != null ? x.FactoryType.ToString() : null,
                MonthlyInsurance = x.MonthlyInsurance,
                LeaveBalance = x.LeaveBalance,
                RemainingLeaveBalance = x.LeaveBalance - x.AttendanceRecords.Count(a => a.AttendanceFlag == AttendanceFlag.OnLeaveWithPermission),
                OvertimeRateMultiplier = x.OvertimeRateMultiplier,
                AbsentRateMultiplier = x.AbsentRateMultiplier,
                LateRateMultiplier = x.LateRateMultiplier,
                IsActive = x.IsActive,
                CreatedAt = x.CreatedAt,
            }).FirstOrDefault();
        if (employee == null)
        {
            return new OperationResponse<EmployeeResponseDto>(ErrorCodes.NoContent, null);
        }
        return new OperationResponse<EmployeeResponseDto>(ErrorCodes.Success, employee);
    }

    public override async Task<OperationResponse<EmployeeResponseDto>> AddAsync(EmployeePostDto request)
    {
        if (string.IsNullOrWhiteSpace(request.DeviceId))
        {
            return new OperationResponse<EmployeeResponseDto>(
                ErrorCodes.BadRequest,
                null,
                message: "كود الموظف مطلوب ولا يمكن تركه فارغاً.");
        }

        var trimmedCode = request.DeviceId.Trim();
        FactoryType targetFactory = FactoryType.Factory1;
        if (!string.IsNullOrWhiteSpace(request.FactoryType) && Enum.TryParse<FactoryType>(request.FactoryType, out var parsed))
        {
            targetFactory = parsed;
        }

        var repo = _repositoriesFactory.GetRepository<Employee>();
        var existsQuery = await repo.Filter(x =>
            x.DeviceId != null &&
            x.DeviceId.Trim() == trimmedCode &&
            (x.FactoryType == targetFactory || (targetFactory == FactoryType.Factory1 && x.FactoryType == null)));

        var existing = existsQuery.Data.FirstOrDefault();
        if (existing != null)
        {
            var factoryLabel = targetFactory == FactoryType.Factory1 ? "المصنع الأول" : targetFactory == FactoryType.Factory2 ? "المصنع الثاني" : "المصنع الثالث";
            return new OperationResponse<EmployeeResponseDto>(
                ErrorCodes.BadRequest,
                null,
                message: $"كود الموظف ({trimmedCode}) مسجل بالفعل في {factoryLabel} للموظف: {existing.Name}");
        }

        var result = await base.AddAsync(request);
        if (result.ErrorCode == ErrorCodes.Success)
        {
            try
            {
                await AttendanceService.EnsureAttendanceRecordsUpToDateAsync(_repositoriesFactory);
            }
            catch { }
        }
        return result;
    }

    public override async Task<OperationResponse<EmployeeResponseDto>> UpdateAsync(EmployeePutDto request)
    {
        if (!request.Id.HasValue || request.Id.Value == Guid.Empty)
        {
            return new OperationResponse<EmployeeResponseDto>(
                ErrorCodes.BadRequest,
                null,
                message: "معرف الموظف مطلوب.");
        }

        if (string.IsNullOrWhiteSpace(request.DeviceId))
        {
            return new OperationResponse<EmployeeResponseDto>(
                ErrorCodes.BadRequest,
                null,
                message: "كود الموظف مطلوب ولا يمكن تركه فارغاً.");
        }

        var trimmedCode = request.DeviceId.Trim();
        FactoryType targetFactory = FactoryType.Factory1;
        if (!string.IsNullOrWhiteSpace(request.FactoryType) && Enum.TryParse<FactoryType>(request.FactoryType, out var parsed))
        {
            targetFactory = parsed;
        }

        var repo = _repositoriesFactory.GetRepository<Employee>();
        var existsQuery = await repo.Filter(x =>
            x.Id != request.Id.Value &&
            x.DeviceId != null &&
            x.DeviceId.Trim() == trimmedCode &&
            (x.FactoryType == targetFactory || (targetFactory == FactoryType.Factory1 && x.FactoryType == null)));

        var existingWithCode = existsQuery.Data.FirstOrDefault();
        if (existingWithCode != null)
        {
            var factoryLabel = targetFactory == FactoryType.Factory1 ? "المصنع الأول" : targetFactory == FactoryType.Factory2 ? "المصنع الثاني" : "المصنع الثالث";
            return new OperationResponse<EmployeeResponseDto>(
                ErrorCodes.BadRequest,
                null,
                message: $"كود الموظف ({trimmedCode}) مسجل بالفعل في {factoryLabel} للموظف: {existingWithCode.Name}");
        }

        var empQuery = await repo.Filter(x => x.Id == request.Id.Value);
        var existingEmployee = await empQuery.Data
            .IgnoreAutoIncludes()
            .FirstOrDefaultAsync();

        if (existingEmployee == null)
        {
            return new OperationResponse<EmployeeResponseDto>(ErrorCodes.NotFound, null, message: "الموظف غير موجود");
        }

        existingEmployee.Name = request.Name;
        existingEmployee.Phone = request.Phone;
        existingEmployee.EmploymentTypeId = request.EmploymentTypeId;
        existingEmployee.EmploymentType = null!;
        existingEmployee.ShiftId = request.ShiftId;
        existingEmployee.Shift = null!;
        existingEmployee.StartDate = request.StartDate;
        existingEmployee.EndDate = request.EndDate;
        existingEmployee.DeviceId = trimmedCode;
        existingEmployee.MonthlyInsurance = request.MonthlyInsurance;
        existingEmployee.LeaveBalance = request.LeaveBalance;
        existingEmployee.MonthlyRate = request.MonthlyRate;
        existingEmployee.SupplementaryRate = request.SupplementaryRate;
        existingEmployee.OvertimeRateMultiplier = request.OvertimeRateMultiplier ?? 1.5m;
        existingEmployee.AbsentRateMultiplier = request.AbsentRateMultiplier ?? 1.5m;
        existingEmployee.LateRateMultiplier = request.LateRateMultiplier ?? 1.5m;
        existingEmployee.FactoryType = targetFactory;
        existingEmployee.IsActive = true;

        var updateOp = await repo.Update(existingEmployee);
        if (updateOp.ErrorCode != ErrorCodes.Success)
        {
            return new OperationResponse<EmployeeResponseDto>(updateOp.ErrorCode, null, message: updateOp.Message);
        }

        try
        {
            await AttendanceService.EnsureAttendanceRecordsUpToDateAsync(_repositoriesFactory);
        }
        catch { }

        return await GetAsync(existingEmployee.Id);
    }
    
}