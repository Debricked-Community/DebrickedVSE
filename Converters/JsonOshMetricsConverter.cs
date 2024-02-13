using Debricked.Models.DebrickedApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Debricked.Converters
{
    internal class JsonOshMetricsConverter : JsonConverter<DebrickedOSHMetrics>
    {
        public override DebrickedOSHMetrics Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                if(reader.TokenType == JsonTokenType.StartArray)
                {
                    reader.Read();
                    if(reader.TokenType == JsonTokenType.EndArray)
                    {
                        return new DebrickedOSHMetrics();
                    }
                }
                throw new Exception("property must be an object or empty array");
            }
            reader.Read();

            DebrickedOSHMetrics metrics = new DebrickedOSHMetrics();

            while (reader.TokenType != JsonTokenType.EndObject)
            {
                string propertyName = reader.GetString();
                reader.Read();
                switch (propertyName)
                {
                    case "1": metrics.ContributorsScore = GetOshScore(ref reader); break;
                    case "2": metrics.PopularityScore = GetOshScore(ref reader); break;
                    case "3": metrics.SecurityScore = GetOshScore(ref reader); break;
                    default:
                        break;
                }
                reader.Read();
            }
            return metrics;
        }

        private DebrickedOshScore GetOshScore(ref Utf8JsonReader reader)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new Exception("property must be an object");
            }
            reader.Read();

            DebrickedOshScore score = new DebrickedOshScore();

            while (reader.TokenType != JsonTokenType.EndObject)
            {
                string propertyName = reader.GetString();
                reader.Read();
                switch (propertyName)
                {
                    case "score": score.Score = reader.GetInt32(); break;
                    default:
                        break;
                }
                reader.Read();
            }
            return score;
        }

        public override void Write(Utf8JsonWriter writer, DebrickedOSHMetrics value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}
