using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Debricked.Models.DebrickedApi
{
    internal class DebrickedLicensesReply : IDebrickedApiReply<DebrickedLicense>
    {
        [JsonPropertyName("licenses")]
        public List<DebrickedLicense> Licenses { get; set; } = new List<DebrickedLicense>();

        [JsonIgnore]
        public List<DebrickedLicense> Entities { get {  return Licenses; } }
    }
}
