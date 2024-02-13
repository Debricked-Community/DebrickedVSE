using System.Text.Json.Serialization;

namespace Debricked.Models.DebrickedApi
{
    internal class DebrickedGetSelectDependencyIdReply
    {
        [JsonPropertyName("dependencyId")]
        public int DependencyId { get; set; }
    }
}
