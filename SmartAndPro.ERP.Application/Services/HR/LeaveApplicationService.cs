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

    public override async Task<OperationResponse<List<LeaveApplicationResponseDto>>> GetAllAsync(
        ParamFilter filters,
        int? pageNumber = null,
        int? pageSize = null,
        string? sortField = null,
        int? sortOrder = null)
    {
        try
        {
            var repo = _repositoriesFactory.GetRepository<LeaveApplication>();
            var operation = await repo.GetAll();
            if (operation.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<List<LeaveApplicationResponseDto>>(operation.ErrorCode, null);

            var query = operation.Data.Include(x => x.Employee).AsQueryable();

            if (!filters.AllPropertiesAreNull())
            {
                query = query.FilterQueryableData(filters);
            }

            var entities = await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
            var result = _mapper.Map<List<LeaveApplicationResponseDto>>(entities);
            return new OperationResponse<List<LeaveApplicationResponseDto>>(ErrorCodes.Success, result);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return new OperationResponse<List<LeaveApplicationResponseDto>>(ErrorCodes.GeneralFail, null);
        }
    }

    public override async Task<OperationResponse<LeaveApplicationResponseDto>> AddAsync(LeaveApplicationPostDto request)
    {
        try
        {
            var entity = _mapper.Map<LeaveApplication>(request);
            // Default new applications to Approved automatically so they take effect immediately
            entity.Status = LeaveStatus.Approved;
            entity.ApprovedAt = DateTime.Now;

            // Generate reference number if missing
            if (string.IsNullOrWhiteSpace(entity.ReferenceNumber))
            {
                var repoCount = await _repositoriesFactory.GetRepository<LeaveApplication>().GetAll();
                int count = repoCount.ErrorCode == ErrorCodes.Success ? await repoCount.Data.CountAsync() : 0;
                entity.ReferenceNumber = $"LA-{(count + 1):D5}";
            }

            // Calculate total days if not provided
            if (entity.TotalDays <= 0 && entity.StartDate <= entity.EndDate)
            {
                entity.TotalDays = (entity.EndDate.DayNumber - entity.StartDate.DayNumber) + 1;
            }

            var addOp = await _repositoriesFactory.GetRepository<LeaveApplication>().Add(entity);
            if (addOp.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<LeaveApplicationResponseDto>(addOp.ErrorCode, null, message: addOp.Message);

            // Sync attendance and leave balance immediately
            await ApplyLeaveToAttendanceAndBalanceAsync(addOp.Data);

            var empRepo = _repositoriesFactory.GetRepository<Employee>();
            var empQuery = await empRepo.Filter(e => e.Id == addOp.Data.EmployeeId);
            addOp.Data.Employee = await empQuery.Data.FirstOrDefaultAsync();

            var res = _mapper.Map<LeaveApplicationResponseDto>(addOp.Data);
            return new OperationResponse<LeaveApplicationResponseDto>(ErrorCodes.Success, res);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return new OperationResponse<LeaveApplicationResponseDto>(ErrorCodes.GeneralFail, null, message: ex.Message);
        }
    }

    public override async Task<OperationResponse<LeaveApplicationResponseDto>> UpdateAsync(LeaveApplicationPutDto request)
    {
        try
        {
            var repo = _repositoriesFactory.GetRepository<LeaveApplication>();
            var query = await repo.Filter(x => x.Id == request.Id);
            var existing = await query.Data.FirstOrDefaultAsync();
            if (existing == null)
                return new OperationResponse<LeaveApplicationResponseDto>(ErrorCodes.NotFound, null);

            _mapper.Map(request, existing);
            var updateOp = await repo.Update(existing);
            if (updateOp.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<LeaveApplicationResponseDto>(updateOp.ErrorCode, null, message: updateOp.Message);

            if (existing.Status == LeaveStatus.Approved)
            {
                await ApplyLeaveToAttendanceAndBalanceAsync(existing);
            }
            else if (existing.Status == LeaveStatus.Cancelled || existing.Status == LeaveStatus.Rejected)
            {
                await RevertLeaveAttendanceAsync(existing);
            }

            var empRepo = _repositoriesFactory.GetRepository<Employee>();
            var empQuery = await empRepo.Filter(e => e.Id == updateOp.Data.EmployeeId);
            updateOp.Data.Employee = await empQuery.Data.FirstOrDefaultAsync();

            var res = _mapper.Map<LeaveApplicationResponseDto>(updateOp.Data);
            return new OperationResponse<LeaveApplicationResponseDto>(ErrorCodes.Success, res);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return new OperationResponse<LeaveApplicationResponseDto>(ErrorCodes.GeneralFail, null, message: ex.Message);
        }
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

            await ApplyLeaveToAttendanceAndBalanceAsync(application);

            var empRepo = _repositoriesFactory.GetRepository<Employee>();
            var empQuery = await empRepo.Filter(e => e.Id == updateResult.Data.EmployeeId);
            updateResult.Data.Employee = await empQuery.Data.FirstOrDefaultAsync();

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

            await RevertLeaveAttendanceAsync(application);

            var response = _mapper.Map<LeaveApplicationResponseDto>(updateResult.Data);
            return new OperationResponse<LeaveApplicationResponseDto>(ErrorCodes.Success, response);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return new OperationResponse<LeaveApplicationResponseDto>(ErrorCodes.GeneralFail, null);
        }
    }

    /// <summary>
    /// Applies approved leave to the employee's attendance records and accounts for leave balance vs unpaid deductions.
    /// </summary>
    private async Task ApplyLeaveToAttendanceAndBalanceAsync(LeaveApplication leave)
    {
        if (leave == null || leave.StartDate > leave.EndDate) return;

        try
        {
            var empRepo = _repositoriesFactory.GetRepository<Employee>();
            var empQuery = await empRepo.Filter(e => e.Id == leave.EmployeeId);
            var employee = await empQuery.Data
                .Include(e => e.EmploymentType)
                .Include(e => e.AttendanceRecords)
                .FirstOrDefaultAsync();

            if (employee == null) return;

            var weeklyDayOffs = employee.EmploymentType?.WeeklyDayOff ?? new List<DayOfWeek> { DayOfWeek.Friday };

            // Calculate current used balance this year (excluding current leave dates)
            var yearStart = new DateOnly(leave.StartDate.Year, 1, 1);
            var yearEnd = new DateOnly(leave.StartDate.Year, 12, 31);

            int previouslyUsedPaidLeaves = employee.AttendanceRecords?
                .Count(a => a.AttendanceFlag == AttendanceFlag.OnLeaveWithPermission
                    && a.AttendanceDate >= yearStart && a.AttendanceDate <= yearEnd
                    && (a.AttendanceDate < leave.StartDate || a.AttendanceDate > leave.EndDate)
                    && (a.Reason == null || !a.Reason.Contains("[بدون رصيد]"))) ?? 0;

            int availableBalance = Math.Max(0, employee.LeaveBalance - previouslyUsedPaidLeaves);

            var attRepo = _repositoriesFactory.GetRepository<Attendance>();
            var existingAttQuery = await attRepo.Filter(a => a.EmployeeId == leave.EmployeeId && a.AttendanceDate >= leave.StartDate && a.AttendanceDate <= leave.EndDate);
            var existingAttendances = await existingAttQuery.Data.ToListAsync();
            var existingDict = existingAttendances.ToDictionary(a => a.AttendanceDate);

            var toAdd = new List<Attendance>();
            var toUpdate = new List<Attendance>();

            for (var cur = leave.StartDate; cur <= leave.EndDate; cur = cur.AddDays(1))
            {
                // Skip weekly day off (e.g. Friday), it's already a weekend
                if (weeklyDayOffs.Contains(cur.DayOfWeek)) continue;

                // Determine whether this leave day is covered by leave balance or unpaid
                bool isPaidLeave = false;
                if (leave.LeaveType == LeaveType.Unpaid)
                {
                    isPaidLeave = false;
                }
                else if (availableBalance > 0)
                {
                    isPaidLeave = true;
                    availableBalance--;
                }
                else
                {
                    isPaidLeave = false;
                }

                string reasonText = isPaidLeave
                    ? $"إجازة معتمدة (من رصيد الإجازات - {GetLeaveTypeName(leave.LeaveType)})"
                    : $"إجازة معتمدة [بدون رصيد] - خصم يوم بيوم ({GetLeaveTypeName(leave.LeaveType)})";

                if (existingDict.TryGetValue(cur, out var existing))
                {
                    existing.AttendanceFlag = AttendanceFlag.OnLeaveWithPermission;
                    existing.Reason = reasonText;
                    toUpdate.Add(existing);
                }
                else
                {
                    toAdd.Add(new Attendance
                    {
                        Id = Guid.NewGuid(),
                        EmployeeId = leave.EmployeeId,
                        AttendanceDate = cur,
                        AttendanceFlag = AttendanceFlag.OnLeaveWithPermission,
                        Reason = reasonText,
                        CreatedAt = DateTime.UtcNow,
                        IsActive = true
                    });
                }
            }

            if (toUpdate.Any())
            {
                await attRepo.UpdateRange(toUpdate);
            }
            if (toAdd.Any())
            {
                await attRepo.AddRange(toAdd);
            }
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
    }

    private async Task RevertLeaveAttendanceAsync(LeaveApplication leave)
    {
        if (leave == null) return;
        try
        {
            var attRepo = _repositoriesFactory.GetRepository<Attendance>();
            var existingAttQuery = await attRepo.Filter(a => a.EmployeeId == leave.EmployeeId
                && a.AttendanceDate >= leave.StartDate
                && a.AttendanceDate <= leave.EndDate
                && a.AttendanceFlag == AttendanceFlag.OnLeaveWithPermission);

            var list = await existingAttQuery.Data.ToListAsync();
            if (list.Any())
            {
                foreach (var item in list)
                {
                    item.AttendanceFlag = AttendanceFlag.Absent;
                    item.Reason = "تم إلغاء الإجازة";
                }
                await attRepo.UpdateRange(list);
            }
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
    }

    private static string GetLeaveTypeName(LeaveType type)
    {
        return type switch
        {
            LeaveType.Annual => "سنوية",
            LeaveType.Sick => "مرضية",
            LeaveType.Emergency => "طارئة",
            LeaveType.Unpaid => "بدون راتب",
            LeaveType.Maternity => "أمومة",
            LeaveType.Paternity => "أبوة",
            _ => "إجازة رسمية"
        };
    }
}
