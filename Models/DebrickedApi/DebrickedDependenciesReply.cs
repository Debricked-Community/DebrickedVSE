using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Debricked.Models.DebrickedApi
{
    internal class DebrickedDependenciesReply : IDebrickedApiReply<DebrickedDependency>
    {
        [JsonPropertyName("dependencies")]
        public List<DebrickedDependency> Dependencies { get; set; } = new List<DebrickedDependency>();

        [JsonIgnore]
        public List<DebrickedDependency> Entities { get {  return Dependencies; } }
    }
}
