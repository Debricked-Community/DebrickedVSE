using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Debricked.Models.DebrickedApi
{
    internal class DebrickedRuleDataCondition
    {
        [JsonPropertyName("subject")]
        public string Subject { get; set; }

        [JsonPropertyName("op")]
        public string Operation { get; set; }

        [JsonPropertyName("conj")]
        public string Conjunction { get; set; }

        [JsonPropertyName("untriggeredOp")]
        public bool IsUntriggered { get; set; }

        [JsonPropertyName("andConjunction")]
        public bool IsAndConjunction { get; set; }

        [JsonPropertyName("value")]
        public List<string> Value { get; set; }
    }
}
