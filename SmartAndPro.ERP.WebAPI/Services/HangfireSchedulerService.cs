
using System;
using System.Collections.Generic;
using SmartAndPro.ERP.Application.Services.HR;
using SmartAndPro.ERP.Domains.Entities;
using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.Infrastructure.Repositories;
using SmartAndPro.ERP.SharedKernel.Entities.Configurations;
using Hangfire;
using Hangfire.Storage;
using Microsoft.Extensions.Logging;

namespace SmartAndPro.ERP.WebAPI.Services
{
    public interface IHangfireSchedulerService
    {
        Task ScheduleAllJobsAsync();
    }

    public class HangfireSchedulerService : IHangfireSchedulerService
    {
        private readonly ILogger<HangfireSchedulerService> _logger;
        private readonly AppSettings _appSettings;
        private readonly RepositoriesFactory _repositoriesFactory;
        public HangfireSchedulerService(
            ILogger<HangfireSchedulerService> logger,
            AppSettings appSettings,
            RepositoriesFactory repositoriesFactory
            )
        {
            _logger = logger;
            _appSettings = appSettings;
            _repositoriesFactory = repositoriesFactory;
        }

        public async Task ScheduleAllJobsAsync()
        {
            try
            {
                using (var connection = JobStorage.Current.GetConnection())
                {
                    var recurringJobs = connection.GetRecurringJobs();
                    foreach (var job in recurringJobs)
                    {
                        RecurringJob.RemoveIfExists(job.Id);
                        _logger.LogInformation("Deleted existing Hangfire recurring job: {JobId}", job.Id);
                    }
                }
                RecurringJob.AddOrUpdate<AttendanceService>(
                    "schedule-shift-jobs",
                    x => x.ScheduleDailyShiftJobsAsync(),
                    "0 0 * * *"
                );
                
                var settingsQuery = await _repositoriesFactory.GetRepository<FingerprintDeviceSettings>().GetAll();
                var settings = settingsQuery?.Data?.FirstOrDefault();
                if (settings == null)
                {
                    _logger.LogWarning("No FingerprintDeviceSettings configured; skipping Hangfire scheduling.");
                    return;
                }
                var devices = new List<ZkTecoDevice>();
                if (!string.IsNullOrWhiteSpace(settings.Factory1DeviceIP) && !string.IsNullOrWhiteSpace(settings.Factory1DevicePort) && settings.Factory1DeviceEnabled && int.TryParse(settings.Factory1DevicePort, out var p1))
                {
                    devices.Add(new ZkTecoDevice
                    {
                        IpAddress = settings.Factory1DeviceIP,
                        Port = p1,
                        Period = 12,
                        FactoryType = FactoryType.Factory1
                    });
                }
                if (!string.IsNullOrWhiteSpace(settings.Factory2DeviceIP) && !string.IsNullOrWhiteSpace(settings.Factory2DevicePort) && settings.Factory2DeviceEnabled && int.TryParse(settings.Factory2DevicePort, out var p2))
                {
                    devices.Add(new ZkTecoDevice
                    {
                        IpAddress = settings.Factory2DeviceIP,
                        Port = p2,
                        Period = 12,
                        FactoryType = FactoryType.Factory2
                    });
                }
                if (!string.IsNullOrWhiteSpace(settings.Factory3DeviceIP) && !string.IsNullOrWhiteSpace(settings.Factory3DevicePort) && settings.Factory3DeviceEnabled && int.TryParse(settings.Factory3DevicePort, out var p3))
                {
                    devices.Add(new ZkTecoDevice
                    {
                        IpAddress = settings.Factory3DeviceIP,
                        Port = p3,
                        Period = 12,
                        FactoryType = FactoryType.Factory3
                    });
                }

                foreach (var zk in devices)
                {
                    var jobId = $"ZkFetch_{zk.IpAddress.Replace('.', '_')}_{zk.Port}";
                    // Determine cron expression based on period
                    string cronExpr;
                    if (zk.Period == 0)
                    {
                        cronExpr = "0 */1 * * *"; // every hour
                    }
                    else
                    {
                        cronExpr = $"0 */{zk.Period} * * *"; // every zk.Period hours, at minute 0
                    }

                    // Use IRecurringJobManager to register jobs dynamically
                    RecurringJob.AddOrUpdate<ZkTecoAttendanceFetcher>(
                        jobId,
                        fetcher => fetcher.FetchAttendanceDataAsync(zk, zk.FactoryType),
                        cronExpr
                    );

                    _logger.LogInformation(
                        "Scheduled Hangfire job '{JobId}' for device {Ip}:{Port} every {Period} {Unit} (cron: {Cron}).",
                        jobId, zk.IpAddress, zk.Port, zk.Period == 0 ? "1" : zk.Period.ToString(), zk.Period == 0 ? "minute" : "hours", cronExpr);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while scheduling Hangfire jobs.");
            }
        }
    }
}
public class ZkTecoDevice
{
    public string IpAddress { get; set; }
    public int Port { get; set; }
    public int Period { get; set; }
    public FactoryType FactoryType { get; set; }  // <-- Add this
}
