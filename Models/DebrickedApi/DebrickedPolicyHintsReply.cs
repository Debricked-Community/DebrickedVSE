using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Debricked.Models.DebrickedApi
{
    internal class DebrickedPolicyHintsReply
    {
        [JsonPropertyName("triggered")]
        public List<DebrickedPolicyHintsTriggeredRule> Triggered {  get; set; }

        [JsonPropertyName("passed")]
        public int Passed { get; set; }

        [JsonPropertyName("summary")]
        public DebrickedPolicyHintsSummary Summary { get; set; }
    }
}
