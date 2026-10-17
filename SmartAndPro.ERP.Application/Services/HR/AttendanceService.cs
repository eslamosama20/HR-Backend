using SmartAndPro.ERP.Application.Common.Dtos;
using SmartAndPro.ERP.Application.Common.Dtos.HR.Attendance;
using SmartAndPro.ERP.Application.Common.Dtos.HR.Employee;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Entities.HR;
using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using SmartAndPro.ERP.SharedKernel.Entities.Contracts;
using SmartAndPro.ERP.SharedKernel.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;
using AutoMapper;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace SmartAndPro.ERP.Application.Services.HR;

public class AttendanceService 
    : ServiceBase
    <
        Attendance,
        AttendancePostDto,
        AttendancePutDto,
        AttendanceResponseDto,
        ParamFilter
    > 

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    
    public AttendanceService(RepositoriesFactory repositoriesFactory, AppSettings appSettings , IMapper mapper) 
        : base(repositoriesFactory, appSettings,mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }
    
    public override async Task<OperationResponse<List<AttendanceResponseDto>>> GetAllAsync(ParamFilter filters, int? pageNumber = null, int? pageSize = null, string? sortField = null, int? sortOrder = null)
    {
        var result = new OperationResponse<List<AttendanceResponseDto>>(ErrorCodes.GeneralFail,
            null);
        try
        {
            await EnsureAttendanceRecordsUpToDateAsync(_repositoriesFactory);

            var operation = await _repositoriesFactory.GetRepository<Attendance>().GetAll();
            if (operation.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<List<AttendanceResponseDto>>(operation.ErrorCode, null);

            var query = operation.Data.AsQueryable();
            // Apply Filters
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
            // Apply Sorting
            if (!string.IsNullOrEmpty(filters.SortField))
            {
                query = query.OrderByDynamic(filters.SortField, filters.SortOrder == "1");
            }
            else
            {
                query = query.OrderByDescending(x => x.AttendanceDate);
            }
            
            var totalRecords = await query.CountAsync();
            // Apply Pagination
            if (filters.PageNumber.HasValue && filters.PageSize.HasValue)
            {
                var skip = (filters.PageNumber.Value -1) * filters.PageSize.Value;
                query = query.Skip(skip).Take(filters.PageSize.Value);
            }

            var operationData = await query
                .Select(x => new AttendanceResponseDto
                {
                    Id = x.Id,
                    EmployeeId = x.EmployeeId,
                    Employee = new EmployeeResponseDto
                    {
                        Id = x.EmployeeId,
                        ReferenceNumber = x.Employee.ReferenceNumber,
                        Name = x.Employee.Name,
                        DeviceId = x.Employee.DeviceId,
                        FactoryType = x.Employee.FactoryType.ToString(),
                    },
                    AttendanceDate = x.AttendanceDate,
                    AttendanceFlag = x.AttendanceFlag.ToString(),
                    Reason = x.Reason,
                    CreatedAt = x.CreatedAt,
                })
                .ToListAsync();

            
            
            if (!operationData.Any())
                return new OperationResponse<List<AttendanceResponseDto>>(ErrorCodes.NoContent, null);

            
            if (filters.PageSize.HasValue && filters.PageNumber.HasValue)
            {
                return new OperationResponse<List<AttendanceResponseDto>>(
                    ErrorCodes.Success,
                    operationData,
                    totalCount: totalRecords,
                    isPaginated: true
                );
            }
            return new OperationResponse<List<AttendanceResponseDto>>(ErrorCodes.Success, operationData);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    public async Task MarkAbsentEmployeesAsync()
    {
        await EnsureAttendanceRecordsUpToDateAsync(_repositoriesFactory);
    }

    public static async Task EnsureAttendanceRecordsUpToDateAsync(
        RepositoriesFactory repositoriesFactory,
        DateOnly? fromDate = null,
        DateOnly? toDate = null)
    {
        try
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var end = toDate.HasValue ? (toDate.Value > today ? today : toDate.Value) : today;
            var start = fromDate ?? new DateOnly(end.Year, end.Month, 1);

            if (start > end) return;

            var empRepo = repositoriesFactory.GetRepository<Employee>();
            var empQuery = await empRepo.GetAll();
            var employees = await empQuery.Data
                .Where(e => e.IsActive != false)
                .Include(e => e.EmploymentType)
                .Include(e => e.Shift)
                .ToListAsync();

            if (!employees.Any()) return;

            var attRepo = repositoriesFactory.GetRepository<Attendance>();
            var attQuery = await attRepo.Filter(a => a.AttendanceDate >= start && a.AttendanceDate <= end);
            var existingAttendances = await attQuery.Data.ToListAsync();
            var existingDict = existingAttendances
                .GroupBy(a => (a.EmployeeId, a.AttendanceDate))
                .ToDictionary(g => g.Key, g => g.First());

            var checkInRepo = repositoriesFactory.GetRepository<CheckInLog>();
            var startDateTime = start.ToDateTime(TimeOnly.MinValue);
            var endDateTime = end.ToDateTime(TimeOnly.MaxValue);
            var logsQuery = await checkInRepo.Filter(c => c.Time >= startDateTime && c.Time <= endDateTime);
            var logs = await logsQuery.Data.ToListAsync();
            var logLookup = logs
                .GroupBy(c => (c.EmployeeId, DateOnly.FromDateTime(c.Time)))
                .ToDictionary(g => g.Key, g => g.OrderBy(x => x.Time).ToList());

            var missingAttendances = new List<Attendance>();
            var updateAttendances = new List<Attendance>();

            foreach (var emp in employees)
            {
                var empStart = start;
                var empEnd = (emp.EndDate.HasValue && emp.EndDate.Value < end) ? emp.EndDate.Value : end;

                if (empStart > empEnd) continue;

                var weeklyDayOffs = emp.EmploymentType?.WeeklyDayOff ?? new List<DayOfWeek> { DayOfWeek.Friday };

                for (var curDate = empStart; curDate <= empEnd; curDate = curDate.AddDays(1))
                {
                    bool isWeeklyOff = weeklyDayOffs.Contains(curDate.DayOfWeek);

                    if (existingDict.TryGetValue((emp.Id, curDate), out var existing))
                    {
                        if (!isWeeklyOff && logLookup.TryGetValue((emp.Id, curDate), out var activeLogs) && activeLogs.Any())
                        {
                            var inLog = activeLogs.FirstOrDefault(l => l.LogType == LogType.IN) ?? activeLogs.First();
                            var shiftStart = emp.Shift?.StartTime ?? new TimeOnly(9, 0);
                            var grace = emp.Shift?.GracePeriodInMinutes ?? 0;
                            var shiftStartDateTime = curDate.ToDateTime(shiftStart);
                            var diffMinutes = (inLog.Time - shiftStartDateTime).TotalMinutes;
                            var flag = diffMinutes <= grace ? AttendanceFlag.Present : AttendanceFlag.LateEntry;
                            var expectedReason = flag == AttendanceFlag.Present ? "حضور مسجل" : "تأخير مسجل";

                            if (existing.AttendanceFlag != flag || existing.Reason != expectedReason)
                            {
                                existing.AttendanceFlag = flag;
                                existing.Reason = expectedReason;
                                updateAttendances.Add(existing);
                            }
                        }
                        continue;
                    }

                    if (isWeeklyOff)
                    {
                        missingAttendances.Add(new Attendance
                        {
                            Id = Guid.NewGuid(),
                            EmployeeId = emp.Id,
                            AttendanceDate = curDate,
                            AttendanceFlag = AttendanceFlag.Weekend,
                            Reason = "عطلة أسبوعية",
                            CreatedAt = DateTime.UtcNow,
                            IsActive = true
                        });
                    }
                    else
                    {
                        if (logLookup.TryGetValue((emp.Id, curDate), out var dayLogs) && dayLogs.Any())
                        {
                            var inLog = dayLogs.FirstOrDefault(l => l.LogType == LogType.IN) ?? dayLogs.First();
                            var shiftStart = emp.Shift?.StartTime ?? new TimeOnly(9, 0);
                            var grace = emp.Shift?.GracePeriodInMinutes ?? 0;
                            var shiftStartDateTime = curDate.ToDateTime(shiftStart);
                            var diffMinutes = (inLog.Time - shiftStartDateTime).TotalMinutes;
                            var flag = diffMinutes <= grace ? AttendanceFlag.Present : AttendanceFlag.LateEntry;

                            missingAttendances.Add(new Attendance
                            {
                                Id = Guid.NewGuid(),
                                EmployeeId = emp.Id,
                                AttendanceDate = curDate,
                                AttendanceFlag = flag,
                                Reason = flag == AttendanceFlag.Present ? "حضور مسجل" : "تأخير مسجل",
                                CreatedAt = DateTime.UtcNow,
                                IsActive = true
                            });
                        }
                        else
                        {
                            missingAttendances.Add(new Attendance
                            {
                                Id = Guid.NewGuid(),
                                EmployeeId = emp.Id,
                                AttendanceDate = curDate,
                                AttendanceFlag = AttendanceFlag.Absent,
                                Reason = "غياب تلقائي (عدم تسجيل بصمة)",
                                CreatedAt = DateTime.UtcNow,
                                IsActive = true
                            });
                        }
                    }
                }
            }

            if (updateAttendances.Any())
            {
                await attRepo.UpdateRange(updateAttendances);
            }

            if (missingAttendances.Any())
            {
                await attRepo.AddRange(missingAttendances);
            }
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
    }
    public async Task ScheduleDailyShiftJobsAsync()
    {
        var shiftsOp = await _repositoriesFactory.GetRepository<Shift>().GetAll();

        foreach (var shift in shiftsOp.Data)
        {
            var end = shift.EndTime.AddMinutes(shift.GracePeriodInMinutes ?? 0);
            var scheduledTime = DateTime.Today.Add(end.ToTimeSpan());

            if (scheduledTime > DateTime.Now)
            {
                BackgroundJob.Schedule(
                    () => MarkAbsentEmployeesAsync(),
                    scheduledTime - DateTime.Now
                );
            }
        }
    }

    public override async Task<OperationResponse<AttendanceResponseDto>> UpdateAsync(AttendancePutDto request)
    {
        try
        {
            if (!request.Id.HasValue || request.Id.Value == Guid.Empty)
            {
                return new OperationResponse<AttendanceResponseDto>(ErrorCodes.BadRequest, null, message: "معرف سجل الحضور مطلوب.");
            }

            var attRepo = _repositoriesFactory.GetRepository<Attendance>();
            var existingQuery = await attRepo.Filter(a => a.Id == request.Id.Value);
            var existing = await existingQuery.Data.FirstOrDefaultAsync();
            if (existing == null)
            {
                return new OperationResponse<AttendanceResponseDto>(ErrorCodes.NotFound, null, message: "سجل الحضور غير موجود.");
            }

            var oldFlag = existing.AttendanceFlag;
            var oldReason = existing.Reason ?? "";
            if (!Enum.TryParse<AttendanceFlag>(request.AttendanceFlag, out var newFlag))
            {
                return new OperationResponse<AttendanceResponseDto>(ErrorCodes.BadRequest, null, message: "نوع الحضور غير صالح.");
            }

            var empRepo = _repositoriesFactory.GetRepository<Employee>();
            var empQuery = await empRepo.Filter(e => e.Id == existing.EmployeeId);
            var employee = await empQuery.Data.FirstOrDefaultAsync();

            string returnMessage = "تم التعديل بنجاح";
            bool isUnpaidLeave = false;

            if (employee != null)
            {
                // Scenario 1: Changing TO OnLeaveWithPermission from any other status
                if (oldFlag != AttendanceFlag.OnLeaveWithPermission && newFlag == AttendanceFlag.OnLeaveWithPermission)
                {
                    if (employee.LeaveBalance > 0)
                    {
                        employee.LeaveBalance -= 1;
                        await empRepo.Update(employee);
                        returnMessage = $"تم تسجيل إجازة بإذن وخصم يوم من رصيد الإجازات. رصيد الموظف المتبقي: {employee.LeaveBalance} يوم.";
                    }
                    else
                    {
                        // Employee has no leave balance left (0)
                        isUnpaidLeave = true;
                        returnMessage = "تنبيه: لقد نفذ رصيد إجازات الموظف (0 يوم)، وسيتم احتساب الإجازة بدون أجر وخصم يوم واحد من المرتب.";
                    }
                }
                // Scenario 2: Changing FROM OnLeaveWithPermission to any other status (e.g. Absent or Present)
                else if (oldFlag == AttendanceFlag.OnLeaveWithPermission && newFlag != AttendanceFlag.OnLeaveWithPermission)
                {
                    if (oldReason.Contains("[بدون رصيد]"))
                    {
                        returnMessage = "تم تعديل الحالة بنجاح (كانت إجازة بدون رصيد ولم تكن مخصومة من رصيد الإجازات).";
                    }
                    else
                    {
                        employee.LeaveBalance += 1;
                        await empRepo.Update(employee);
                        returnMessage = $"تم تعديل الحالة واسترداد يوم إلى رصيد إجازات الموظف. رصيد الإجازات الحالي: {employee.LeaveBalance} يوم.";
                    }
                }
                // Scenario 3: Remaining OnLeaveWithPermission
                else if (oldFlag == AttendanceFlag.OnLeaveWithPermission && newFlag == AttendanceFlag.OnLeaveWithPermission)
                {
                    if (oldReason.Contains("[بدون رصيد]"))
                    {
                        isUnpaidLeave = true;
                    }
                }
            }

            existing.AttendanceFlag = newFlag;
            existing.AttendanceDate = request.AttendanceDate;
            
            var cleanReason = (request.Reason ?? "").Replace("[بدون رصيد]", "").Trim();
            if (isUnpaidLeave)
            {
                existing.Reason = string.IsNullOrEmpty(cleanReason) 
                    ? "إجازة بإذن [بدون رصيد]" 
                    : $"{cleanReason} [بدون رصيد]";
            }
            else
            {
                existing.Reason = string.IsNullOrEmpty(cleanReason) ? null : cleanReason;
            }
            existing.Employee = null;

            var updateOp = await attRepo.Update(existing);
            if (updateOp.ErrorCode != ErrorCodes.Success)
            {
                return new OperationResponse<AttendanceResponseDto>(updateOp.ErrorCode, null, message: updateOp.Message);
            }

            var responseDto = _mapper.Map<AttendanceResponseDto>(updateOp.Data);
            return new OperationResponse<AttendanceResponseDto>(ErrorCodes.Success, responseDto, message: returnMessage);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return new OperationResponse<AttendanceResponseDto>(ErrorCodes.GeneralFail, null, message: ex.Message);
        }
    }

    public override async Task<OperationResponse<AttendanceResponseDto>> AddAsync(AttendancePostDto request)
    {
        try
        {
            if (Enum.TryParse<AttendanceFlag>(request.AttendanceFlag, out var newFlag) && newFlag == AttendanceFlag.OnLeaveWithPermission)
            {
                var empRepo = _repositoriesFactory.GetRepository<Employee>();
                var empQuery = await empRepo.Filter(e => e.Id == request.EmployeeId);
                var employee = await empQuery.Data.FirstOrDefaultAsync();
                string returnMessage = "تم إنشاء سجل الحضور بنجاح";
                bool isUnpaidLeave = false;

                if (employee != null)
                {
                    if (employee.LeaveBalance > 0)
                    {
                        employee.LeaveBalance -= 1;
                        await empRepo.Update(employee);
                        returnMessage = $"تم تسجيل إجازة بإذن وخصم يوم من رصيد الإجازات. رصيد الموظف المتبقي: {employee.LeaveBalance} يوم.";
                    }
                    else
                    {
                        isUnpaidLeave = true;
                        returnMessage = "تنبيه: لقد نفذ رصيد إجازات الموظف (0 يوم)، وسيتم احتساب الإجازة بدون أجر وخصم يوم واحد من المرتب.";
                    }
                }

                var cleanReason = (request.Reason ?? "").Replace("[بدون رصيد]", "").Trim();
                if (isUnpaidLeave)
                {
                    request.Reason = string.IsNullOrEmpty(cleanReason)
                        ? "إجازة بإذن [بدون رصيد]"
                        : $"{cleanReason} [بدون رصيد]";
                }
                else
                {
                    request.Reason = string.IsNullOrEmpty(cleanReason) ? null : cleanReason;
                }

                var entity = _mapper.Map<Attendance>(request);
                var attRepo = _repositoriesFactory.GetRepository<Attendance>();
                var operation = await attRepo.Add(entity);
                if (operation.ErrorCode != ErrorCodes.Success)
                {
                    return new OperationResponse<AttendanceResponseDto>(operation.ErrorCode, null, message: operation.Message);
                }

                var responseData = _mapper.Map<AttendanceResponseDto>(operation.Data);
                return new OperationResponse<AttendanceResponseDto>(ErrorCodes.Success, responseData, message: returnMessage);
            }

            return await base.AddAsync(request);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return new OperationResponse<AttendanceResponseDto>(ErrorCodes.GeneralFail, null, message: ex.Message);
        }
    }

    public override async Task<OperationResponse<AttendanceResponseDto>> DeleteAsync(Guid id)
    {
        try
        {
            var attRepo = _repositoriesFactory.GetRepository<Attendance>();
            var existingQuery = await attRepo.Filter(a => a.Id == id);
            var existing = await existingQuery.Data.FirstOrDefaultAsync();
            if (existing != null)
            {
                if (existing.AttendanceFlag == AttendanceFlag.OnLeaveWithPermission)
                {
                    if (existing.Reason == null || !existing.Reason.Contains("[بدون رصيد]"))
                    {
                        var empRepo = _repositoriesFactory.GetRepository<Employee>();
                        var empQuery = await empRepo.Filter(e => e.Id == existing.EmployeeId);
                        var employee = await empQuery.Data.FirstOrDefaultAsync();
                        if (employee != null)
                        {
                            employee.LeaveBalance += 1;
                            await empRepo.Update(employee);
                        }
                    }
                }
            }

            return await base.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return new OperationResponse<AttendanceResponseDto>(ErrorCodes.GeneralFail, null, message: ex.Message);
        }
    }
}