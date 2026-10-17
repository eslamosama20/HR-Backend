using System.Runtime.InteropServices;
using TimeZoneConverter;

namespace SmartAndPro.ERP.Application.Utility;

public static class TimeZoneUtils
{
    public static DateTime ApplyTimeZone(string? timeZone, DateTime dateTime)
    {
        TimeZoneInfo targetTimeZone;

        try
        {
            if (!string.IsNullOrWhiteSpace(timeZone))
            {
                targetTimeZone = TZConvert.GetTimeZoneInfo(timeZone);
            }
            else
            {
                targetTimeZone = TZConvert.GetTimeZoneInfo("Asia/Riyadh");
            }
        }
        catch (Exception)
        {
            try
            {
                targetTimeZone = TZConvert.GetTimeZoneInfo("Asia/Riyadh");
            }
            catch (Exception)
            {
                targetTimeZone = TimeZoneInfo.Utc;
            }
        }

        var utcDateTime = dateTime.Kind switch
        {
            DateTimeKind.Utc => dateTime,
            DateTimeKind.Local => dateTime.ToUniversalTime(),
            _ => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
        };

        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, targetTimeZone);
    }
}