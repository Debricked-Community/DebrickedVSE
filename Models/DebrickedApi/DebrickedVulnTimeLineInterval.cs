using System.Text.Json.Serialization;
using System.Windows.Media;

namespace Debricked.Models.DebrickedApi
{
    internal class DebrickedVulnTimeLineInterval
    {
        [JsonPropertyName("vulnerable")]
        public bool Vulnerable { get; set; }

        [JsonPropertyName("startVersion")]
        public string StartVersion { get; set; } = "";

        [JsonPropertyName("endVersion")]
        public string EndVersion { get; set; } = "";

        [JsonIgnore]
        public Brush Brush { get { return Vulnerable ? Brushes.Red : Brushes.Green; } }
    }
}
