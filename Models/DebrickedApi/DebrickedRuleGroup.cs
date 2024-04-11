using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Debricked.Models.DebrickedApi
{
    internal class DebrickedRuleGroup
    {
        [JsonPropertyName("repositoryIds")]
        public List<int> RepositoryIds { get; set; }

        [JsonPropertyName("ruleIds")]
        public List<int> RuleIds { get; set; }

        [JsonPropertyName("defaultRuleIds")]
        public HashSet<int> DefaultRuleIds { get; set; }

        [JsonPropertyName("desc")]
        public string Description { get; set; }

        [JsonPropertyName("active")]
        public bool Active { get; set; }

        [JsonPropertyName("updatedAt")]
        public long UpdatedAt { get; set; }

        [JsonPropertyName("updatedBy")]
        public string UpdatedBy { get; set; }

        [JsonPropertyName("updatedType")]
        public string UpdatedType { get; set; }

        [JsonPropertyName("selectedPolicyId")]
        public string SelectedPolicyId { get; set; }

        [JsonPropertyName("actions")]
        public HashSet<string> Actions { get; set; }

        [JsonPropertyName("conditions")]
        public List<DebrickedRuleDataCondition> Conditions { get; set; } = null;

        public int GetRuleId(int repositoryId)
        {
            if (RepositoryIds.IndexOf(repositoryId) != -1)
            {
                return RuleIds[RepositoryIds.IndexOf(repositoryId)];
            } else { return -1; }
        }

        public StringContent GetPutContent(List<DebrickedRuleDataCondition> conditions)
        {
            dynamic content = new ExpandoObject();
            content.conditions = conditions;
            content.repositoryIds = RepositoryIds;
            content.ruleIds = RuleIds;
            content.defaultRuleIds = DefaultRuleIds;
            var str = JsonSerializer.Serialize(content);
            return new StringContent(str);
        }
    }
}
