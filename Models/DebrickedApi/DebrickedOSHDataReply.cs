using Debricked.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Debricked.Models.DebrickedApi
{
    internal class DebrickedOSHDataReply
    {
        [JsonPropertyName("metrics")]
        [JsonConverter(typeof(JsonOshMetricsConverter))]
        public DebrickedOSHMetrics Metrics { get; set; }
    }

    //TODO this fails to parse if metrics is an empty array
    internal class DebrickedOSHMetrics
    {
        [JsonPropertyName("1")]
        public DebrickedOshScore ContributorsScore { get; set; }

        [JsonPropertyName("2")]
        public DebrickedOshScore PopularityScore { get; set; }

        [JsonPropertyName("3")]
        public DebrickedOshScore SecurityScore { get; set; }
    }

    internal class DebrickedOshScore
    {
        [JsonPropertyName("score")]
        public int Score { get; set; } = -1;
    }
}
