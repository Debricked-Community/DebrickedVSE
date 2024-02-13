using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Debricked.Converters
{
    internal class JsonIntegerConverter : JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                if(reader.TokenType == JsonTokenType.Null)
                {
                    return 0;
                }
                return reader.GetInt32();
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }
}
