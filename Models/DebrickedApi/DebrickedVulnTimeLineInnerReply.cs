using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Debricked.Models.DebrickedApi
{
    internal class DebrickedVulnTimeLineInnerReply
    {
        [JsonPropertyName("dependencies")]
        public List<DebrickedDependency.DebrickedDependencyName> DependencyNames { get; set; } = new List<DebrickedDependency.DebrickedDependencyName>();

        [JsonPropertyName("intervals")]
        public List<DebrickedVulnTimeLineInterval> Intervals { get; set; } = new List<DebrickedVulnTimeLineInterval>();
    }
}
