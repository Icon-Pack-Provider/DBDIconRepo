using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IconPackAPI.Internal
{
    internal class DateTimeConversion : JsonConverter<DateTime>
    {
        public const string DateFormat = "yyyy-MM-dd HH:mm:ss";

        public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(DateTime);

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var muricaFormat = new CultureInfo("en-US");
            DateTime output = DateTime.MinValue;
            DateTime.TryParseExact(reader.GetString(), DateFormat,
                muricaFormat.DateTimeFormat, DateTimeStyles.AssumeUniversal,
                out output);
            return output;
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(DateFormat));
        }
    }
}