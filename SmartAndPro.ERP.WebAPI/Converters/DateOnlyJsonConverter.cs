using System.Text.Json;
using System.Text.Json.Serialization;
using System.Globalization;

namespace SmartAndPro.ERP.WebAPI.Converters;

public class DateOnlyJsonConverter : JsonConverter<DateOnly?>
{
    private const string DateFormat = "yyyy-MM-dd";
    private static readonly DateTimeStyles DateTimeParseStyles =
        DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal;

    public override DateOnly? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            var dateString = reader.GetString();
            if (string.IsNullOrEmpty(dateString))
            {
                return null;
            }

            if (DateOnly.TryParseExact(dateString, DateFormat, out var date))
            {
                return date;
            }

            if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeParseStyles, out var dateTime))
            {
                return DateOnly.FromDateTime(dateTime);
            }

            // Try parsing with other common formats
            if (DateOnly.TryParse(dateString, out date))
            {
                return date;
            }
        }

        return null;
    }

    public override void Write(Utf8JsonWriter writer, DateOnly? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
        {
            writer.WriteStringValue(value.Value.ToString(DateFormat));
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}

public class DateOnlyNonNullableJsonConverter : JsonConverter<DateOnly>
{
    private const string DateFormat = "yyyy-MM-dd";
    private static readonly DateTimeStyles DateTimeParseStyles =
        DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal;

    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var dateString = reader.GetString();
            if (!string.IsNullOrEmpty(dateString))
            {
                if (DateOnly.TryParseExact(dateString, DateFormat, out var date))
                {
                    return date;
                }

                if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeParseStyles, out var dateTime))
                {
                    return DateOnly.FromDateTime(dateTime);
                }

                if (DateOnly.TryParse(dateString, out date))
                {
                    return date;
                }
            }
        }

        return DateOnly.MinValue;
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(DateFormat));
    }
}
