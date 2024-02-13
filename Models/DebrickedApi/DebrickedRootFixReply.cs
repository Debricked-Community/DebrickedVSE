using Debricked.Converters;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Debricked.Models.DebrickedApi
{
    internal class DebrickedRootFixReply : IDebrickedApiReply<KeyValuePair<string, string>>
    {
        [JsonPropertyName("rootFixesCount")]
        public int RootFixesCount { get; set; }

        [JsonPropertyName("fixes")]
        [JsonConverter(typeof(JsonNamedCollectionPropertiesToDictStringStringConverter))]
        public Dictionary<string,string> fixes { get; set; } = new Dictionary<string, string>();

        [JsonIgnore]
        public Dictionary<string, string> RootFixes { get { return fixes.ToDictionary(x => x.Key, x => x.Value); } }

        [JsonPropertyName("isReady")]
        public bool IsReady { get; set; }

        public List<KeyValuePair<string, string>> Entities { get { return fixes.ToList(); } }
    }
}
