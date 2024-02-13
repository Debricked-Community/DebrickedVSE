using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Debricked.Models.DebrickedApi
{
    internal class DebrickedRulesDataReply
    {
        [JsonPropertyName("desc")]
        public string Description { get; set; }

        [JsonPropertyName("repository")]
        public string Repository { get; set; }

        [JsonPropertyName("conditions")]
        public List<DebrickedRuleDataCondition> Conditions { get; set; }

        //lots more here that i dont need
    }
}
