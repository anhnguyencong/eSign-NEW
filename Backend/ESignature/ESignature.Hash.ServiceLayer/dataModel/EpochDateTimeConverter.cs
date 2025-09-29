using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using Newtonsoft.Json;

public class EpochDateTimeConverter : JsonConverter<DateTime>
{
    public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer)
    {
        writer.WriteValue(new DateTimeOffset(value).ToUnixTimeMilliseconds());
    }

    public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Integer && reader.Value is long milliseconds)
        {
            return DateTimeOffset.FromUnixTimeMilliseconds(milliseconds).UtcDateTime;
        }

        throw new JsonSerializationException("Invalid timestamp format for DateTime.");
    }
}
