using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Debricked.Models.DebrickedApi
{
    internal class DebrickedVulnTimelineReply
    {
        [JsonPropertyName("timelines")]
        public List<DebrickedVulnTimeLineInnerReply> Timelines { get; set; } = new List<DebrickedVulnTimeLineInnerReply>();
    }
}
