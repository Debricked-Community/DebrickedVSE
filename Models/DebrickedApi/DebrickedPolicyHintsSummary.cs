using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Debricked.Models.DebrickedApi
{
    internal class DebrickedPolicyHintsSummary
    {
        [JsonPropertyName("failure")]
        public DebrickedPolicyHintsSummaryItem Failure { get; set; }

        [JsonPropertyName("warning")]
        public DebrickedPolicyHintsSummaryItem Warning { get; set; }

        [JsonPropertyName("notification")]
        public DebrickedPolicyHintsSummaryItem Notificatin { get; set; }

        internal class DebrickedPolicyHintsSummaryItem
        {
            [JsonPropertyName("count")]
            public int Count { get; set; }

            [JsonPropertyName("details")]
            public DebrickedPolicyHintsSummaryItemDetails Details { get; set; }


            internal class DebrickedPolicyHintsSummaryItemDetails
            {
                [JsonPropertyName("licenses")]
                public List<string> Licenses { get; set; }

                [JsonPropertyName("vulnerabilities")]
                public List<string> Vulnerabilities { get; set; }
            }
        }
    }
}
