using SmartAndPro.ERP.Application.Common.Dtos;
using SmartAndPro.ERP.Application.Common.Dtos.HR.CheckInLog;
using SmartAndPro.ERP.Application.Common.Dtos.HR.Employee;
using SmartAndPro.ERP.Application.Services.Auth;
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

public class CheckInLogService 
    : ServiceBase
    <
        CheckInLog,
        CheckInLogPostDto,
        CheckInLogPutDto,
        CheckInLogResponseDto,
        ParamFilter
    > 

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    
    public CheckInLogService(RepositoriesFactory repositoriesFactory, AppSettings appSettings , IMapper mapper)
        : base(repositoriesFactory, appSettings,mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }
    
    public override async Task<OperationResponse<List<CheckInLogResponseDto>>> GetAllAsync(ParamFilter filters, int? pageNumber = null, int? pageSize = null, string? sortField = null, int? sortOrder = null)
    {
        var result = new OperationResponse<List<CheckInLogResponseDto>>(ErrorCodes.GeneralFail,
            null);
        try
        {
            var operation = await _repositoriesFactory.GetRepository<CheckInLog>().GetAll();
            if (operation.ErrorCode != ErrorCodes.Success)
                return new OperationResponse<List<CheckInLogResponseDto>>(operation.ErrorCode, null);

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
                query = query.OrderByDescending(x => x.Time);
            }
            
            var totalRecords = await query.CountAsync();
            // Apply Pagination
            if (filters.PageNumber.HasValue && filters.PageSize.HasValue)
            {
                var skip = (filters.PageNumber.Value -1) * filters.PageSize.Value;
                query = query.Skip(skip).Take(filters.PageSize.Value);
            }

            var operationData = await query
                .Select(x => new CheckInLogResponseDto
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
                    Time = x.Time,
                    LogType = x.LogType.ToString(),
                    CreatedAt = x.CreatedAt,
                })
                .ToListAsync();

            
            
            if (!operationData.Any())
                return new OperationResponse<List<CheckInLogResponseDto>>(ErrorCodes.NoContent, null);

            
            if (filters.PageSize.HasValue && filters.PageNumber.HasValue)
            {
                return new OperationResponse<List<CheckInLogResponseDto>>(
                    ErrorCodes.Success,
                    operationData,
                    totalCount: totalRecords,
                    isPaginated: true
                );
            }
            return new OperationResponse<List<CheckInLogResponseDto>>(ErrorCodes.Success, operationData);
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }

    public override async Task<OperationResponse<CheckInLogResponseDto>> AddAsync(CheckInLogPostDto request)
    {
        var result = await base.AddAsync(request);
        if (result.ErrorCode != ErrorCodes.Success || result.Data == null)
            return result;

        var checkInLogQuery = await _repositoriesFactory.GetRepository<CheckInLog>()
            .Filter(c => c.Id == result.Data.Id);

        var checkInLog = checkInLogQuery.Data
            .IgnoreAutoIncludes()
            .Include(c => c.Employee)
            .ThenInclude(e => e.Shift)
            .FirstOrDefault();

        if (checkInLog == null)
            return result;

        var shift = checkInLog.Employee.Shift;
        var attendanceDate = DateOnly.FromDateTime(checkInLog.Time);

        // === Step 1: Adjust IN to OUT if conditions are met ===
        if (checkInLog.LogType == LogType.IN)
        {
            // Define thresholds (you can move these to config)
            const int NearStartThresholdMinutes = 120;
            const int NearEndThresholdMinutes = 120;

            var shiftStart = checkInLog.Time.Date + shift.StartTime.ToTimeSpan();
            var shiftEnd = checkInLog.Time.Date + shift.EndTime.ToTimeSpan();

            var existingInLogsQuery = await _repositoriesFactory.GetRepository<CheckInLog>()
                .Filter(l =>
                    l.EmployeeId == checkInLog.EmployeeId &&
                    l.Time.Date == checkInLog.Time.Date &&
                    l.LogType == LogType.IN &&
                    l.Id != checkInLog.Id); // Exclude the new log

            var existingInLogs = existingInLogsQuery.Data.ToList();

            bool hasEarlyIn = existingInLogs.Any(log =>
                Math.Abs((log.Time - shiftStart).TotalMinutes) <= NearStartThresholdMinutes);

            bool isLateIn = Math.Abs((checkInLog.Time - shiftEnd).TotalMinutes) <= NearEndThresholdMinutes;

            if (hasEarlyIn && isLateIn)
            {
                checkInLog.LogType = LogType.OUT;
                checkInLog.Employee = null;
                // Persist the change to database
                await _repositoriesFactory.GetRepository<CheckInLog>().Update(checkInLog);
            }
        }

        // === Step 2: Continue normal attendance logic ===
        var attendanceRepository = _repositoriesFactory.GetRepository<Attendance>();
        var attendanceQuery = await attendanceRepository
            .Filter(a => a.EmployeeId == checkInLog.EmployeeId && a.AttendanceDate == attendanceDate);

        var attendance = attendanceQuery.Data?
            .IgnoreAutoIncludes()
            .Include(a => a.Employee)
            .ThenInclude(e => e.Shift)
            .FirstOrDefault();

        if (checkInLog.LogType == LogType.IN)
        {
            if (attendance == null)
            {
                var newAttendance = new Attendance
                {
                    Id = Guid.NewGuid(),
                    EmployeeId = checkInLog.EmployeeId,
                    AttendanceDate = attendanceDate,
                    AttendanceFlag = DetermineEntryFlag(checkInLog.Time, shift.StartTime, shift.GracePeriodInMinutes)
                };
                await attendanceRepository.Add(newAttendance);
            }
            else
            {
                attendance.AttendanceFlag = DetermineEntryFlag(checkInLog.Time, shift.StartTime, shift.GracePeriodInMinutes);
                attendance.Employee = null;
                await attendanceRepository.Update(attendance);
            }
        }
        else if (checkInLog.LogType == LogType.OUT && attendance != null)
        {
            attendance.AttendanceFlag = DetermineExitFlag(checkInLog.Time, shift.EndTime, shift.GracePeriodInMinutes);
            attendance.Employee = null;
            await attendanceRepository.Update(attendance);
        }

        return result;
    }

    private AttendanceFlag DetermineEntryFlag(DateTime checkInTime, TimeOnly shiftStartTime, int? gracePeriodInMinutes)
    {
        int gracePeriod = gracePeriodInMinutes ?? 0;
        var shiftStart = checkInTime.Date + shiftStartTime.ToTimeSpan();
        var diffMinutes = (checkInTime - shiftStart).TotalMinutes;

        return diffMinutes <= gracePeriod ? AttendanceFlag.Present : AttendanceFlag.LateEntry;
    }

    private AttendanceFlag DetermineExitFlag(DateTime checkOutTime, TimeOnly shiftEndTime, int? gracePeriodInMinutes)
    {
        int gracePeriod = gracePeriodInMinutes ?? 0;
        var shiftEnd = checkOutTime.Date + shiftEndTime.ToTimeSpan();
        var diffMinutes = (shiftEnd - checkOutTime).TotalMinutes;

        return diffMinutes <= gracePeriod ? AttendanceFlag.Present : AttendanceFlag.EarlyExit;
    }

    public virtual async Task<OperationResponse<List<CheckInLogResponseDto>>> AddRangeAsync(List<CheckInLogPostDto> logDtos)
    {
        if (logDtos == null || logDtos.Count == 0)
            return new OperationResponse<List<CheckInLogResponseDto>>(ErrorCodes.GeneralFail, null);

        var checkInLogRepo = _repositoriesFactory.GetRepository<CheckInLog>();
        var attendanceRepo = _repositoriesFactory.GetRepository<Attendance>();
        var employeeIds = new HashSet<Guid>(logDtos.Select(l => l.EmployeeId));

        // Preload employees with shift info to avoid N+1
        var employees = await _repositoriesFactory.GetRepository<Employee>()
            .Filter(e => employeeIds.Contains(e.Id))
            .ContinueWith(t => t.Result.Data
                .Include(e => e.Shift)
                .ToDictionary(e => e.Id));

        // Preload existing logs for same employees and dates
        var logDates = new HashSet<DateTime>(logDtos.Select(l => l.Time.Date));
        var existingLogs = await checkInLogRepo
            .Filter(l => employeeIds.Contains(l.EmployeeId) && logDates.Contains(l.Time.Date))
            .ContinueWith(t => t.Result.Data.ToList());

        // Preload attendances for all employees and relevant dates
        var attendanceDates = new HashSet<DateOnly>(logDtos.Select(l => DateOnly.FromDateTime(l.Time)));
        var attendances = await attendanceRepo
            .Filter(a => employeeIds.Contains(a.EmployeeId) && attendanceDates.Contains(a.AttendanceDate))
            .ContinueWith(t => t.Result.Data.ToList());
        var attendanceDict = attendances.ToDictionary(a => (a.EmployeeId, a.AttendanceDate));
        var originalDbAttendanceIds = attendances.Select(a => a.Id).ToHashSet();

        // Track all known log timestamps (from DB + this batch) to skip duplicates safely
        var existingLogKeys = existingLogs
            .Select(x => (x.EmployeeId, x.Time))
            .ToHashSet();

        var newCheckInLogs = new List<CheckInLog>();
        var attendanceToInsert = new List<Attendance>();
        var attendanceToUpdate = new List<Attendance>();
        var attendanceUpdateIds = new HashSet<Guid>();
        // Group new logs by employee + date
        var logsGrouped = logDtos
            .GroupBy(l => new { l.EmployeeId, Date = l.Time.Date })
            .ToList();

        foreach (var group in logsGrouped)
        {
            var employeeId = group.Key.EmployeeId;
            var date = group.Key.Date;

            if (!employees.TryGetValue(employeeId, out var employee) || employee.Shift == null)
            {
                LogService.LogInformation($"Skipping check-in logs for employee {employeeId}: employee or shift not found.");
                continue;
            }
            var shift = employee.Shift;

            var shiftStart = date + shift.StartTime.ToTimeSpan();
            var shiftEnd = date + shift.EndTime.ToTimeSpan();
            var grace = shift.GracePeriodInMinutes ?? 0;

            // Existing logs for this employee on this date
            var logsOnDay = existingLogs
                .Where(x => x.EmployeeId == employeeId && x.Time.Date == date)
                .ToList();

            // Grouped logs from the request
            var orderedNewLogs = group.OrderBy(l => l.Time).ToList();

            foreach (var dto in orderedNewLogs)
            {
                var time = dto.Time;
                var logKey = (employeeId, time);

                // Skip duplicate punches (both historical and duplicates within same batch)
                if (existingLogKeys.Contains(logKey))
                {
                    continue;
                }
                existingLogKeys.Add(logKey);

                var logType = Enum.Parse<LogType>(dto.LogType);

                // Handle smart IN-to-OUT detection
                if (logType == LogType.IN)
                {
                    var hasEarlyIn = logsOnDay.Any(log =>
                        log.LogType == LogType.IN &&
                        Math.Abs((log.Time - shiftStart).TotalMinutes) <= 60);

                    var isLateIn = Math.Abs((time - shiftEnd).TotalMinutes) <= 60;

                    if (hasEarlyIn && isLateIn)
                        logType = LogType.OUT;
                }

                // Create new CheckInLog
                var checkInLog = new CheckInLog
                {
                    Id = Guid.NewGuid(),
                    EmployeeId = dto.EmployeeId,
                    Time = dto.Time,
                    LogType = logType
                };

                newCheckInLogs.Add(checkInLog);
                logsOnDay.Add(checkInLog); // Update cache for subsequent logic

                // Process Attendance (preloaded)
                var attendanceDate = DateOnly.FromDateTime(time);
                var attendanceKey = (employeeId, attendanceDate);
                attendanceDict.TryGetValue(attendanceKey, out var existingAttendance);

                if (logType == LogType.IN)
                {
                    if (existingAttendance == null)
                    {
                        var newAttendance = new Attendance
                        {
                            Id = Guid.NewGuid(),
                            EmployeeId = employeeId,
                            AttendanceDate = attendanceDate,
                            AttendanceFlag = DetermineEntryFlag(time, shift.StartTime, grace)
                        };
                        attendanceToInsert.Add(newAttendance);
                        attendanceDict[attendanceKey] = newAttendance; // So subsequent logs see it
                    }
                    else
                    {
                        existingAttendance.AttendanceFlag = DetermineEntryFlag(time, shift.StartTime, grace);
                        if (originalDbAttendanceIds.Contains(existingAttendance.Id) && attendanceUpdateIds.Add(existingAttendance.Id))
                        {
                            existingAttendance.Employee = null;
                            attendanceToUpdate.Add(existingAttendance);
                        }
                    }
                }
                else if (logType == LogType.OUT && existingAttendance != null)
                {
                    existingAttendance.AttendanceFlag = DetermineExitFlag(time, shift.EndTime, grace);
                    if (originalDbAttendanceIds.Contains(existingAttendance.Id) && attendanceUpdateIds.Add(existingAttendance.Id))
                    {
                        existingAttendance.Employee = null;
                        attendanceToUpdate.Add(existingAttendance);
                    }
                }
            }
        }

        // Bulk insert logs and attendances in manageable chunks
        const int batchSize = 500;

        // Helper method for batching
        static IEnumerable<List<T>> Batch<T>(List<T> source, int size)
        {
            for (int i = 0; i < source.Count; i += size)
                yield return source.GetRange(i, Math.Min(size, source.Count - i));
        }

        if (newCheckInLogs.Count > 0)
        {
            foreach (var batch in Batch(newCheckInLogs, batchSize))
            {
                var op = await checkInLogRepo.AddRange(batch);
                if (op.ErrorCode != ErrorCodes.Success)
                    LogService.LogError(new Exception($"CheckInLog AddRange returned error: {op.Message}"));
            }
        }

        if (attendanceToInsert.Count > 0)
        {
            foreach (var batch in Batch(attendanceToInsert, batchSize))
            {
                var op = await attendanceRepo.AddRange(batch);
                if (op.ErrorCode != ErrorCodes.Success)
                    LogService.LogError(new Exception($"Attendance AddRange returned error: {op.Message}"));
            }
        }

        if (attendanceToUpdate.Count > 0)
        {
            foreach (var batch in Batch(attendanceToUpdate, batchSize))
            {
                var op = await attendanceRepo.UpdateRange(batch);
                if (op.ErrorCode != ErrorCodes.Success)
                    LogService.LogError(new Exception($"Attendance UpdateRange returned error: {op.Message}"));
            }
        }

        var checkInLogsMapped = _mapper.Map<List<CheckInLogResponseDto>>(newCheckInLogs);
        return new OperationResponse<List<CheckInLogResponseDto>>(ErrorCodes.Success, checkInLogsMapped);
    }



    
}