using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using OfficeOpenXml;
using SmartAndPro.ERP.Application.Utility;
using SmartAndPro.ERP.WebAPI.Converters;
using Xunit;

namespace SmartAndPro.ERP.Tests.ApplicationTests.Services
{
    public class DateTimeAndExcelDateParsingTests
    {
        private static readonly string[] SupportedDateFormats = new[]
        {
            "dd/MM/yyyy", "d/M/yyyy", "dd/M/yyyy", "d/MM/yyyy",
            "dd-MM-yyyy", "d-M-yyyy", "dd-M-yyyy", "d-MM-yyyy",
            "yyyy-MM-dd", "yyyy/MM/dd", "yyyy.MM.dd",
            "MM/dd/yyyy", "M/d/yyyy", "M/dd/yyyy", "MM/d/yyyy",
            "MM-dd-yyyy", "M-d-yyyy", "M-dd-yyyy", "MM-d-yyyy",
            "dd/MM/yyyy HH:mm:ss", "dd/MM/yyyy hh:mm:ss tt",
            "dd/MM/yyyy H:mm:ss", "dd/MM/yyyy h:mm:ss tt",
            "dd-MM-yyyy HH:mm:ss", "dd-MM-yyyy hh:mm:ss tt",
            "yyyy-MM-dd HH:mm:ss", "yyyy-MM-ddTHH:mm:ss", "yyyy-MM-ddTHH:mm:ss.fffZ",
            "MM/dd/yyyy HH:mm:ss", "MM/dd/yyyy hh:mm:ss tt",
            "dd/MM/yy", "d/M/yy", "dd-MM-yy", "d-M-yy", "yy-MM-dd", "yy/MM/dd",
            "MM/dd/yy", "M/d/yy", "MM-dd-yy", "M-d-yy"
        };

        private static string NormalizeArabicDigits(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            return input
                .Replace('٠', '0').Replace('١', '1').Replace('٢', '2').Replace('٣', '3').Replace('٤', '4')
                .Replace('٥', '5').Replace('٦', '6').Replace('٧', '7').Replace('٨', '8').Replace('٩', '9')
                .Replace('۰', '0').Replace('۱', '1').Replace('۲', '2').Replace('۳', '3').Replace('۴', '4')
                .Replace('۵', '5').Replace('۶', '6').Replace('۷', '7').Replace('۸', '8').Replace('۹', '9');
        }

        [Theory]
        [InlineData("29/09/2026", 2026, 9, 29)]
        [InlineData("01/05/2025", 2025, 5, 1)]
        [InlineData("5/7/2024", 2024, 7, 5)]
        [InlineData("29-09-2026", 2026, 9, 29)]
        [InlineData("01-05-2025", 2025, 5, 1)]
        [InlineData("2026-09-29", 2026, 9, 29)]
        [InlineData("2026/09/29", 2026, 9, 29)]
        public void ParseDate_SupportedFormats_ShouldParseCorrectly(string input, int expectedYear, int expectedMonth, int expectedDay)
        {
            var success = DateTime.TryParseExact(input, SupportedDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result);

            Assert.True(success, $"Failed to parse date string: {input}");
            Assert.Equal(expectedYear, result.Year);
            Assert.Equal(expectedMonth, result.Month);
            Assert.Equal(expectedDay, result.Day);
        }

        [Theory]
        [InlineData("٢٩/٠٩/٢٠٢٦", 2026, 9, 29)]
        [InlineData("٢٠٢٦-٠٩-٢٩", 2026, 9, 29)]
        [InlineData("٠١-٠٥-٢٠٢٥", 2025, 5, 1)]
        public void ParseDate_ArabicIndicDigits_ShouldNormalizeAndParse(string arabicInput, int expectedYear, int expectedMonth, int expectedDay)
        {
            var normalized = NormalizeArabicDigits(arabicInput);
            var success = DateTime.TryParseExact(normalized, SupportedDateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result);

            Assert.True(success, $"Failed to parse Arabic date: {arabicInput} -> {normalized}");
            Assert.Equal(expectedYear, result.Year);
            Assert.Equal(expectedMonth, result.Month);
            Assert.Equal(expectedDay, result.Day);
        }

        [Fact]
        public void ParseDate_ExcelOADateSerial_ShouldParseCorrectly()
        {
            // In Excel, 46294.0 represents 2026-09-29
            double serial = 46294.0;
            var parsed = DateTime.FromOADate(serial).Date;

            Assert.Equal(2026, parsed.Year);
            Assert.Equal(9, parsed.Month);
            Assert.Equal(29, parsed.Day);
        }

        [Fact]
        public void TimeZoneUtils_ApplyRiyadhTimeZone_ShouldAddThreeHours()
        {
            // 2026-09-29 12:00:00 UTC -> 2026-09-29 15:00:00 in Asia/Riyadh (UTC+3)
            var utcTime = new DateTime(2026, 9, 29, 12, 0, 0, DateTimeKind.Utc);
            var riyadhTime = TimeZoneUtils.ApplyTimeZone("Asia/Riyadh", utcTime);

            Assert.Equal(2026, riyadhTime.Year);
            Assert.Equal(9, riyadhTime.Month);
            Assert.Equal(29, riyadhTime.Day);
            Assert.Equal(15, riyadhTime.Hour);
            Assert.Equal(0, riyadhTime.Minute);
        }

        [Fact]
        public void DateRangeBoundary_TimeOnlyMaxValue_ShouldCoverEntireDay()
        {
            var startDate = new DateOnly(2026, 9, 1);
            var endDate = new DateOnly(2026, 9, 29);

            var fromDateTime = startDate.ToDateTime(TimeOnly.MinValue);
            var toDateTime = endDate.ToDateTime(TimeOnly.MaxValue);

            // A transaction created at 23:55:00 on the last day
            var transactionTime = new DateTime(2026, 9, 29, 23, 55, 0);

            Assert.True(transactionTime >= fromDateTime);
            Assert.True(transactionTime <= toDateTime, "Transaction on the last day should be within the range");
        }

        [Fact]
        public void DateOnlyJsonConverter_ShouldSerializeToStandardIsoDate()
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new DateOnlyJsonConverter());

            var testObj = new { Date = (DateOnly?)new DateOnly(2026, 9, 29) };
            var json = JsonSerializer.Serialize(testObj, options);

            Assert.Contains("\"2026-09-29\"", json);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("Invalid/Timezone_Name_123")]
        public void TimeZoneUtils_InvalidOrEmptyTimezone_ShouldFallbackGracefullyWithoutThrowing(string? invalidTz)
        {
            var utcTime = new DateTime(2026, 9, 29, 12, 0, 0, DateTimeKind.Utc);
            var result = TimeZoneUtils.ApplyTimeZone(invalidTz, utcTime);

            // Defaults to Asia/Riyadh (UTC+3)
            Assert.Equal(2026, result.Year);
            Assert.Equal(9, result.Month);
            Assert.Equal(29, result.Day);
            Assert.Equal(15, result.Hour);
        }

        [Fact]
        public void TimeZoneUtils_CrossMidnightBoundary_ShouldAdvanceToNextDay()
        {
            // 2026-09-29 22:30:00 UTC -> 2026-09-30 01:30:00 in Asia/Riyadh (+3)
            var utcTime = new DateTime(2026, 9, 29, 22, 30, 0, DateTimeKind.Utc);
            var riyadhTime = TimeZoneUtils.ApplyTimeZone("Asia/Riyadh", utcTime);

            Assert.Equal(2026, riyadhTime.Year);
            Assert.Equal(9, riyadhTime.Month);
            Assert.Equal(30, riyadhTime.Day);
            Assert.Equal(1, riyadhTime.Hour);
            Assert.Equal(30, riyadhTime.Minute);
        }

        [Fact]
        public void TimeZoneUtils_CrossNewYearBoundary_ShouldAdvanceToNextYear()
        {
            // 2026-12-31 22:00:00 UTC -> 2027-01-01 01:00:00 in Asia/Riyadh (+3)
            var utcTime = new DateTime(2026, 12, 31, 22, 0, 0, DateTimeKind.Utc);
            var riyadhTime = TimeZoneUtils.ApplyTimeZone("Asia/Riyadh", utcTime);

            Assert.Equal(2027, riyadhTime.Year);
            Assert.Equal(1, riyadhTime.Month);
            Assert.Equal(1, riyadhTime.Day);
            Assert.Equal(1, riyadhTime.Hour);
        }

        [Fact]
        public void TimeZoneUtils_DateTimeKinds_ShouldHandleUtcAndUnspecifiedConsistently()
        {
            var utcDate = new DateTime(2026, 5, 10, 10, 0, 0, DateTimeKind.Utc);
            var unspecifiedDate = new DateTime(2026, 5, 10, 10, 0, 0, DateTimeKind.Unspecified);

            var resultFromUtc = TimeZoneUtils.ApplyTimeZone("Asia/Riyadh", utcDate);
            var resultFromUnspecified = TimeZoneUtils.ApplyTimeZone("Asia/Riyadh", unspecifiedDate);

            Assert.Equal(resultFromUtc, resultFromUnspecified);
            Assert.Equal(13, resultFromUtc.Hour); // 10:00 + 3 hours = 13:00
        }

        [Fact]
        public void DateFormatting_StandardDdMmYyyy_ShouldFormatWithLeadingZeros()
        {
            var date = new DateTime(2026, 5, 4); // May 4th
            var formatted = date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

            Assert.Equal("04/05/2026", formatted);
        }

        [Fact]
        public void DateTimeUtcJsonConverter_SerializesWithZSuffix()
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new DateTimeUtcJsonConverter());

            var dt = new DateTime(2026, 10, 1, 11, 30, 0, DateTimeKind.Utc);
            var json = JsonSerializer.Serialize(dt, options);

            Assert.Contains("2026-10-01T11:30:00", json);
            Assert.EndsWith("Z\"", json);
        }

        [Fact]
        public void DateTimeUtcJsonConverter_UnspecifiedKind_TreatedAsUtcWithZSuffix()
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new DateTimeUtcJsonConverter());

            // EF Core returns Unspecified DateTime for datetime2 columns
            var dt = new DateTime(2026, 10, 1, 11, 30, 0, DateTimeKind.Unspecified);
            var json = JsonSerializer.Serialize(dt, options);

            Assert.Contains("2026-10-01T11:30:00", json);
            Assert.EndsWith("Z\"", json);
        }

        [Fact]
        public void NullableDateTimeUtcJsonConverter_SerializesNullCorrectly()
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new NullableDateTimeUtcJsonConverter());

            DateTime? dt = null;
            var json = JsonSerializer.Serialize(dt, options);

            Assert.Equal("null", json);
        }

        [Fact]
        public void NullableDateTimeUtcJsonConverter_SerializesValueWithZSuffix()
        {
            var options = new JsonSerializerOptions();
            options.Converters.Add(new NullableDateTimeUtcJsonConverter());

            DateTime? dt = new DateTime(2026, 10, 1, 11, 30, 0, DateTimeKind.Utc);
            var json = JsonSerializer.Serialize(dt, options);

            Assert.Contains("2026-10-01T11:30:00", json);
            Assert.EndsWith("Z\"", json);
        }
    }
}
