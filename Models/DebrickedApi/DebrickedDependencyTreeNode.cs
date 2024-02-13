using System.Text.Json.Serialization;
using System.Collections.Generic;

namespace Debricked.Models.DebrickedApi
{
    public class DebrickedDependencyTreeNode
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("notRelated")]
        public bool NotRelated { get; set; }

        [JsonPropertyName("vulnerable")]
        public bool Vulnerable { get; set; }

        [JsonPropertyName("large")]
        public bool Large { get; set; }

        [JsonPropertyName("children")]
        public List<DebrickedDependencyTreeNode> Children { get; set; } = new List<DebrickedDependencyTreeNode>();

        public string IntroducedThrough {  get; set; }

        public string Fix { get; set; } = "";

        [JsonIgnore] 
        public bool ShouldBeEnabled { get { return Children.Count > 0; } }
    }
}