using Debricked.Models.Constants;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Windows.Navigation;

namespace Debricked.Models.DebrickedApi
{
    internal class DebrickedUploadResultAutomationRule
    {
        [JsonPropertyName("ruleDescription")]
        public string Description { get; set; }

        [JsonPropertyName("ruleLink")]
        public string Link { get; set; }

        [JsonPropertyName("hasCves")]
        public bool HasCves { get; set; }

        [JsonPropertyName("triggered")]
        public bool IsTriggered { get; set; }

        [JsonPropertyName("ruleActions")]
        public List<string> RuleActions { get; set; } = new List<string>();

        [JsonPropertyName("triggerEvents")]
        public List<DebrickedUploadResultAutomationRuleEvent> TriggerEvents { get; set; }

        public int GetId(Regex re)
        {
            var matches = re.Matches(this.Link);
            if (matches.Count >= 1 && matches[0].Groups.Count == 2)
            {
                return int.Parse(matches[0].Groups[1].Value);
            }
            else
            {
                throw new Exception("Unable to extract rule id from link " + this.Link);
            }
        }

    }
}
