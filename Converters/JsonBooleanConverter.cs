using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Debricked.Converters
{
    internal class JsonBooleanConverter : JsonConverter<Boolean>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if(reader.TokenType == JsonTokenType.Null)
            {
                return false;
            }
            return reader.GetBoolean();
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
