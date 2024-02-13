using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Debricked.Models.DebrickedApi
{
    internal class DebrickedVulnRefSummaryReply
    {
        [JsonPropertyName("cwe")]
        public DebrickedVulnRefSummary Cwe { get; set; } = new DebrickedVulnRefSummary();

        [JsonPropertyName("x100")]
        public DebrickedVulnRefSummary Npm { get; set; } = new DebrickedVulnRefSummary();

        [JsonPropertyName("x101")]
        public DebrickedVulnRefSummary GitHub { get; set; } = new DebrickedVulnRefSummary();

        [JsonPropertyName("x102")]
        public DebrickedVulnRefSummary Nvd { get; set; } = new DebrickedVulnRefSummary();

        [JsonPropertyName("x103")]
        public DebrickedVulnRefSummary FriendsOfPhp { get; set; } = new DebrickedVulnRefSummary();

        [JsonPropertyName("x104")]
        public DebrickedVulnRefSummary CSharpAnnouncement { get; set; } = new DebrickedVulnRefSummary();

        [JsonPropertyName("x106")]
        public DebrickedVulnRefSummary PyPa { get; set; } = new DebrickedVulnRefSummary();

        [JsonPropertyName("x107")]
        public DebrickedVulnRefSummary GoVd { get; set; } = new DebrickedVulnRefSummary();

        [JsonPropertyName("x108")]
        public DebrickedVulnRefSummary Debricked { get; set; } = new DebrickedVulnRefSummary();

        [JsonPropertyName("x109")]
        public DebrickedVulnRefSummary GLAD { get; set; } = new DebrickedVulnRefSummary();

        [JsonPropertyName("x110")]
        public DebrickedVulnRefSummary GLAD2 { get; set; } = new DebrickedVulnRefSummary();

        public List<DebrickedVulnRefSummary> GetPresentAsList()
        {
            List<DebrickedVulnRefSummary> result = new List<DebrickedVulnRefSummary>();
            if (!Cwe.IsMissing) result.Add(Cwe);
            if (!Npm.IsMissing) result.Add(Npm);
            if (!GitHub.IsMissing) result.Add(GitHub);
            if (!Nvd.IsMissing) result.Add(Nvd);
            if (!FriendsOfPhp.IsMissing) result.Add(FriendsOfPhp);
            if (!CSharpAnnouncement.IsMissing) result.Add(CSharpAnnouncement);
            if (!PyPa.IsMissing) result.Add(PyPa);
            if (!GoVd.IsMissing) result.Add(GoVd);
            if (!Debricked.IsMissing) result.Add(Debricked);
            if (!GLAD.IsMissing) result.Add(GLAD);
            if (!GLAD2.IsMissing) result.Add(GLAD2);
            return result;
        }
    }
}
