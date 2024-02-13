using System;
using System.Text.Json.Serialization;
using System.Windows;

namespace Debricked.Models.DebrickedApi
{
    internal class DebrickedVulnRefSummary
    {
        [JsonPropertyName("category")]
        public string Category { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("explanation")]
        public string Explanation { get; set; }

        [JsonPropertyName("missing")]
        public bool IsMissing { get; set; } = true;

        [JsonPropertyName("link")]
        public string Link { get; set; }

        [JsonIgnore]
        public Visibility LinkImageVisibility { get { return String.IsNullOrEmpty(Link) ? Visibility.Hidden : Visibility.Visible; } }
    }
}
