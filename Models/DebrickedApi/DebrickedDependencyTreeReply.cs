using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Debricked.Models.DebrickedApi
{
    internal class DebrickedDependencyTreeReply
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("trees")]
        public List<DebrickedDependencyTreeNode> Trees { get; set; } = new List<DebrickedDependencyTreeNode>();
    }
}
