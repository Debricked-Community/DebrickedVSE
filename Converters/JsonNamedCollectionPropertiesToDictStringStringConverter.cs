using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Debricked.Converters
{
    internal class JsonNamedCollectionPropertiesToDictStringStringConverter : JsonConverter<Dictionary<string, string>>
    {
        public override Dictionary<string, string> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new Exception("property must be an object");
            }
            reader.Read();

            Dictionary<string,string> result = new Dictionary<string, string>();

            while(reader.TokenType != JsonTokenType.EndObject)
            {
                string propertyName = reader.GetString();
                reader.Read();
                string value = reader.GetString();
                reader.Read();
                result.Add(propertyName, value);
            }

            return result;
        }

        public override void Write(Utf8JsonWriter writer, Dictionary<string, string> value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
