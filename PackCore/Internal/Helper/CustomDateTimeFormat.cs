using System.Text.Json;
using System.Text.Json.Serialization;

namespace IconPack.Helper;

public class CustomDateTimeFormat : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var muricaFormat = new System.Globalization.CultureInfo("en-US");
        DateTime.TryParseExact(reader.GetString(), "yyyy-MM-dd HH:mm:ss",
            muricaFormat.DateTimeFormat, System.Globalization.DateTimeStyles.AssumeUniversal, out DateTime output);
        return output;
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("yyyy-MM-dd HH:mm:ss"));
    }
}
