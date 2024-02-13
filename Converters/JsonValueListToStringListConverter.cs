using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Debricked.Converters
{
    internal class JsonValueListToStringListConverter : JsonConverter<List<string>>
    {
        public override List<string> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if(reader.TokenType != JsonTokenType.StartArray)
            {
                throw new Exception("property must be an array");
            }
            reader.Read();

            List<string> result = new List<string>();
            while(reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType == JsonTokenType.Number)
                {
                    result.Add(reader.GetInt64().ToString());
                } 
                else if(reader.TokenType == JsonTokenType.True)
                {
                    result.Add("true");
                }
                else if (reader.TokenType == JsonTokenType.False)
                {
                    result.Add("false");
                }
                else
                {
                    result.Add(reader.GetString());
                }
                reader.Read();
            }
            return result;
        }

        public override void Write(Utf8JsonWriter writer, List<string> value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
