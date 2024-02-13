using System.Text.Json.Serialization;

namespace Debricked.Models.DebrickedApi
{
    public class DebrickedCommitIdentifier
    {
        [JsonPropertyName("repositoryId")]
        public int RepositoryId { get; set; }

        [JsonPropertyName("commitId")]
        public int CommitId { get; set; }
    }
}
