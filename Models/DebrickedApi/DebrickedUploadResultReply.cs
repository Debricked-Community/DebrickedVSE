using Debricked.Models.Constants;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Debricked.Models.DebrickedApi
{
    internal class DebrickedUploadResultReply
    {
        [JsonPropertyName("vulnerabilitiesFound")]
        public int VulnerabilitiesFound { get; set; } = 0;

        [JsonPropertyName("unaffectedVulnerabilitiesFound")]
        public int UnaffectedVulnerabilitiesFound { get; set; }

        [JsonPropertyName("automationsAction")]
        public string AutomationsAction { get; set; }

        [JsonPropertyName("detailsUrl")]
        public string DetailsUrl { get; set; }

        [JsonPropertyName("automationRules")]
        public List<DebrickedUploadResultAutomationRule> AutomationRules { get; set; }


        public (int repositoryId, int commitId) getRepoAndCommitId()
        {
            Regex regex = new Regex(RegEx.DetailsLinkRegex);
            var matches = regex.Matches(this.DetailsUrl);
            if (matches.Count == 1 && matches[0].Groups.Count == 3)
            {
                return (int.Parse(matches[0].Groups[1].Value), int.Parse(matches[0].Groups[2].Value));
            }
            else
            {
                throw new Exception("Failed to parse repositoryId and commitId from cli output");
            }
        }

    }
}
