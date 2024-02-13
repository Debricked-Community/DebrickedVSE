using System.Text.Json.Serialization;

namespace Debricked.Models
{
    internal class GithubAsset
    {
        [JsonPropertyName("browser_download_url")]
        public string Url { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
}
