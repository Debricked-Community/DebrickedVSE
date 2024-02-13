using System.Text.Json.Serialization;

namespace Debricked.Models.DebrickedApi
{
    public class DebrickedRepositoryIdentifier
    {
        [JsonPropertyName("name")]
        public string RepositoryName { get; set; }

        [JsonPropertyName("id")]
        public int RepositoryId { get; set; }
    }
}
