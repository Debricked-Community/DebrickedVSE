using Debricked.Converters;
using Debricked.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Debricked.Models.DebrickedApi
{
    internal class DebrickedPolicyHintsTriggeredRule
    {
        [JsonPropertyName("rule_id")]
        public int RuleId { get; set; }

        [JsonPropertyName("triggered_conditions")]
        public List<List<DebrickedPolicyHintsTriggeredRuleCondition>> TriggeredConditions { get; set; }

        [JsonPropertyName("actions")]
        public List<DebrickedPolicyHintsTriggeredRuleAction> Actions { get; set; }

        [JsonPropertyName("context")]
        public DebrickedPolicyHintsTriggeredRuleContext Context { get; set; }



        internal class DebrickedPolicyHintsTriggeredRuleCondition
        {
            [JsonPropertyName("subject")]
            public string Subject { get; set; }

            [JsonPropertyName("values")]
            [JsonConverter(typeof(JsonValueListToStringListConverter))]
            public List<string> Values { get; set; }


            public override bool Equals(object obj)
            {
                DebrickedPolicyHintsTriggeredRuleCondition c = obj as DebrickedPolicyHintsTriggeredRuleCondition;
                if(c == null) return false;
                if(this.Subject != c.Subject) return false;
                if(this.Values == null && c.Values!=null) { return false; }
                if(c.Values==null && this.Values!=null) { return false; }
                if(c.Values!=null && this.Values!=null) { bool he = c.GetHashCode() == this.GetHashCode(); return c.Values.EqualTo(this.Values); }
                return true;
            }

            public override int GetHashCode()
            {
                return this.Subject.GetHashCode() ^ this.Values.GetHashCode();
            }
        }

        internal class DebrickedPolicyHintsTriggeredRuleAction
        {
            [JsonPropertyName("action")]
            public string Action { get; set; }

            [JsonPropertyName("level")]
            public string Level { get; set; }
        }

        internal class DebrickedPolicyHintsTriggeredRuleContext
        {
            [JsonPropertyName("rule_context")]
            public string RuleContext { get; set; }

            [JsonPropertyName("context_id")]
            public int ContextId { get; set; }
        }
    }
}
