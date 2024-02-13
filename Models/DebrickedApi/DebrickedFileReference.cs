using System.Text.Json.Serialization;

namespace Debricked.Models.DebrickedApi
{
    internal class DebrickedFileReference
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName ("name")]
        public string Name { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }
    }
}
