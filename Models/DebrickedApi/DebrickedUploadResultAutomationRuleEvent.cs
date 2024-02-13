using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace Debricked.Models.DebrickedApi
{
    internal class DebrickedUploadResultAutomationRuleEvent
    {
        [JsonPropertyName("dependency")]
        public string Dependency { get; set; }

        [JsonPropertyName("dependencyLink")]
        public string DependencyLink { get; set; }

        [JsonPropertyName("licenses")]
        public List<string> Licenses { get; set; }

        [JsonPropertyName("cve")]
        public string Cve { get; set; }

        [JsonPropertyName("cveLink")]
        public string CveLink { get; set; }

        [JsonPropertyName("cvss3")]
        public double Cvss3 { get; set; }

        [JsonPropertyName("cvss2")]
        public double Cvss2 { get; set; }


        public int GetDependencyId(Regex re)
        {
            var matches = re.Matches(this.DependencyLink);
            if (matches.Count >= 1 && matches[0].Groups.Count == 2)
            {
                return int.Parse(matches[0].Groups[1].Value);
            }
            else
            {
                throw new Exception("Unable to extract rule id from link " + this.DependencyLink);
            }
        }
    }
}
