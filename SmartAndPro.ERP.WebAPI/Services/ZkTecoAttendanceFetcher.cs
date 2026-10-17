using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SmartAndPro.ERP.Application.Common.Dtos.HR.CheckInLog;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Application.Services.HR;
using SmartAndPro.ERP.Domains.Entities;
using SmartAndPro.ERP.Domains.Entities.HR;
using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.Infrastructure.Repositories;
using Attendance_ZKTeco_Service.Interfaces;
using AttendanceFetch.Models;
using Microsoft.Extensions.Logging;

namespace SmartAndPro.ERP.WebAPI.Services
{
    public class ZkTecoSyncResult
    {
        public bool Success { get; set; }
        public string DeviceIp { get; set; } = string.Empty;
        public int DevicePort { get; set; }
        public FactoryType FactoryType { get; set; }
        public int FetchedCount { get; set; }
        public int InsertedCount { get; set; }
        public int SkippedCount { get; set; }
        public int FailedCount { get; set; }
        public string Message { get; set; } = string.Empty;
    }

    public class ZkTecoAttendanceFetcher
    {
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _deviceLocks = new();
        private readonly ILogger<ZkTecoAttendanceFetcher> _logger;
        private readonly ServiceBaseFactory _serviceBaseFactory;
        private readonly RepositoriesFactory _repositoriesFactory;
        private readonly IZKTecoAttendance_DataFetchBL _ztecoAttendance_DataFetchBL;

        public ZkTecoAttendanceFetcher(
            ILogger<ZkTecoAttendanceFetcher> logger,
            IZKTecoAttendance_DataFetchBL ztecoAttendance_DataFetchBL,
            ServiceBaseFactory serviceBaseFactory,
            RepositoriesFactory repositoriesFactory)
        {
            _logger = logger;
            _ztecoAttendance_DataFetchBL = ztecoAttendance_DataFetchBL;
            _serviceBaseFactory = serviceBaseFactory;
            _repositoriesFactory = repositoriesFactory;
        }

        /// <summary>
        /// Hangfire recurring job entry point.
        /// Throws on failure so Hangfire records the failure and handles retries.
        /// </summary>
        public async Task FetchAttendanceDataAsync(ZkTecoDevice zk, FactoryType factoryType)
        {
            var result = await SyncDeviceAttendanceAsync(zk, factoryType);
            if (!result.Success)
            {
                throw new InvalidOperationException($"ZKTeco recurring sync failed for device {zk.IpAddress}:{zk.Port}: {result.Message}");
            }
        }

        /// <summary>
        /// Executes synchronization with concurrency control and returns structured results.
        /// Safe for both Hangfire jobs and manual "Sync Now" calls.
        /// </summary>
        public async Task<ZkTecoSyncResult> SyncDeviceAttendanceAsync(ZkTecoDevice zk, FactoryType factoryType)
        {
            var deviceKey = $"{zk.IpAddress}:{zk.Port}";
            var semaphore = _deviceLocks.GetOrAdd(deviceKey, _ => new SemaphoreSlim(1, 1));

            // Prevent concurrent sync executions against the same physical device socket
            if (!await semaphore.WaitAsync(0))
            {
                var busyMsg = $"عملية مزامنة أخرى جارية حالياً للجهاز {zk.IpAddress}:{zk.Port}. يرجى الانتظار.";
                _logger.LogWarning("Concurrent sync requested for device {Ip}:{Port} - rejected.", zk.IpAddress, zk.Port);
                return new ZkTecoSyncResult
                {
                    Success = false,
                    DeviceIp = zk.IpAddress,
                    DevicePort = zk.Port,
                    FactoryType = factoryType,
                    Message = busyMsg
                };
            }

            try
            {
                _logger.LogInformation("Starting ZKTeco sync for Device: {Ip}:{Port}, Factory: {Factory}", zk.IpAddress, zk.Port, factoryType);
                await UpdateSyncStatusAsync(factoryType, "Running", "عملية المزامنة قيد التشغيل...", 0, 0, 0);

                var checkInService = _serviceBaseFactory.GetService<CheckInLogService>();
                var repo = _repositoriesFactory.GetRepository<Employee>();
                var device = new AttendanceDevice
                {
                    IPAddress = zk.IpAddress,
                    Port = zk.Port
                };

                // 1. Check device connectivity
                var status = await _ztecoAttendance_DataFetchBL.CheckDeviceConnectionStatus(zk.IpAddress, zk.Port);
                if (status.ResultType != ResultType.Success)
                {
                    var connectionErrMsg = $"فشل الاتصال بجهاز البصمة ({zk.IpAddress}:{zk.Port}): {status.Message}";
                    _logger.LogError("ZKTeco device connection failed for {Ip}:{Port}, Factory: {Factory}. Reason: {Msg}",
                        zk.IpAddress, zk.Port, factoryType, status.Message);

                    await UpdateSyncStatusAsync(factoryType, "Failed", connectionErrMsg, 0, 0, 0);
                    return new ZkTecoSyncResult
                    {
                        Success = false,
                        DeviceIp = zk.IpAddress,
                        DevicePort = zk.Port,
                        FactoryType = factoryType,
                        FailedCount = 1,
                        Message = connectionErrMsg
                    };
                }

                _logger.LogInformation("Device {Ip}:{Port} connected successfully.", zk.IpAddress, zk.Port);

                // 2. Retrieve logs from device
                var result = await _ztecoAttendance_DataFetchBL.GetData_Zkteco(device);
                if (result.ResultType != ResultType.Success || result.Data == null)
                {
                    var noDataMsg = $"تم الاتصال بنجاح ولكن لم يتم العثور على حركات جديدة: {result.Message}";
                    _logger.LogInformation("Device {Ip}:{Port} retrieved no logs or empty list: {Msg}", zk.IpAddress, zk.Port, result.Message);

                    await UpdateSyncStatusAsync(factoryType, "Success", noDataMsg, 0, 0, 0);
                    return new ZkTecoSyncResult
                    {
                        Success = true,
                        DeviceIp = zk.IpAddress,
                        DevicePort = zk.Port,
                        FactoryType = factoryType,
                        Message = noDataMsg
                    };
                }

                // 3. Filter logs by minimum date threshold (2025-01-01 to ignore reset/ancient timestamps from device)
                var minDate = new DateTime(2025, 1, 1);
                var filteredLogs = result.Data
                    .Select(log =>
                    {
                        var parsed = DateTime.TryParseExact(
                            log.DateTimeRecord?.Trim(),
                            "yyyy-MM-dd HH:mm:ss",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out var logTime);
                        return new
                        {
                            Log = log,
                            ParsedTime = parsed ? logTime : (DateTime?)null
                        };
                    })
                    .Where(x => x.ParsedTime.HasValue && x.ParsedTime.Value >= minDate)
                    .ToList();

                _logger.LogInformation("Device {Ip}:{Port} retrieved {RawCount} raw logs, {FilteredCount} valid logs after date filter (>= {MinDate}).",
                    zk.IpAddress, zk.Port, result.Data.Count, filteredLogs.Count, minDate.ToString("yyyy-MM-dd"));

                if (filteredLogs.Count == 0)
                {
                    var zeroMsg = "لا توجد حركات حديثة على جهاز البصمة مطابقة لتاريخ بدء النظام.";
                    await UpdateSyncStatusAsync(factoryType, "Success", zeroMsg, 0, 0, 0);
                    return new ZkTecoSyncResult
                    {
                        Success = true,
                        DeviceIp = zk.IpAddress,
                        DevicePort = zk.Port,
                        FactoryType = factoryType,
                        Message = zeroMsg
                    };
                }

                // 4. Extract unique device IDs
                var deviceIdSet = new HashSet<string>(filteredLogs.Select(l => l.Log.IndRegID.ToString().Trim()));

                // 5. Preload employees for those device IDs and factory type
                var employeesQuery = await repo
                    .Filter(e => deviceIdSet.Contains(e.DeviceId) && e.FactoryType == factoryType);
                var employees = employeesQuery.Data.ToList();

                // 6. Safe employee mapping: group by DeviceId to prevent ToDictionary collisions
                var employeeGroups = employees
                    .Where(e => !string.IsNullOrWhiteSpace(e.DeviceId))
                    .GroupBy(e => e.DeviceId!.Trim())
                    .ToList();

                var employeeMap = new Dictionary<string, Employee>();
                foreach (var group in employeeGroups)
                {
                    if (group.Count() > 1)
                    {
                        var empNames = string.Join(", ", group.Select(e => $"{e.Name} (ID: {e.Id})"));
                        _logger.LogWarning(
                            "Duplicate DeviceId '{DeviceId}' detected for {Count} employees in Factory {Factory}: {Employees}. Using first employee.",
                            group.Key, group.Count(), factoryType, empNames);
                    }
                    employeeMap[group.Key] = group.First();
                }

                // 7. Prepare DTOs for matching employees
                var postDtos = new List<CheckInLogPostDto>(filteredLogs.Count);
                int skippedNoEmployee = 0;

                foreach (var x in filteredLogs)
                {
                    var indRegStr = x.Log.IndRegID.ToString().Trim();
                    if (!employeeMap.TryGetValue(indRegStr, out var employee))
                    {
                        // Try numeric fallback if leading zeros vary (e.g., "05" vs "5")
                        if (int.TryParse(indRegStr, out var numericId))
                        {
                            employee = employeeMap.Values.FirstOrDefault(e =>
                                int.TryParse(e.DeviceId?.Trim(), out var empNum) && empNum == numericId);
                        }
                    }

                    if (employee == null)
                    {
                        skippedNoEmployee++;
                        continue;
                    }

                    postDtos.Add(new CheckInLogPostDto
                    {
                        EmployeeId = employee.Id,
                        LogType = x.Log.PunchInOutMode == 0 ? LogType.IN.ToString() : LogType.OUT.ToString(),
                        Time = x.ParsedTime!.Value
                    });
                }

                if (skippedNoEmployee > 0)
                {
                    _logger.LogWarning("Device {Ip}:{Port}: Skipped {Count} logs because no matching employee was found with matching DeviceId and FactoryType {Factory}.",
                        zk.IpAddress, zk.Port, skippedNoEmployee, factoryType);
                }

                // 8. Bulk insert logs using deduplicated AddRangeAsync
                int insertedCount = 0;
                if (postDtos.Count > 0)
                {
                    var opResult = await checkInService.AddRangeAsync(postDtos);
                    insertedCount = opResult.Data?.Count ?? 0;
                }

                int skippedCount = filteredLogs.Count - insertedCount;
                var summaryMsg = $"تمت المزامنة بنجاح: تم سحب {filteredLogs.Count} حركة، تمت إضافة {insertedCount} حركة جديدة، تم تخطي {skippedCount} حركة (مكررة أو لموظف غير مسجل).";

                _logger.LogInformation("ZKTeco sync completed for {Ip}:{Port}, Factory: {Factory}. Fetched: {Fetched}, Inserted: {Inserted}, Skipped: {Skipped}",
                    zk.IpAddress, zk.Port, factoryType, filteredLogs.Count, insertedCount, skippedCount);

                await UpdateSyncStatusAsync(factoryType, "Success", summaryMsg, filteredLogs.Count, insertedCount, skippedCount);

                return new ZkTecoSyncResult
                {
                    Success = true,
                    DeviceIp = zk.IpAddress,
                    DevicePort = zk.Port,
                    FactoryType = factoryType,
                    FetchedCount = filteredLogs.Count,
                    InsertedCount = insertedCount,
                    SkippedCount = skippedCount,
                    Message = summaryMsg
                };
            }
            catch (Exception ex)
            {
                var failureMsg = $"حدث خطأ أثناء مزامنة البصمة: {ex.Message}";
                _logger.LogError(ex, "ZKTeco sync failed for Device {Ip}:{Port}, Factory: {Factory}. Reason: {Msg}",
                    zk.IpAddress, zk.Port, factoryType, ex.Message);

                await UpdateSyncStatusAsync(factoryType, "Failed", failureMsg, 0, 0, 0);

                return new ZkTecoSyncResult
                {
                    Success = false,
                    DeviceIp = zk.IpAddress,
                    DevicePort = zk.Port,
                    FactoryType = factoryType,
                    FailedCount = 1,
                    Message = failureMsg
                };
            }
            finally
            {
                semaphore.Release();
            }
        }

        private async Task UpdateSyncStatusAsync(FactoryType factoryType, string status, string message, int fetched, int inserted, int skipped)
        {
            try
            {
                var settingsRepo = _repositoriesFactory.GetRepository<FingerprintDeviceSettings>();
                var settingsQuery = await settingsRepo.GetAll();
                var settings = settingsQuery.Data.FirstOrDefault();
                if (settings == null) return;

                if (factoryType == FactoryType.Factory1)
                {
                    settings.Factory1LastSyncTime = DateTime.Now;
                    settings.Factory1LastSyncStatus = status;
                    settings.Factory1LastSyncMessage = message;
                    settings.Factory1LastSyncFetchedCount = fetched;
                    settings.Factory1LastSyncInsertedCount = inserted;
                    settings.Factory1LastSyncSkippedCount = skipped;
                }
                else if (factoryType == FactoryType.Factory2)
                {
                    settings.Factory2LastSyncTime = DateTime.Now;
                    settings.Factory2LastSyncStatus = status;
                    settings.Factory2LastSyncMessage = message;
                    settings.Factory2LastSyncFetchedCount = fetched;
                    settings.Factory2LastSyncInsertedCount = inserted;
                    settings.Factory2LastSyncSkippedCount = skipped;
                }
                else if (factoryType == FactoryType.Factory3)
                {
                    settings.Factory3LastSyncTime = DateTime.Now;
                    settings.Factory3LastSyncStatus = status;
                    settings.Factory3LastSyncMessage = message;
                    settings.Factory3LastSyncFetchedCount = fetched;
                    settings.Factory3LastSyncInsertedCount = inserted;
                    settings.Factory3LastSyncSkippedCount = skipped;
                }

                await settingsRepo.Update(settings);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to update FingerprintDeviceSettings sync status in database.");
            }
        }
    }
}
