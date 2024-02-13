using System.Text.Json.Serialization;

namespace Debricked.Models.DebrickedApi
{
    internal class DebrickedSelectDependencyDataReply
    {
        [JsonPropertyName("selectPage")]
        public string SelectPage { get; set; }

        public string GetPurl()
        {
            return SelectPage.Replace("/select/package/", "");
        }
    }
}
