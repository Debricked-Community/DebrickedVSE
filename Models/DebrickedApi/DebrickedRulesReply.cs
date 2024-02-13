using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Debricked.Models.DebrickedApi
{
    internal class DebrickedRulesReply
    {
        [JsonPropertyName("ruleGroups")]
        public List<DebrickedRuleGroup> RuleGroups { get; set; }

        [JsonPropertyName("totalRules")]
        public int TotalRules { get; set; }

        [JsonPropertyName("isBitbucketIntegration")]
        public bool IsBitbucketIntegration { get; set; }
    }
}
