using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;

namespace SmartAndPro.ERP.WebAPI.Converters;

/// <summary>
/// Ensures all DateTime values sent to frontend are formatted in ISO 8601 UTC with 'Z' suffix
/// and deserialized accurately from UTC or local representations.
/// </summary>
public class DateTimeUtcJsonConverter : JsonConverter<DateTime>
{
    private static readonly DateTimeStyles DateTimeParseStyles =
        DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal;

    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var dateString = reader.GetString();
            if (string.IsNullOrWhiteSpace(dateString))
            {
                return DateTime.MinValue;
            }

            if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeParseStyles, out var dateTime))
            {
                return dateTime;
            }

            if (DateTime.TryParse(dateString, out dateTime))
            {
                return dateTime.ToUniversalTime();
            }
        }

        if (reader.TokenType == JsonTokenType.Null)
        {
            return DateTime.MinValue;
        }

        return reader.GetDateTime().ToUniversalTime();
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var utcDateTime = value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

        writer.WriteStringValue(utcDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture));
    }
}

/// <summary>
/// Ensures all nullable DateTime? values sent to frontend are formatted in ISO 8601 UTC with 'Z' suffix.
/// </summary>
public class NullableDateTimeUtcJsonConverter : JsonConverter<DateTime?>
{
    private static readonly DateTimeStyles DateTimeParseStyles =
        DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal;

    public override DateTime? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var dateString = reader.GetString();
            if (string.IsNullOrWhiteSpace(dateString))
            {
                return null;
            }

            if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeParseStyles, out var dateTime))
            {
                return dateTime;
            }

            if (DateTime.TryParse(dateString, out dateTime))
            {
                return dateTime.ToUniversalTime();
            }
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, DateTime? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            var val = value.Value;
            var utcDateTime = val.Kind switch
            {
                DateTimeKind.Utc => val,
                DateTimeKind.Local => val.ToUniversalTime(),
                _ => DateTime.SpecifyKind(val, DateTimeKind.Utc)
            };

            writer.WriteStringValue(utcDateTime.ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture));
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
