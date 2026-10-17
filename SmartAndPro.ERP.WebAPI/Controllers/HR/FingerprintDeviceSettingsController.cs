using SmartAndPro.ERP.Application.Common.Dtos;
using SmartAndPro.ERP.Application.Common.Dtos.FingerprintDeviceSettings;
using SmartAndPro.ERP.Application.Services;
using SmartAndPro.ERP.Application.Services.@base;
using SmartAndPro.ERP.Domains.Enums;
using SmartAndPro.ERP.SharedKernel.Utilities;
using SmartAndPro.ERP.WebAPI.Services;
using SmartAndPro.ERP.WebAPI.Utils;
using Attendance_ZKTeco_Service.Interfaces;
using AttendanceFetch.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SmartAndPro.ERP.WebAPI.Controllers.HR;
[Route("api/hr/[controller]/")]
[ApiController]
[Authorize]
public class FingerprintDeviceSettingsController : Controller
{
    private readonly ServiceBaseFactory _serviceBaseFactory;
    private readonly FingerprintDeviceSettingsService _service;
    private readonly IZKTecoAttendance_DataFetchBL _ztecoAttendance_DataFetchBL;
    private readonly IHangfireSchedulerService _schedulerService;
    private readonly ZkTecoAttendanceFetcher _fetcher;
    
    public FingerprintDeviceSettingsController(
        ServiceBaseFactory serviceBaseFactory,
        IZKTecoAttendance_DataFetchBL ztecoAttendance_DataFetchBL,
        IHangfireSchedulerService schedulerService,
        ZkTecoAttendanceFetcher fetcher)
    {
        _serviceBaseFactory = serviceBaseFactory;
        _service = _serviceBaseFactory.GetService<FingerprintDeviceSettingsService>();
        _ztecoAttendance_DataFetchBL = ztecoAttendance_DataFetchBL;
        _schedulerService = schedulerService;
        _fetcher = fetcher;
    }
    
    [HttpPost("reschedule-jobs")]
    public async Task<IActionResult> RescheduleJobs()
    {
        try
        {
            await _schedulerService.ScheduleAllJobsAsync();
            return Ok("Jobs rescheduled.");
        }
        catch (Exception e)
        {
            LogService.LogError(e);
            return BadRequest(e.Message);
        }
    }

    [HttpPost("sync-now/{factoryType}")]
    public async Task<IActionResult> SyncNow(string factoryType)
    {
        if (!Enum.TryParse<FactoryType>(factoryType, true, out var factoryTypeEnum))
        {
            return BadRequest(new ZkTecoSyncResult
            {
                Success = false,
                Message = "نوع المصنع غير صالح. استخدم Factory1 أو Factory2 أو Factory3."
            });
        }

        try
        {
            var settingsResponse = await _service.GetAllAsync(new ParamFilter());
            var settings = settingsResponse.Data?.FirstOrDefault();
            if (settings == null)
            {
                return BadRequest(new ZkTecoSyncResult
                {
                    Success = false,
                    Message = "لم يتم العثور على إعدادات أجهزة البصمة. يرجى إدخال الإعدادات أولاً."
                });
            }

            string? ip;
            string? portStr;
            bool isEnabled;

            if (factoryTypeEnum == FactoryType.Factory1)
            {
                ip = settings.Factory1DeviceIP;
                portStr = settings.Factory1DevicePort;
                isEnabled = settings.Factory1DeviceEnabled;
            }
            else if (factoryTypeEnum == FactoryType.Factory2)
            {
                ip = settings.Factory2DeviceIP;
                portStr = settings.Factory2DevicePort;
                isEnabled = settings.Factory2DeviceEnabled;
            }
            else
            {
                ip = settings.Factory3DeviceIP;
                portStr = settings.Factory3DevicePort;
                isEnabled = settings.Factory3DeviceEnabled;
            }

            if (string.IsNullOrWhiteSpace(ip) || string.IsNullOrWhiteSpace(portStr))
            {
                return BadRequest(new ZkTecoSyncResult
                {
                    Success = false,
                    FactoryType = factoryTypeEnum,
                    Message = $"عنوان IP أو Port لجهاز {factoryType} غير مضبوط في الإعدادات."
                });
            }

            if (!isEnabled)
            {
                return BadRequest(new ZkTecoSyncResult
                {
                    Success = false,
                    DeviceIp = ip,
                    FactoryType = factoryTypeEnum,
                    Message = $"جهاز {factoryType} غير مفعّل. يرجى تفعيله من الإعدادات والضغط على حفظ."
                });
            }

            if (!int.TryParse(portStr, out var port))
            {
                return BadRequest(new ZkTecoSyncResult
                {
                    Success = false,
                    DeviceIp = ip,
                    FactoryType = factoryTypeEnum,
                    Message = $"منفذ (Port) جهاز {factoryType} غير صالح: {portStr}"
                });
            }

            var zkDevice = new ZkTecoDevice
            {
                IpAddress = ip,
                Port = port,
                FactoryType = factoryTypeEnum
            };

            var syncResult = await _fetcher.SyncDeviceAttendanceAsync(zkDevice, factoryTypeEnum);
            if (syncResult.Success)
            {
                return Ok(syncResult);
            }
            else
            {
                return BadRequest(syncResult);
            }
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return StatusCode(500, new ZkTecoSyncResult
            {
                Success = false,
                FactoryType = factoryTypeEnum,
                Message = $"حدث خطأ غير متوقع أثناء المزامنة: {ex.Message}"
            });
        }
    }
    
    [HttpGet("checkConnectionForFactory1Device")]
    public async Task<IActionResult> CheckConnectionForFactory1Device([FromQuery] string? ip = null, [FromQuery] string? port = null)
    {
        try
        {
            string? deviceIp = ip;
            string? devicePort = port;

            if (string.IsNullOrWhiteSpace(deviceIp) || string.IsNullOrWhiteSpace(devicePort))
            {
                var settings = await _service.GetAllAsync(new ParamFilter());
                var settingsData = settings.Data?.FirstOrDefault();
                if (settingsData != null)
                {
                    deviceIp = settingsData.Factory1DeviceIP;
                    devicePort = settingsData.Factory1DevicePort;
                }
            }

            if (string.IsNullOrWhiteSpace(deviceIp) || string.IsNullOrWhiteSpace(devicePort))
            {
                return Ok(new { success = false, connected = false, message = "يرجى إدخال عنوان IP ورقم Port للمصنع الأول." });
            }

            if (!int.TryParse(devicePort, out int portNum))
            {
                return Ok(new { success = false, connected = false, message = "رقم Port غير صالح." });
            }

            var responseDate = await _ztecoAttendance_DataFetchBL.CheckDeviceConnectionStatus(deviceIp, portNum);
            bool isConnected = responseDate.ResultType == ResultType.Success;
            return Ok(new { success = true, connected = isConnected, message = isConnected ? "تم الاتصال بجهاز البصمة بنجاح." : "تعذر الاتصال بالجهاز. يرجى التأكد من تشغيل الجهاز وتوصيله بنفس الشبكة." });
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return Ok(new { success = false, connected = false, message = $"فشل التحقق: {ex.Message}" });
        }
    }

    [HttpGet("checkConnectionForFactory2Device")]
    public async Task<IActionResult> CheckConnectionForFactory2Device([FromQuery] string? ip = null, [FromQuery] string? port = null)
    {
        try
        {
            string? deviceIp = ip;
            string? devicePort = port;

            if (string.IsNullOrWhiteSpace(deviceIp) || string.IsNullOrWhiteSpace(devicePort))
            {
                var settings = await _service.GetAllAsync(new ParamFilter());
                var settingsData = settings.Data?.FirstOrDefault();
                if (settingsData != null)
                {
                    deviceIp = settingsData.Factory2DeviceIP;
                    devicePort = settingsData.Factory2DevicePort;
                }
            }

            if (string.IsNullOrWhiteSpace(deviceIp) || string.IsNullOrWhiteSpace(devicePort))
            {
                return Ok(new { success = false, connected = false, message = "يرجى إدخال عنوان IP ورقم Port للمصنع الثاني." });
            }

            if (!int.TryParse(devicePort, out int portNum))
            {
                return Ok(new { success = false, connected = false, message = "رقم Port غير صالح." });
            }

            var responseDate = await _ztecoAttendance_DataFetchBL.CheckDeviceConnectionStatus(deviceIp, portNum);
            bool isConnected = responseDate.ResultType == ResultType.Success;
            return Ok(new { success = true, connected = isConnected, message = isConnected ? "تم الاتصال بجهاز البصمة بنجاح." : "تعذر الاتصال بالجهاز. يرجى التأكد من تشغيل الجهاز وتوصيله بنفس الشبكة." });
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return Ok(new { success = false, connected = false, message = $"فشل التحقق: {ex.Message}" });
        }
    }

    [HttpGet("checkConnectionForFactory3Device")]
    public async Task<IActionResult> CheckConnectionForFactory3Device([FromQuery] string? ip = null, [FromQuery] string? port = null)
    {
        try
        {
            string? deviceIp = ip;
            string? devicePort = port;

            if (string.IsNullOrWhiteSpace(deviceIp) || string.IsNullOrWhiteSpace(devicePort))
            {
                var settings = await _service.GetAllAsync(new ParamFilter());
                var settingsData = settings.Data?.FirstOrDefault();
                if (settingsData != null)
                {
                    deviceIp = settingsData.Factory3DeviceIP;
                    devicePort = settingsData.Factory3DevicePort;
                }
            }

            if (string.IsNullOrWhiteSpace(deviceIp) || string.IsNullOrWhiteSpace(devicePort))
            {
                return Ok(new { success = false, connected = false, message = "يرجى إدخال عنوان IP ورقم Port للمصنع الثالث." });
            }

            if (!int.TryParse(devicePort, out int portNum))
            {
                return Ok(new { success = false, connected = false, message = "رقم Port غير صالح." });
            }

            var responseDate = await _ztecoAttendance_DataFetchBL.CheckDeviceConnectionStatus(deviceIp, portNum);
            bool isConnected = responseDate.ResultType == ResultType.Success;
            return Ok(new { success = true, connected = isConnected, message = isConnected ? "تم الاتصال بجهاز البصمة بنجاح." : "تعذر الاتصال بالجهاز. يرجى التأكد من تشغيل الجهاز وتوصيله بنفس الشبكة." });
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
            return Ok(new { success = false, connected = false, message = $"فشل التحقق: {ex.Message}" });
        }
    }

    [HttpPost("clear-data/{factoryType}")]
    public async Task<IActionResult> ClearData(string factoryType)
    {
        try
        {
            var factoryTypeEnum = Enum.Parse<FactoryType>(factoryType);
            var settings = await _service.GetAllAsync(new ParamFilter());
            var settingsData = settings.Data.FirstOrDefault();
            if (settingsData == null)
            {
                return BadRequest("No FingerprintDeviceSettings configured.");
            }

            if (factoryTypeEnum == FactoryType.Factory1)
            {
                if (settingsData.Factory1DeviceIP == null || settingsData.Factory1DevicePort == null)
                {
                    return BadRequest("Factory1 Device IP or Port not configured.");
                }
                
                var deviceStatus = await _ztecoAttendance_DataFetchBL.CheckDeviceConnectionStatus(settingsData.Factory1DeviceIP, int.Parse(settingsData.Factory1DevicePort));
                if (deviceStatus.ResultType == ResultType.Success)
                {
                    var device = new AttendanceDevice
                    {
                        IPAddress =settingsData.Factory1DeviceIP,
                        Port =  int.Parse(settingsData.Factory1DevicePort)
                    };
                    var clearResult = await _ztecoAttendance_DataFetchBL.DeleteData_Zkteco(device);
                    if (clearResult.ResultType == ResultType.Success)
                    {
                        return Ok("Factory1 Device data cleared.");
                    }
                    else
                    {
                        return BadRequest(clearResult.Message);
                    }
                }
                return BadRequest(deviceStatus.Message);
            } 
            if (factoryTypeEnum == FactoryType.Factory2)
            {
                if (settingsData.Factory2DeviceIP == null || settingsData.Factory2DevicePort == null)
                {
                    return BadRequest("Factory2 Device IP or Port not configured.");
                }
                
                var deviceStatus = await _ztecoAttendance_DataFetchBL.CheckDeviceConnectionStatus(settingsData.Factory2DeviceIP, int.Parse(settingsData.Factory2DevicePort));
                if (deviceStatus.ResultType == ResultType.Success)
                {
                    var device = new AttendanceDevice
                    {
                        IPAddress =settingsData.Factory2DeviceIP,
                        Port =  int.Parse(settingsData.Factory2DevicePort)
                    };
                    var clearResult = await _ztecoAttendance_DataFetchBL.DeleteData_Zkteco(device);
                    if (clearResult.ResultType == ResultType.Success)
                    {
                        return Ok("Factory2 Device data cleared.");
                    }
                    else
                    {
                        return BadRequest(clearResult.Message);
                    }
                }
                return BadRequest(deviceStatus.Message);
            }
            if (factoryTypeEnum == FactoryType.Factory3)
            {
                if (settingsData.Factory3DeviceIP == null || settingsData.Factory3DevicePort == null)
                {
                    return BadRequest("Factory3 Device IP or Port not configured.");
                }
                
                var deviceStatus = await _ztecoAttendance_DataFetchBL.CheckDeviceConnectionStatus(settingsData.Factory3DeviceIP, int.Parse(settingsData.Factory3DevicePort));
                if (deviceStatus.ResultType == ResultType.Success)
                {
                    var device = new AttendanceDevice
                    {
                        IPAddress =settingsData.Factory3DeviceIP,
                        Port =  int.Parse(settingsData.Factory3DevicePort)
                    };
                    var clearResult = await _ztecoAttendance_DataFetchBL.DeleteData_Zkteco(device);
                    if (clearResult.ResultType == ResultType.Success)
                    {
                        return Ok("Factory3 Device data cleared.");
                    }
                    else
                    {
                        return BadRequest(clearResult.Message);
                    }
                }
                return BadRequest(deviceStatus.Message);
            }
            return BadRequest();
        }
        catch (Exception e)
        {
            LogService.LogError(e);
            return BadRequest(e.Message);
        }
    }
    

    // FingerprintDeviceSettings get all
    
    [HttpGet("")]
    public async Task<IActionResult> GetFingerprintDeviceSettings([FromQuery] ParamFilter filters )
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.GetAllAsync(filters);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // FingerprintDeviceSettings get by id
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetFingerprintDeviceSettingsById(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.GetAsync(id);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // FingerprintDeviceSettings create
    [HttpPost("")]
    public async Task<IActionResult> CreateFingerprintDeviceSettings([FromBody] FingerprintDeviceSettingsPostDto paycheckReceiptAccount)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.AddAsync(paycheckReceiptAccount);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // FingerprintDeviceSettings update
    [HttpPut]
    public async Task<IActionResult> UpdateFingerprintDeviceSettings([FromBody] FingerprintDeviceSettingsPutDto paycheckReceiptAccount)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.UpdateAsync(paycheckReceiptAccount);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }
        return result;
    }
    
    // FingerprintDeviceSettings delete
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteFingerprintDeviceSettings(Guid id)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);
        IActionResult result = NoContent();
        try
        {
            var responseDate = await _service.DeleteAsync(id);
            result = responseDate.HandleResult();
        }
        catch (Exception ex)
        {
            LogService.LogError(ex);
        }

        return result;
    }

}