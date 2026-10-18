using SmartAndPro.ERP.Application.Common.Dtos;
using SmartAndPro.ERP.Application.Common.Dtos.HR.Attendance;
using SmartAndPro.ERP.Application.Common.Dtos.HR.Employee;
using SmartAndPro.ERP.Application.Common.Dtos.HR.EmploymentType;
using SmartAndPro.ERP.Application.Common.Dtos.HR.Shift;
using SmartAndPro.ERP.Application.Common.Extensions;
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

    public override async Task<OperationResponse<List<EmployeeResponseDto>>> GetAllAsync(
        ParamFilter filters, int? pageNumber = null, int? pageSize = null, string? sortField = null, int? sortOrder = null)
    {
        try
        {
            var repo = _repositoriesFactory.GetRepository<Employee>();
            var operation = await repo.GetAll();
            if (operation.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<List<EmployeeResponseDto>>(operation.ErrorCode, null);

            var query = operation.Data
                .Include(x => x.EmploymentType)
                .Include(x => x.Shift)
                .AsQueryable();

            if (filters.Filters != null && filters.Filters.Count > 0)
            {
                foreach (var filter in filters.Filters)
                {
                    query = query.WhereDynamic(filter.Key, filter.Value);
                }
            }
            if (!string.IsNullOrEmpty(filters.Search))
            {
                query = query.ApplyGlobalSearch(filters.Search);
            }

            var totalRecords = await query.CountAsync();

            if (pageNumber.HasValue || pageSize.HasValue)
            {
                pageNumber ??= 1;
                pageSize ??= 50;
                if (pageSize > 1000) pageSize = 1000;
                if (pageSize < 1) pageSize = 1;

                var skip = (pageNumber.Value - 1) * pageSize.Value;
                query = query.ApplySort(sortField, sortOrder).Skip(skip).Take(pageSize.Value);
            }
            else
            {
                query = query.ApplySort(sortField, sortOrder);
            }

            var entities = await query.ToListAsync();
            var responseData = _mapper.Map<List<EmployeeResponseDto>>(entities);

            if (pageNumber.HasValue && pageSize.HasValue)
            {
                return new OperationResponse<List<EmployeeResponseDto>>(
                    ErrorCodes.Success,
                    responseData,
                    totalCount: totalRecords,
                    isPaginated: true
                );
            }

            return new OperationResponse<List<EmployeeResponseDto>>(ErrorCodes.Success, responseData);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return new OperationResponse<List<EmployeeResponseDto>>(ErrorCodes.GeneralFail, null);
        }
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
                Department = x.Department,
                Workshop = x.Workshop,
                JobTitle = x.JobTitle,
                NationalId = x.NationalId,
                Address = x.Address,
                Governorate = x.Governorate,
                BirthDate = x.BirthDate,
                InsuranceNumber = x.InsuranceNumber,
                InsuranceTitle = x.InsuranceTitle,
                LivingAllowance = x.LivingAllowance,
                MealAllowance = x.MealAllowance,
                TransportAllowance = x.TransportAllowance,
                HousingAllowance = x.HousingAllowance,
                ExtraIncrease = x.ExtraIncrease,
                SupervisionBonus = x.SupervisionBonus,
                MasteryBonus = x.MasteryBonus,
                RegularityBonus = x.RegularityBonus,
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
        existingEmployee.Department = request.Department;
        existingEmployee.Workshop = request.Workshop;
        existingEmployee.JobTitle = request.JobTitle;
        existingEmployee.NationalId = request.NationalId;
        existingEmployee.Address = request.Address;
        existingEmployee.Governorate = request.Governorate;
        existingEmployee.BirthDate = request.BirthDate;
        existingEmployee.InsuranceNumber = request.InsuranceNumber;
        existingEmployee.InsuranceTitle = request.InsuranceTitle;
        existingEmployee.LivingAllowance = request.LivingAllowance ?? 0;
        existingEmployee.MealAllowance = request.MealAllowance ?? 0;
        existingEmployee.TransportAllowance = request.TransportAllowance ?? 0;
        existingEmployee.HousingAllowance = request.HousingAllowance ?? 0;
        existingEmployee.ExtraIncrease = request.ExtraIncrease ?? 0;
        existingEmployee.SupervisionBonus = request.SupervisionBonus ?? 0;
        existingEmployee.MasteryBonus = request.MasteryBonus ?? 0;
        existingEmployee.RegularityBonus = request.RegularityBonus ?? 0;
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