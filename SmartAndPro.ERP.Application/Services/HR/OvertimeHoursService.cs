using SmartAndPro.ERP.Application.Common.Dtos;
using SmartAndPro.ERP.Application.Common.Dtos.HR.OvertimeHours;
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

public class OvertimeHoursService 
    : ServiceBase
    <
        OvertimeHours,
        OvertimeHoursPostDto,
        OvertimeHoursPutDto,
        OvertimeHoursResponseDto,
        ParamFilter
    > 

{
    private readonly RepositoriesFactory _repositoriesFactory;
    private readonly AppSettings _appSettings;
    private readonly IMapper _mapper;
    
    public OvertimeHoursService(RepositoriesFactory repositoriesFactory, AppSettings appSettings , IMapper mapper)
        : base(repositoriesFactory, appSettings,mapper)
    {
        _repositoriesFactory = repositoriesFactory;
        _appSettings = appSettings;
        _mapper = mapper;
    }

    public override async Task<OperationResponse<List<OvertimeHoursResponseDto>>> GetAllAsync(
        ParamFilter filters, int? pageNumber = null, int? pageSize = null, string? sortField = null, int? sortOrder = null)
    {
        try
        {
            await SyncFromCheckInLogsAsync();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return await base.GetAllAsync(filters, pageNumber, pageSize, sortField, sortOrder);
    }

    public async Task<int> SyncFromCheckInLogsAsync()
    {
        var checkInLogRepo = _repositoriesFactory.GetRepository<CheckInLog>();
        var overtimeRepo = _repositoriesFactory.GetRepository<OvertimeHours>();
        var employeeRepo = _repositoriesFactory.GetRepository<Employee>();

        var empQuery = await employeeRepo.GetAll();
        var employees = await empQuery.Data
            .Include(e => e.Shift)
            .ToListAsync();

        var checkInLogsQuery = await checkInLogRepo.GetAll();
        var allCheckInLogs = await checkInLogsQuery.Data.ToListAsync();

        if (!allCheckInLogs.Any()) return 0;

        var existingOvertimeQuery = await overtimeRepo.GetAll();
        var existingOvertimes = await existingOvertimeQuery.Data.ToListAsync();
        var existingDict = existingOvertimes
            .GroupBy(o => (o.EmployeeId, o.Date))
            .ToDictionary(g => g.Key, g => g.First());

        var newRecords = new List<OvertimeHours>();
        int count = 0;

        foreach (var emp in employees)
        {
            if (emp.Shift == null) continue;

            var empLogs = allCheckInLogs
                .Where(l => l.EmployeeId == emp.Id)
                .GroupBy(l => DateOnly.FromDateTime(l.Time));

            foreach (var dayGroup in empLogs)
            {
                var curDate = dayGroup.Key;
                var logs = dayGroup.OrderBy(l => l.Time).ToList();
                var lastOut = logs.LastOrDefault(l => l.LogType == LogType.OUT);
                if (lastOut == null && logs.Count > 1)
                {
                    lastOut = logs.Last();
                }

                if (lastOut == null) continue;

                var shiftEnd = emp.Shift.EndTime;
                var shiftEndDateTime = DateTime.SpecifyKind(curDate.ToDateTime(shiftEnd), DateTimeKind.Unspecified);

                var overtimeSpan = lastOut.Time - shiftEndDateTime;

                // If worked more than 15 minutes past shift end time
                if (overtimeSpan.TotalMinutes > 15)
                {
                    decimal hours = Math.Round((decimal)overtimeSpan.TotalHours, 2);
                    string reason = $"سحب تلقائي من البصمة (انصراف {lastOut.Time:hh:mm tt} - نهاية وردية {shiftEnd:hh:mm tt})";

                    if (existingDict.TryGetValue((emp.Id, curDate), out var existing))
                    {
                        if (existing.Reason != null && existing.Reason.StartsWith("سحب تلقائي من البصمة") && existing.HoursCount != hours)
                        {
                            existing.HoursCount = hours;
                            existing.Reason = reason;
                            await overtimeRepo.Update(existing);
                            count++;
                        }
                    }
                    else
                    {
                        newRecords.Add(new OvertimeHours
                        {
                            Id = Guid.NewGuid(),
                            EmployeeId = emp.Id,
                            Date = curDate,
                            HoursCount = hours,
                            Reason = reason,
                            CreatedAt = DateTime.UtcNow,
                            IsActive = true
                        });
                        count++;
                    }
                }
            }
        }

        if (newRecords.Any())
        {
            await overtimeRepo.AddRange(newRecords);
        }

        return count;
    }
}