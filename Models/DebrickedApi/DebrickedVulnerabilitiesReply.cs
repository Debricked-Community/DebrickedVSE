using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Debricked.Models.DebrickedApi
{
    internal class DebrickedVulnerabilitiesReply : IDebrickedApiReply<DebrickedVulnerability>
    {
        [JsonPropertyName("vulnerabilities")]
        public List<DebrickedVulnerability> Vulnerabilities { get; set; } = new List<DebrickedVulnerability>();

        [JsonIgnore]
        public List<DebrickedVulnerability> Entities { get {  return Vulnerabilities; } }
    }
}
