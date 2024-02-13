using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Debricked.Models
{
    internal class GitHubRelease
    {
        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("tag_name")]
        public string TagName { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("assets")]
        public List<GithubAsset> Assets { get; set; }

        [JsonIgnore]
        public string Version { get { return Name.Replace("v", ""); } }
    }
}
