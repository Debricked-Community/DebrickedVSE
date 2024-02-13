using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Debricked.Models.DebrickedApi
{
    internal class DebrickedDependencyInfoReply
    {
        [JsonPropertyName("licenses")]
        public List<string> Licenses { get; set; }

        [JsonPropertyName("oshDependencyId")]
        public int SelectId { get; set; }

        [JsonPropertyName("contributors")]
        public int ContributorsScore { get; set; }

        [JsonPropertyName("popularity")]
        public int PopularityScore { get; set; }
    }
}
